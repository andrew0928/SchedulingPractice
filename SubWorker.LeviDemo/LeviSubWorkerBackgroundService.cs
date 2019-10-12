using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using SchedulingPractice.Core;

namespace SubWorker.LeviDemo
{
    public class LeviSubWorkerBackgroundService : BackgroundService
    {
        private const int IntervalSeconds = 10;
        private const int FetchTasksIntervalSeconds = 5 * 1000;
        private const string ConnectionString = "Server=.;Database=master;user id=SA;password=Levi123456";
        private readonly SimpleThreadPool _stp;

        public LeviSubWorkerBackgroundService()
        {
            _stp = new SimpleThreadPool(5);

            Console.WriteLine("In Levi's Background Service'");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Delay(1, stoppingToken);

            using (var repo = new JobsRepo(ConnectionString))
            {
                while (stoppingToken.IsCancellationRequested == false)
                {
//                    var isGetJobs = false;
                    Console.WriteLine("Fetch Jobs ...");
                    var jobs = repo.GetReadyJobs(TimeSpan.FromSeconds(IntervalSeconds));

                    foreach (var job in jobs)
                    {
                        if (stoppingToken.IsCancellationRequested) goto shutdown;

                        Console.WriteLine($"Put Job {job.Id} to queue");
                        _stp.QueueUserWorkerItem(job);
//                        isGetJobs = true;
                    }

//                    if (isGetJobs == false) continue;

                    try
                    {
                        await Task.Delay(TimeSpan.FromSeconds(IntervalSeconds), stoppingToken);
                        Console.WriteLine("_");
                    }
                    catch (TaskCanceledException)
                    {
                        _stp.EndPool();
                        break;
                    }
                }
            }

            shutdown:
            Console.WriteLine("In Levi's BackgroundService'");
            Console.WriteLine("- shutdown event detected, stop worker service...");
        }
    }
}