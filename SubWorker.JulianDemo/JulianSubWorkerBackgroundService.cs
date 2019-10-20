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
    public class JulianSubWorkerBackgroundService : BackgroundService
    {
        private const int maxChannelNumber = 5;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var channels = new List<Channel<JobInfo>>();
            for (int i = 0; i < maxChannelNumber; i++)
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
                    var newJobs = repo.GetReadyJobs(TimeSpan.FromSeconds(10)).ToList();

                    foreach (var job in newJobs)
                    {
                        var done = false;
                        while (!done)
                        {
                            foreach (var channel in channels)
                            {
                                var writer = channel.Writer;
                                if (writer.WaitToWriteAsync(stoppingToken).Result)
                                {
                                    writer.TryWrite(job);
                                    done = true;
                                    break;
                                }
                            }
                        }
                    }

                    try
                    {
                        await Task.Delay(JobSettings.MinPrepareTime, stoppingToken);
                        Console.Write("_");
                    }
                    catch (TaskCanceledException)
                    {
                        break;
                    }
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