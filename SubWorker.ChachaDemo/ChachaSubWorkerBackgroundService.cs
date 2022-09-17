using System;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using SchedulingPractice.Core;

namespace SubWorker.ChachaDemo
{
    public class ChachaSubWorkerBackgroundService : BackgroundService
    {
        private readonly int _channelCount = Environment.ProcessorCount;
        private readonly Channel<JobInfo>[] _channels;

        public ChachaSubWorkerBackgroundService() {
            _channels = new Channel<JobInfo>[_channelCount];
            for (var i = 0; i < _channelCount; i++) {
                _channels[i] = Channel.CreateUnbounded<JobInfo>();
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
            await Task.Delay(1, stoppingToken);
            try {
                var getJobs = Task.Run(async () => { await GetJobs(stoppingToken); }, stoppingToken);
                Parallel.For((long)0, _channels.Length, new ParallelOptions() {
                    MaxDegreeOfParallelism = Environment.ProcessorCount,
                    CancellationToken = stoppingToken
                }, async i => {
                    while (!stoppingToken.IsCancellationRequested) {
                        try {
                            var item = await _channels[i].Reader.ReadAsync(stoppingToken);
                            await ProcessJob(item, stoppingToken);
                        }
                        catch {
                            Console.WriteLine($"channel #{i} exit.");
                        }
                    }
                });
                await Task.WhenAny(getJobs, Task.Delay(Timeout.Infinite, stoppingToken));
            }
            catch {
                Console.WriteLine("Application shut down.");
            }
        }

        private async Task GetJobs(CancellationToken cts) {
            using var jobRepo = new JobsRepo();
            while (!cts.IsCancellationRequested) {
                var jobs = jobRepo.GetReadyJobs(JobSettings.MinPrepareTime).ToList();
                for (var i = 0; i < jobs.Count; i++) {
                    var index = i % _channelCount;
                    await _channels[index].Writer.WriteAsync(jobs[i], cts);
                }

                await Task.Delay(JobSettings.MinPrepareTime, cts);
            }
        }

        private async Task ProcessJob(JobInfo jobInfo, CancellationToken cts) {
            using var jobsRepo = new JobsRepo();
            var now = DateTime.Now;
            if (jobInfo.RunAt > now)
                try {
                    await Task.Delay(jobInfo.RunAt.Subtract(now), cts);
                }
                catch {
                    Console.WriteLine($"X - Leave unlock job. job id {jobInfo.Id}");
                }

            if (jobsRepo.GetJob(jobInfo.Id).State != 0) return;
            if (jobsRepo.AcquireJobLock(jobInfo.Id))
                jobsRepo.ProcessLockedJob(jobInfo.Id);
            Console.WriteLine("O");
        }
    }
}