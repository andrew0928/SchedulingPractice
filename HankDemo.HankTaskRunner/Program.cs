using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SchedulingPractice.Core;

namespace HankTestTwo
{
    abstract class Program
    {
        static void Main(string[] args)
        {
            var host = new HostBuilder()
                .ConfigureServices((context, services) => { services.AddHostedService<HankTestProgram>(); })
                .Build();

            using (host)
            { 
                host.Start();
                host.WaitForShutdown();
            }
        }

    }
    public class HankTestProgram : BackgroundService
    {
        private static Dictionary<int, Queue<JobInfo>> jobList;

        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            ThreadPool.SetMaxThreads(5, 5);
            ThreadPool.QueueUserWorkItem(Worker, 0);
            ThreadPool.QueueUserWorkItem(Worker, 1);
            ThreadPool.QueueUserWorkItem(Worker, 2);
            ThreadPool.QueueUserWorkItem(Worker, 3);
            ThreadPool.QueueUserWorkItem(Worker, 4);

            using (JobsRepo repo = new JobsRepo())
            {
                while (stoppingToken.IsCancellationRequested == false)
                {
                    var newJob = repo.GetReadyJobs(TimeSpan.FromSeconds(15)).ToList();

                    jobList = getCleanDictionary();

                    for (int i = 0; i < newJob.Count; i++)
                    {
                        jobList[i % 5].Enqueue(newJob[i]);
                    }

                    try
                    {
                        await Task.Delay(5000, stoppingToken);
                        Console.Write("_");
                    }
                    catch (TaskCanceledException)
                    {
                        break;
                    }
                }
            }

        shutdown:

            Console.WriteLine($"- shutdown event detected, stop worker service...");
        }

        private static void Worker(object obj)
        {
            var index = (int)obj;
            JobInfo job;
            while (true)
            {
                if (jobList == null || jobList[index] == null || jobList[index].TryDequeue(out job) == false)
                {
                    Thread.Sleep(20);
                    continue;
                }

                if (job != null && job.RunAt <= DateTime.Now)
                {
                    try
                    {
                        using (var repo = new JobsRepo())
                        {
                            job = repo.GetJob(job.Id);
                            if (job.State == 0 && repo.AcquireJobLock(job.Id))
                            {
                                repo.ProcessLockedJob(job.Id);
                                Console.Write("O");
                            }
                            else
                            {
                                Console.Write("X");
                            }
                        }
                    }
                    catch
                    {
                        Console.Write("E");
                    }
                }
            }
        }


        private static Dictionary<int, Queue<JobInfo>> getCleanDictionary()
        {
            return new Dictionary<int, Queue<JobInfo>>()
                {
                    {0, new Queue<JobInfo>()},
                    {1, new Queue<JobInfo>()},
                    {2, new Queue<JobInfo>()},
                    {3, new Queue<JobInfo>()},
                    {4, new Queue<JobInfo>()},
                };
        }
    }
}