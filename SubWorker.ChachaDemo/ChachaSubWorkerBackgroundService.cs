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
        private const int ChannelCount = 5;
        private Channel<JobInfo>[] channels;

        public ChachaSubWorkerBackgroundService() {
            channels = new Channel<JobInfo>[ChannelCount];
            for (var i = 0; i < ChannelCount; i++) {
                channels[i] = Channel.CreateUnbounded<JobInfo>();
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
            try {
                var getJob = Task.Run(async () => { await GetJob(stoppingToken); }, stoppingToken);
                Parallel.For(0, channels.Length, new ParallelOptions() {
                    MaxDegreeOfParallelism = Environment.ProcessorCount,
                    CancellationToken = stoppingToken
                }, async i => {
                    try {
                        await foreach (var item in channels[i].Reader.ReadAllAsync(stoppingToken)) {
                            await ProcessJob(item, stoppingToken);
                        }
                    }
                    catch {
                        Console.WriteLine("shut down");
                    }
                });

                await Task.WhenAny(getJob);
            }

            catch {
                Console.WriteLine("shut down");
            }
        }

        private async Task GetJob(CancellationToken cts) {
            using var jobRepo = new JobsRepo();
            while (!cts.IsCancellationRequested) {
                var jobs = jobRepo.GetReadyJobs(JobSettings.MinPrepareTime).ToList();
                for (var i = 0; i < jobs.Count; i++) {
                    var index = i % ChannelCount;
                    await channels[index].Writer.WriteAsync(jobs[i], cts);
                }

                await Task.Delay(JobSettings.MinPrepareTime, cts);
            }
        }

        private async Task ProcessJob(JobInfo jobInfo, CancellationToken cts) {
            using var jobsRepo = new JobsRepo();
            var now = DateTime.Now;
            if (jobInfo.RunAt > now)
                await Task.Delay(jobInfo.RunAt.Subtract(now), cts);
            if (jobsRepo.GetJob(jobInfo.Id).State != 0) return;
            if (jobsRepo.AcquireJobLock(jobInfo.Id))
                jobsRepo.ProcessLockedJob(jobInfo.Id);
        }
    }
}