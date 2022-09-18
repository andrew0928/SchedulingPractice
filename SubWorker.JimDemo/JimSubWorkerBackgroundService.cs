using Microsoft.Extensions.Hosting;
using SchedulingPractice.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SubWorker.JimDemo
{
    public class JimSubWorkerBackgroundService : BackgroundService
    {
        private readonly BlockingCollection<JobInfo> _queue = new BlockingCollection<JobInfo>();
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Delay(1);
            //平行處理
            Thread[] threads = new Thread[5];
            for (int i = 0; i < threads.Length; i++)
            {
                threads[i] = new Thread(Worker);
                threads[i].Start();
            }


            Stopwatch timer = new Stopwatch();

            using (JobsRepo repo = new JobsRepo())
            {
                if (!stoppingToken.IsCancellationRequested)
                {
                    timer.Restart();
                    var allReadyJobs = repo.GetReadyJobs();
                    if (allReadyJobs != null)
                    {
                        foreach (var allReadyJob in allReadyJobs)
                        {
                            if (!stoppingToken.IsCancellationRequested)
                            {
                                //降低Lock次數
                                if (repo.GetJob(allReadyJob.Id).State != 0)
                                {
                                    Console.Write("%");
                                    continue;
                                }
                                //鎖定
                                if (!repo.AcquireJobLock(allReadyJob.Id))
                                {
                                    Console.Write("X");
                                    continue;
                                }
                                _queue.Add(allReadyJob);
                            }
                            else
                            {
                                goto shutdown;
                            }
                        }

                        try
                        {
                            await Task.Delay((int)Math.Max((JobSettings.MinPrepareTime - timer.Elapsed).TotalMilliseconds, 0), stoppingToken);
                        }
                        catch (TaskCanceledException)
                        {
                            goto shutdown;
                        }
                    }
                }
            shutdown:
                _queue.CompleteAdding();
            }
            foreach (var thead in threads)
            {
                thead.Join();
            }
            Console.WriteLine($"- shutdown event detected, stop worker service...");
        }
        private async void Worker()
        {
            using (JobsRepo repo = new JobsRepo())
            {
                foreach (JobInfo job in this._queue.GetConsumingEnumerable())
                {
                    //判斷Lock之後，是否可立即執行
                    if (job.RunAt > DateTime.Now)
                    {
                        Console.Write("@");
                        await Task.Delay(job.RunAt - DateTime.Now);
                        continue;
                    }
                    repo.ProcessLockedJob(job.Id);
                    Console.Write("O");
                }
            }


        }
    }
}
