using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SchedulingPractice.Core;

namespace SubWorker.BorisDemo
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var host = new HostBuilder()
                .ConfigureServices((context, services) =>
                {
                    services.AddHostedService<BorisSubWorkerBackgroundService>();
                })
                .Build();

            using (host)
            {
                await host.StartAsync();
                await host.WaitForShutdownAsync();
            }
        }
    }

    public class BorisSubWorkerBackgroundService : BackgroundService
    {
        const int ThreadCnt = 5;

        static long queueSize = 0;

        BlockingCollection<JobInfo> _queue = new BlockingCollection<JobInfo>(ThreadCnt);
        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var threads = Enumerable.Range(1, ThreadCnt).Select(tid => new Thread(() =>
            {
                using (JobsRepo jr = new JobsRepo())
                {
                    while (!_queue.IsCompleted)
                    {
                        JobInfo job = null;
                        try
                        {
                            Console.WriteLine($"Thread({tid}) is waiting for a job...");
                            _queue.TryTake(out job, 3000);
                        }
                        catch (OperationCanceledException ex)
                        {
                            Console.WriteLine($"Thread({tid}) stopped waiting for a job...");
                        }

                        if (job != null)
                        {
                            var diff = (int)(job.RunAt - DateTime.Now).TotalMilliseconds;
                            if (diff > 0)
                            {
                                Console.WriteLine($"Thread({tid}) is waiting to run job({job.Id})...");
                                Thread.Sleep(diff);
                            }

                            Console.WriteLine($"Thread({tid}) is running job({job.Id})...");
                            jr.ProcessLockedJob(job.Id);
                            Console.WriteLine($"Thread({tid}) processed job({job.Id}).");
                            //Console.Write("O");
                        }
                    }
                }
            })).ToArray();

            foreach (var t in threads) t.Start();

            using (JobsRepo repo = new JobsRepo())
            {
                while (true)
                {
                    foreach (var job in repo.GetReadyJobs(JobSettings.MinPrepareTime))
                    {
                        if (stoppingToken.IsCancellationRequested) break;

                        if (repo.AcquireJobLock(job.Id))
                        {
                            _queue.Add(job);
                        }
                    }

                    if (stoppingToken.IsCancellationRequested)
                        break;

                    try
                    {
                        await Task.Delay(JobSettings.MinPrepareTime, stoppingToken);
                    }
                    catch (TaskCanceledException ex) { }
                    catch (OperationCanceledException ex) { }
                }
            }

            _queue.CompleteAdding();

            Console.WriteLine($"- shutdown event detected, stop worker service...");

            Console.WriteLine($"- queued job count({_queue.Count}) before");

            await Task.Run(() => { foreach (var t in threads) t.Join(); });

            Console.WriteLine($"- queued job count({_queue.Count}) after");

            Console.WriteLine($"- worker service stopped.");
        }
    }

}
