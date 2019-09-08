using Microsoft.Extensions.Hosting;
using SchedulingPractice.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SubWorker.AndrewDemo
{
    public class AndrewSubWorkerBackgroundService : BackgroundService
    {
        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Delay(1);

            using (JobsRepo repo = new JobsRepo())
            {
                while (stoppingToken.IsCancellationRequested == false)
                {
                    bool empty = true;
                    foreach (var job in repo.GetReadyJobs())
                    {
                        if (stoppingToken.IsCancellationRequested == true) goto shutdown;

                        if (repo.AcquireJobLock(job.Id))
                        {
                            repo.ProcessLockedJob(job.Id);
                            Console.Write("O");
                        }
                        else
                        {
                            Console.Write("X");
                        }
                        empty = false;
                    }
                    if (empty == false) continue;

                    try
                    {
                        await Task.Delay(JobSettings.MinPrepareTime, stoppingToken);
                        Console.Write("_");
                    }
                    catch (TaskCanceledException) { break; }
                }
            }

        shutdown:
            Console.WriteLine($"- shutdown event detected, stop worker service...");
        }
    }

}
