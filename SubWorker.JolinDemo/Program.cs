using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SchedulingPractice.Core;

namespace SubWorker.JolinDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            var host = new HostBuilder()
                .ConfigureServices((context, services) =>
                {
                    string connectString = @"Data Source=localhost\SQLEXPRESS01;Initial Catalog=JobsDB;Integrated Security=True;Pooling=False";
                    services.AddSingleton<IHostedService,JolinSubWorkerBackgroundService>(sp=> 
                    {
                        return new JolinSubWorkerBackgroundService(connectString);
                    });
                })
                .Build();

            using (host)
            {
                host.Start();
                host.WaitForShutdown();
            }
        }
    }

    public class JolinSubWorkerBackgroundService : BackgroundService
    {
        private int _intervalSecond = 10;

        private Scheduler _scheduler;

        private string _connectString;
        public JolinSubWorkerBackgroundService(string connectString)
        {
            _scheduler = new Scheduler(connectString);
            _connectString = connectString;
        }

        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Delay(1);

            while (stoppingToken.IsCancellationRequested == false)
            {
                using (JobsRepo repo = new JobsRepo(_connectString))
                {
                    var jobs = repo.GetReadyJobs(TimeSpan.FromSeconds(_intervalSecond));

                    if (jobs.Count() > 0)
                        this._scheduler.SetSchedule(jobs);

                    try
                    {
                        await Task.Delay(_intervalSecond * 1000, stoppingToken);
                    }
                    catch (Exception ex)
                    {
                        this._scheduler.Stop();
                    }
                }
            }

            Console.WriteLine($"- shutdown event detected, stop worker service...");
        }
    }
}
