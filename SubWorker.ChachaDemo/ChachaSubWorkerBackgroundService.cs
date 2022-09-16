using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using SchedulingPractice.Core;

namespace SubWorker.ChachaDemo
{
    public class ChachaSubWorkerBackgroundService : BackgroundService
    {
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var channels = Channel.CreateUnbounded<JobInfo>();
            try
            {
                var getJob = Task.Run(() => { GetJob(channels, stoppingToken); }, stoppingToken).ConfigureAwait(false);
                await foreach (var item in channels.Reader.ReadAllAsync(stoppingToken))
                {
                   await ProcessJob(item);
                }
                await getJob;
            }

            catch
            {
                // Task.WaitAny(GetJob(channels, stoppingToken), Task.Delay(JobSettings.MinPrepareTime));
                Console.WriteLine("Shut down...");
            }
        }

        private void GetJob(Channel<JobInfo> channel, CancellationToken cts)
        {
            using var jobsRepo = new JobsRepo();
            while (!cts.IsCancellationRequested)
            {
                var jobs = jobsRepo.GetReadyJobs(TimeSpan.FromSeconds(10));
                foreach (var job in jobs)
                {
                    channel.Writer.TryWrite(job);
                }
            }
        }

        private async Task ProcessJob(JobInfo jobInfo)
        {
            using var jobsRepo = new JobsRepo();
            var now = DateTime.Now;
            if (jobInfo.RunAt > now)
                await Task.Delay(jobInfo.RunAt.Subtract(now));
            if (jobsRepo.AcquireJobLock(jobInfo.Id))
                jobsRepo.ProcessLockedJob(jobInfo.Id);
        }
    }
}