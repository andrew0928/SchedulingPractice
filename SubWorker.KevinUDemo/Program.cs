using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SchedulingPractice.Core;

namespace SubWorker.KevinUDemo
{/* 5 instances
  Jobs Scheduling Metrics:

--(action count)----------------------------------------------
- CREATE:             873
- ACQUIRE_SUCCESS:    873
- ACQUIRE_FAILURE:    739
- COMPLETE:           873
- QUERYJOB:           4375
- QUERYLIST:          90

--(state count)----------------------------------------------
- COUNT(CREATE):      0
- COUNT(LOCK):        0
- COUNT(COMPLETE):    873

--(statistics)----------------------------------------------
- DELAY(Average):     160
- DELAY(Stdev):       171.2532911466342

--(test result)----------------------------------------------
- Complete Job:       True, 873 / 873
- Delay Too Long:     0
- Fail Job:           True, 0

--(benchmark score)----------------------------------------------
- Exec Cost Score:      20765 (querylist x 100 + acquire-failure x 10 + queryjob x 1)
- Efficient Score:      331.25 (average + stdev)
  */
    //using IHostedService instead of BackgroundService for the practice
    public class ScheduleEngine : IHostedService
    {
        static Lazy<ScheduleQueue<JobInfo>> _que = new ();

        private void ProcessingWork(int id)
        {
            using (JobsRepo repo = new JobsRepo())
            {
                var nowInfo = repo.GetJob(id);

                if (nowInfo.State == 0)
                {
                    if (repo.AcquireJobLock(id))
                    {
                        repo.ProcessLockedJob(nowInfo.Id);
                        Console.WriteLine(
                            $"[T: {Thread.CurrentThread.ManagedThreadId}] process job({nowInfo.Id}) with delay {(DateTime.Now - nowInfo.RunAt).TotalMilliseconds} msec...");

                    }
                }
            }
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("Application is running....");
            await Task.Delay(3000);
            Random rnd = new Random();
            Stopwatch timer = new Stopwatch();
            using (JobsRepo repo = new JobsRepo())
            {
                while (true)
                {
                    Console.WriteLine($"[T: {Thread.CurrentThread.ManagedThreadId}] fetch available jobs from repository...");

                    foreach (var job in repo.GetReadyJobs(JobSettings.MinPrepareTime))
                    {
                        var now = DateTime.Now;
                        int predict_time = rnd.Next(300, 1700);
                        if (job.RunAt - DateTime.Now > TimeSpan.FromMilliseconds(predict_time)) // 等到約一秒前，可以被取消。一秒內就先 LOCK
                        {
                            try
                            {
                                await Task.Delay(job.RunAt - DateTime.Now - TimeSpan.FromMilliseconds(predict_time), cancellationToken);
                            }
                            catch { }
                        }
                        if (repo.GetJob(job.Id).State != 0) continue;
                        // if (repo.AcquireJobLock(job.Id) == false) continue;
                        if (DateTime.Now < job.RunAt) await Task.Delay(job.RunAt - DateTime.Now);
                        _que.Value.Processing = ((info) =>
                        {
                            ProcessingWork(info.Id);
                        });
                        await _que.Value.Enqueue(job);
                       

                    }
                    try
                    {
                        await Task.Delay(
                            (int)Math.Max((JobSettings.MinPrepareTime - timer.Elapsed).TotalMilliseconds, 0),
                            cancellationToken);
                    }
                    catch
                    {
                        await StopAsync(cancellationToken);
                    }
                }

            }

        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _que.Value.Dispose();
            return Task.CompletedTask;
        }
    }
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Starting Application....");
            try
            {
                await StartUp();
            }
            catch (Exception e)
            {
                await _host.StopAsync();
            }
        }
        private static IHost _host;
        public static async Task StartUp()
        {
            _host = Host.CreateDefaultBuilder()
            .ConfigureServices((hostBuilderContext, serviceCollection) =>
            {
                    //di,injection engines or services
                    serviceCollection.AddHostedService<ScheduleEngine>();

            })
            .ConfigureLogging((hostContext, loggingBuilder) =>
            {


            })
            .Build();

            await _host.RunAsync();


        }
    }
}

