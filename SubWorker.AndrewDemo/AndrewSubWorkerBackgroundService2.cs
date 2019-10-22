#define USE_SPINWAIT
using Microsoft.Extensions.Hosting;
using SchedulingPractice.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SubWorker.AndrewDemo
{
    public class AndrewSubWorkerBackgroundService2 : BackgroundService
    {
        private int _process_threads_count = 10;
        private BlockingCollection<JobInfo> _queue = new BlockingCollection<JobInfo>();

        private CancellationToken _stop;


        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Delay(1);
            this._stop = stoppingToken;

            // init worker threads, 1 fetch, 5 process
            Thread[] threads = new Thread[_process_threads_count];
            for (int i = 0; i < _process_threads_count; i++) threads[i] = new Thread(this.ProcessThread);
            foreach (var t in threads) t.Start();

            // fetch
            Stopwatch timer = new Stopwatch();
            Random rnd = new Random();
            using (JobsRepo repo = new JobsRepo())
            {
                while(true)
                {
                    if (stoppingToken.IsCancellationRequested) goto shutdown;

                    timer.Restart();
                    Console.WriteLine($"[T: {Thread.CurrentThread.ManagedThreadId}] fetch available jobs from repository...");
                    foreach (var job in repo.GetReadyJobs(JobSettings.MinPrepareTime))
                    {
                        if (stoppingToken.IsCancellationRequested) goto shutdown;

                        int predict_time = rnd.Next(300, 1700);
                        if (job.RunAt - DateTime.Now > TimeSpan.FromMilliseconds(predict_time)) // 等到約一秒前，可以被取消。一秒內就先 LOCK
                        {
                            try { await Task.Delay(job.RunAt - DateTime.Now - TimeSpan.FromMilliseconds(predict_time), stoppingToken); } catch { goto shutdown; }
                        }

                        if (repo.GetJob(job.Id).State != 0) continue;
                        if (repo.AcquireJobLock(job.Id) == false) continue;
                        if (DateTime.Now < job.RunAt) await Task.Delay(job.RunAt - DateTime.Now);
                        this._queue.Add(job);
                    }
                    try { 
                        await Task.Delay(
                            (int)Math.Max((JobSettings.MinPrepareTime - timer.Elapsed).TotalMilliseconds, 0), 
                            stoppingToken); 
                    }
                    catch
                    { 
                        goto shutdown;
                    }
                }

                shutdown:
                this._queue.CompleteAdding();
            }

            foreach (var t in threads) t.Join();
            Console.WriteLine($"[T: {Thread.CurrentThread.ManagedThreadId}] shutdown background services...");
        }

        private void ProcessThread()
        {
            using (JobsRepo repo = new JobsRepo())
            {
                foreach(var job in this._queue.GetConsumingEnumerable())
                {
                    repo.ProcessLockedJob(job.Id);
                    Console.WriteLine($"[T: {Thread.CurrentThread.ManagedThreadId}] process job({job.Id}) with delay {(DateTime.Now - job.RunAt).TotalMilliseconds} msec...");
                }
                Console.WriteLine($"[T: {Thread.CurrentThread.ManagedThreadId}] process worker thread was terminated...");
            }
        }
    }
}
