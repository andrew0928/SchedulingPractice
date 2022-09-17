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
            await Task.Delay(1,stoppingToken);
            try {
                Task.Run(async () => { await GetJobs(stoppingToken); }, stoppingToken);
                Parallel.For(0, _channels.Length, new ParallelOptions() {
                    MaxDegreeOfParallelism = Environment.ProcessorCount,
                    CancellationToken = stoppingToken
                }, async i => {
                    try {
                        await foreach (var item in _channels[i].Reader.ReadAllAsync(stoppingToken)) {
                            await ProcessJob(i, item, stoppingToken);
                        }
                    }
                    catch {
                        Console.WriteLine("shut down");
                    }
                });
            }

            catch {
                Console.WriteLine("shut down");
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

        private async Task ProcessJob(int channelIndex, JobInfo jobInfo, CancellationToken cts) {
            using var jobsRepo = new JobsRepo();
            if (jobsRepo.GetJob(jobInfo.Id).State != 0) return;
            if (jobsRepo.AcquireJobLock(jobInfo.Id) == false) return;
            var now = DateTime.Now;
            if (jobInfo.RunAt > now) {
                try {
                    Console.WriteLine($"Chanel index is {channelIndex}, job id is {jobInfo.Id}");
                    await Task.Delay(jobInfo.RunAt.Subtract(now), cts);
                }
                catch (Exception e) {
                    Console.WriteLine(
                        $"Application shout down, early process job, job id is {jobInfo.Id} {e.ToString()}");
                    jobsRepo.ProcessLockedJob(jobInfo.Id);
                    // goto shoutDown(e);
                }
            }
            else {
                Console.WriteLine($"Delay {jobInfo.Id}");
                jobsRepo.ProcessLockedJob(jobInfo.Id);
            }

            jobsRepo.ProcessLockedJob(jobInfo.Id);
        }
    }
}