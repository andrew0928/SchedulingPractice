using System;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace SubWorker.JulianDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            var host = new HostBuilder()
                .ConfigureServices((context, services) =>
                {
                    services.AddHostedService<JulianSubWorkerBackgroundService>();
                    //services.AddHostedService<AndrewSubWorker2BackgroundService>();
                })
                .Build();

            using (host)
            {
                host.Start();
                host.WaitForShutdown();
            }
        }
    }
}