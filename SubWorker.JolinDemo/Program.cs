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
                    string connectString = null;// @"Data Source=localhost\SQLEXPRESS01;Initial Catalog=JobsDB;Integrated Security=True;Pooling=False";
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
        private Scheduler _scheduler;

        public JolinSubWorkerBackgroundService(string connectString)
        {
            _scheduler = new Scheduler(connectString, 2);
        }

        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Delay(1);

            while (stoppingToken.IsCancellationRequested == false)
            {
                try
                {
                    await Task.Delay(60 * 1000, stoppingToken);
                }
                catch (Exception ex)
                {
                    this._scheduler.Stop();
                }
            }

            Console.WriteLine($"- shutdown event detected, stop worker service...");
        }
    }
}
