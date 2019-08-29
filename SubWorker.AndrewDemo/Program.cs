using SchedulingPractice.Core;
using System.Linq;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;

namespace SubWorker.AndrewDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            var host = new HostBuilder()
                .ConfigureServices((context, services) =>
                {
                    services.AddHostedService<AndrewSubWorkerBackgroundService>();
                })
                .Build();

            using (host)
            {
                host.Start();
                host.WaitForShutdown();
            }
        }
    }

    public class AndrewSubWorkerBackgroundService : BackgroundService
    {
        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Delay(1);

            using (JobsRepo repo = new JobsRepo())
            {
                while(stoppingToken.IsCancellationRequested == false)
                {
                    bool empty = true;
                    foreach(var job in repo.GetReadyJobs())
                    {
                        if (stoppingToken.IsCancellationRequested == true) goto shutdown;

                        if (repo.AcquireJobLock(job.Id))
                        {
                            repo.ProcessLockedJob(job.Id);
                            Console.Write(".");
                        }
                        empty = false;
                    }
                    if (empty == false) continue;

                    try
                    {
                        await Task.Delay(10000, stoppingToken);
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
