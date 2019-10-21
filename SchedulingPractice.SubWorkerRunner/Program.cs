using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace SchedulingPractice.SubWorkerRunner
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 4)
            {
                Console.WriteLine("Usage: SubWorkerRunner.exe [mode] [min_timeout] [max_timeout] [total_timeout]");
                return;
            }

            string mode = args[0];
            int min_timeout = int.Parse(args[1]);
            int max_timeout = int.Parse(args[2]);
            int total_timeout = int.Parse(args[3]);

            Console.WriteLine($"Exec Parameter(s):");
            Console.WriteLine($"- Mode:          {mode}");
            Console.WriteLine($"- Min-Timeout:   {min_timeout}");
            Console.WriteLine($"- Max-Timeout:   {max_timeout}");
            Console.WriteLine($"- Total-Timeout: {total_timeout}");

            var host = new HostBuilder()
                .ConfigureServices((context, services) =>
                {
                    //services.AddHostedService<AndrewSubWorkerBackgroundService>();
                    //services.AddHostedService<AndrewSubWorkerBackgroundService2>();
                    if (mode == "demo") services.AddHostedService<SubWorker.AndrewDemo.AndrewSubWorkerBackgroundService>();
                    if (mode == "andrew0928") services.AddHostedService<SubWorker.AndrewDemo.AndrewSubWorkerBackgroundService2>();
                    if (mode == "andy19900208") services.AddHostedService<SubWorker.AndyDemo.AndySubWorkerBackgroundService>();
                    if (mode == "julian-chu") services.AddHostedService<SubWorker.JulianDemo.JulianSubWorkerBackgroundService>();
                    if (mode == "borischin") services.AddHostedService<SubWorker.BorisDemo.BorisSubWorkerBackgroundService>();
                    if (mode == "levichen") services.AddHostedService<SubWorker.LeviDemo.LeviSubWorkerBackgroundService>();
                    if (mode == "jwchen-dev") services.AddHostedService<SubWorker.JWDemo.JWSubWorkerBackgroundServiceV2>();
                    //if (mode == "toyo0103") services.AddHostedService<SubWorker.JolinDemo.JolinSubWorkerBackgroundService>();
                    if (mode == "toyo0103")
                    {
                        services.AddSingleton<IHostedService, SubWorker.JolinDemo.JolinSubWorkerBackgroundService>(sp =>
                        {
                            return new SubWorker.JolinDemo.JolinSubWorkerBackgroundService(null);
                        });
                    }
                    if (mode == "acetaxxxx") services.AddHostedService<HankTestTwo.HankTestProgram>();
                })
                .Build();

            Stopwatch timer = new Stopwatch();
            timer.Restart();
            Random rnd = new Random();

            using (host)
            {
                do
                {
                    int timeout = 0; 
                    int delay_start = rnd.Next(10);

                    if (min_timeout == 0 && max_timeout == 0)
                    {
                        timeout = total_timeout;
                    }
                    else
                    {
                        timeout = rnd.Next(min_timeout, max_timeout);
                    }

                    Console.WriteLine($"-----------------------------------------------------------------");
                    Console.WriteLine($"- Delay Start:   {delay_start} sec");
                    Console.WriteLine($"- Round Timeout: {timeout} sec");
                    Console.WriteLine($"-----------------------------------------------------------------");

                    Thread.Sleep(TimeSpan.FromSeconds(delay_start));

                    host.Start();

                    Task.Delay(TimeSpan.FromSeconds(timeout)).Wait();
                    host.StopAsync(TimeSpan.FromSeconds(30)).Wait();
                        
                    Console.WriteLine("- system shutdown...");
                }
                while (timer.Elapsed.TotalSeconds < total_timeout);
            }

        }
    }
}
