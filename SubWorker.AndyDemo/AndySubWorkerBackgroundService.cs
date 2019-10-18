using Microsoft.Extensions.Hosting;
using SchedulingPractice.Core;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace SubWorker.AndyDemo
{
    public class AndySubWorkerBackgroundService : BackgroundService
    {
        private BlockingCollection<JobInfo> queue = new BlockingCollection<JobInfo>();

        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Delay(1);

            List<Thread> threads = new List<Thread>();

            for (int i = 0; i < 5; i++)
            {
                Thread T = new Thread(Worker);
                threads.Add(T);
                T.Start();
            }


            using (JobsRepo repo = new JobsRepo())
            {
                while (stoppingToken.IsCancellationRequested == false)
                {
                    DateTime LastQueryTime = DateTime.Now;

                    foreach (JobInfo job in repo.GetReadyJobs(JobSettings.MinPrepareTime))
                    {
                        if (stoppingToken.IsCancellationRequested == true)
                        {
                            break;
                        }

                        if (repo.GetJob(job.Id).State == 0 && repo.AcquireJobLock(job.Id))
                        {
                            if (job.RunAt > DateTime.Now)
                            {
                                await Task.Delay(job.RunAt - DateTime.Now);
                            }
                            queue.Add(job);
                        }

                    }

                    if (stoppingToken.IsCancellationRequested == false
                        && LastQueryTime.Add(JobSettings.MinPrepareTime) > DateTime.Now
                    )
                    {
                        await Task.Delay(LastQueryTime.Add(JobSettings.MinPrepareTime) - DateTime.Now);
                    }

                }

            }


            queue.CompleteAdding();

            threads.ForEach(T =>
            {
                T.Join();
            });

            Console.WriteLine($"- shutdown event detected, stop worker service...");
        }
    
        private void Worker()
        {
            using (JobsRepo repo = new JobsRepo())
            {
                foreach (JobInfo job in this.queue.GetConsumingEnumerable())
                {
                    repo.ProcessLockedJob(job.Id);
                }
            }


        }
    }
}

