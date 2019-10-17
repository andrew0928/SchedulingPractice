using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using SchedulingPractice.Core;

namespace SubWorker.JulianDemo
{
    internal class JulianSubWorkerBackgroundService : BackgroundService
    {
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var channels = new List<Channel<JobInfo>>();
            for (int i = 0; i < 5; i++)
            {
                var ch = Channel.CreateBounded<JobInfo>(new BoundedChannelOptions(1)
                    {SingleWriter = true, SingleReader = true, AllowSynchronousContinuations = true});
                channels.Add(ch);
            }

            foreach (var ch in channels)
            {
                DoJob(ch);
            }

            using (JobsRepo repo = new JobsRepo())
            {
                while (stoppingToken.IsCancellationRequested == false)
                {
                    var newJob = repo.GetReadyJobs(TimeSpan.FromSeconds(10)).ToList();
                    
                }
            }
            
            
        }

        private static async Task DoJob(ChannelReader<JobInfo> reader)
        {
            using (JobsRepo repo = new JobsRepo())
            {
                while (await reader.WaitToReadAsync())
                {
                    while (reader.TryRead(out JobInfo job))
                    {
                        Task.Run(async () =>
                        {
                            var now = DateTime.Now;
                            if (job.RunAt > now)
                            {
                                await Task.Delay(job.RunAt.Subtract(now));
                            }
                            
                            if (repo.AcquireJobLock(job.Id))
                            {
                                repo.ProcessLockedJob(job.Id);
                                Console.WriteLine("O");
                            }
                            else
                            {
                                Console.WriteLine("X");
                            }
                        }).Wait();
                    }

                    if (reader.Completion.IsCompleted) break;
                }
            }
        }
    }
}