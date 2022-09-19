using Microsoft.Extensions.Hosting;
using SchedulingPractice.Core;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SubWorker.JimDemo
{
    public class JimSubWorkerBackgroundService : BackgroundService
    {
        private BlockingCollection<JobInfo> _queue = new BlockingCollection<JobInfo>();
        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Delay(1);
            List<Task> tasks = new List<Task>();
            for (int i = 0; i < 5; i++)
            {
                Task task = Task.Run(() => Worker());
                tasks.Add(task);
            }

            Stopwatch timer = new Stopwatch();

            using (JobsRepo repo = new JobsRepo())
            {
                while (stoppingToken.IsCancellationRequested == false)
                {
                    timer.Restart();
                    Console.WriteLine($"[T: {Thread.CurrentThread.ManagedThreadId}] fetch available jobs from repository...");

                    foreach (var job in repo.GetReadyJobs(JobSettings.MinPrepareTime))
                    {
                        if (stoppingToken.IsCancellationRequested == false)
                        {
                            if (repo.GetJob(job.Id).State == 0)
                            {
                                if (repo.AcquireJobLock(job.Id))
                                {
                                    if (job.RunAt > DateTime.Now)
                                    {
                                        await Task.Delay(job.RunAt - DateTime.Now);
                                    }
                                    _queue.Add(job);
                                }
                            }
                        }
                    }
                    
                    if (stoppingToken.IsCancellationRequested == false)
                    {
                        if(JobSettings.MinPrepareTime > timer.Elapsed)
                        {
                            await Task.Delay((int)Math.Max((JobSettings.MinPrepareTime - timer.Elapsed).TotalMilliseconds, 0), stoppingToken);
                        }
                    }
                }
                _queue.CompleteAdding();
                Task.WaitAll(tasks.ToArray());
            }
            Console.WriteLine($"[T: {Thread.CurrentThread.ManagedThreadId}] shutdown background services...");
        }
        private void Worker()
        {
            using (JobsRepo repo = new JobsRepo())
            {
                foreach (var job in this._queue.GetConsumingEnumerable())
                {
                    repo.ProcessLockedJob(job.Id);
                    Console.WriteLine($"[T: {Thread.CurrentThread.ManagedThreadId}] process job({job.Id}) with delay {(DateTime.Now - job.RunAt).TotalMilliseconds} msec...");
                }
                Console.WriteLine($"[T: {Thread.CurrentThread.ManagedThreadId}] process worker thread was terminated...");
            }
        }
    }
}
