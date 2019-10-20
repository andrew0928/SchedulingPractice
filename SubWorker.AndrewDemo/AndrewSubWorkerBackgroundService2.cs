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
        private int _process_threads_count = 5;
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
                //while (stoppingToken.IsCancellationRequested == false)
                while(true)
                {
                    if (stoppingToken.IsCancellationRequested) goto shutdown;

                    timer.Restart();
                    Console.WriteLine($"[T: {Thread.CurrentThread.ManagedThreadId}] fetch available jobs from repository...");
                    foreach (var job in repo.GetReadyJobs(JobSettings.MinPrepareTime))
                    {
                        if (stoppingToken.IsCancellationRequested) goto shutdown;

                        int predict_time = rnd.Next(500, 1500);
                        if (job.RunAt - DateTime.Now > TimeSpan.FromMilliseconds(predict_time)) // 等到約一秒前，可以被取消。一秒內就先 LOCK
                        {
                            try { await Task.Delay(job.RunAt - DateTime.Now - TimeSpan.FromMilliseconds(predict_time), stoppingToken); } catch { goto shutdown; }
                        }

                        if (repo.GetJob(job.Id).State != 0) continue;
                        if (repo.AcquireJobLock(job.Id) == false) continue;
                        if (DateTime.Now < job.RunAt) await Task.Delay(job.RunAt - DateTime.Now);
                        this._queue.Add(job);
                        //Console.Write('F');
                    }
                    try { await Task.Delay(JobSettings.MinPrepareTime - timer.Elapsed, stoppingToken); } catch { goto shutdown; }
                    //Console.Write('Z');
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
                    //if (this._stop.IsCancellationRequested) break;
                    repo.ProcessLockedJob(job.Id);
                    Console.WriteLine($"[T: {Thread.CurrentThread.ManagedThreadId}] process job({job.Id}) with delay {(DateTime.Now - job.RunAt).TotalMilliseconds} msec...");
                }
                Console.WriteLine($"[T: {Thread.CurrentThread.ManagedThreadId}] process worker thread was terminated...");
            }
        }

    }

    /*
    public static class JobsRepoExt
    {
        public static async Task<bool> CanAcquireJobLockAsync(this JobsRepo context, int jobid)
        {
            return await Task.Run<bool>(() => { return context.GetJob(jobid).State == 0; });
        }
        public static async Task<bool> AcquireJobLockAsync(this JobsRepo context, int jobid)
        {
            return await Task.Run<bool>(() => { return context.AcquireJobLock(jobid); });
        }
    }
    */

    /*
    public class AndrewSubWorkerBackgroundService2 : BackgroundService
    {
        private CancellationToken _stop;
        private int _worker_count = 5;

        private BlockingCollection<JobInfo> _queue = new BlockingCollection<JobInfo>();

        private class DispatchThreadContext
        {
            public CancellationTokenSource _temp_jobs_reset;
            public AutoResetEvent _temp_ready = new AutoResetEvent(false);
            public Thread _thread;
        }

        private DispatchThreadContext[] _dispatch_contexts = null;

        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            this._stop = stoppingToken;

            Thread fetch = new Thread(this.FetchThread);
            fetch.Start();

            this._dispatch_contexts = new DispatchThreadContext[this._worker_count];
            for (int i = 0; i < this._worker_count; i++)
            {
                var d = this._dispatch_contexts[i] = new DispatchThreadContext();
                d._thread = new Thread(this.DispatchThread);
                d._thread.Start(d);
            }

            await Task.Delay(1);

            stoppingToken.WaitHandle.WaitOne();

            fetch.Join();
            foreach (var d in this._dispatch_contexts) d._thread.Join();
        }


        private void FetchThread()
        {
            Stopwatch timer = new Stopwatch();

            using (var repo = new JobsRepo())
            {
                while (this._stop.IsCancellationRequested == false)
                {
                    timer.Restart();
                    Console.WriteLine($"[Fetch]: init");
                    {
                        this._queue = new BlockingCollection<JobInfo>();
                        foreach (var job in repo.GetReadyJobs(JobSettings.MinPrepareTime))
                        {
                            this._queue.Add(job);
                        }
                        this._queue.CompleteAdding();

                        for (int i = 0; i < this._worker_count; i++)
                        {
                            var disp = this._dispatch_contexts[i];
                            disp._temp_jobs_reset = new CancellationTokenSource();
                            disp._temp_ready.Set();
                        }
                    }
                    Console.WriteLine($"[Fetch]: ready");


                    if (SpinWait.SpinUntil(() => 
                        {
                            if (this._stop.IsCancellationRequested) return true; 
                            return false;
                        }, 
                        (int)(JobSettings.MinPrepareTime - timer.Elapsed).TotalMilliseconds / 2))
                    {
                        // abort
                        Console.WriteLine($"[Fetch]: abort");
                        foreach (var disp in this._dispatch_contexts) disp._temp_jobs_reset.Cancel();
                        break;
                    }
                }
            }
            Console.WriteLine($"[Fetch]: exit");
        }




        private void DispatchThread(object value)
        {
            DispatchThreadContext context = (DispatchThreadContext)value;
            Console.WriteLine($"[Disp #{Thread.CurrentThread.ManagedThreadId}]: init");
            
            using (var repo = new JobsRepo())
            {
                while (this._stop.IsCancellationRequested == false)
                {
                    // ToDo: wait next new temp_job & reset token
                    Console.WriteLine($"[Disp #{Thread.CurrentThread.ManagedThreadId}]: wait ready");

                    switch (WaitHandle.WaitAny(new WaitHandle[]
                        {
                            context._temp_ready,
                            this._stop.WaitHandle,
                        }))
                    {
                        case 0:
                            Console.WriteLine($"[Disp #{Thread.CurrentThread.ManagedThreadId}]: wait ready - auto reset event set!");
                            break;
                        case 1:
                            Console.WriteLine($"[Disp #{Thread.CurrentThread.ManagedThreadId}]: wait ready - abort");
                            continue;
                    }

                    Console.WriteLine($"[Disp #{Thread.CurrentThread.ManagedThreadId}]: ready");

                    //while(this._queue.IsCompleted == false)
                    foreach(var job in this._queue.GetConsumingEnumerable())
                    {
                        //JobInfo job = this._queue.Take(context._temp_jobs_reset.Token);
                        //if (job == null) break;

                        Console.WriteLine($"[Disp #{Thread.CurrentThread.ManagedThreadId}]: wait timer for job[{job.Id}], runat: {job.RunAt}");

                        // use spinwait
                        bool result = SpinWait.SpinUntil(() =>
                        {
                            if (context._temp_jobs_reset.Token.IsCancellationRequested) return true;
                            if (this._stop.IsCancellationRequested) return true;
                            return false;
                        },
                        Math.Max((int)(job.RunAt - DateTime.Now).TotalMilliseconds, 0));

                        if (result == false)
                        {
                            Console.WriteLine($"[Disp #{Thread.CurrentThread.ManagedThreadId}]: do job[{job.Id}]...");
                            if (repo.GetJob(job.Id).State == 0 && repo.AcquireJobLock(job.Id))
                            {
                                repo.ProcessLockedJob(job.Id);
                                Console.WriteLine($"[Disp #{Thread.CurrentThread.ManagedThreadId}]: process job[{job.Id}], runat: {job.RunAt}, execat: {DateTime.Now}, delay: {(DateTime.Now - job.RunAt).TotalMilliseconds} msec ...");
                            }
                            else
                            {
                                //Console.WriteLine($"[Disp #{Thread.CurrentThread.ManagedThreadId}]: do not acquired job[{job.Id}]...");
                            }
                            continue;
                        }
                        else
                        {
                            //Console.WriteLine($"[Disp #{Thread.CurrentThread.ManagedThreadId}]: abort job[{job.Id}], reason: reset job queue / system shutdown...");
                        }

                        goto reset;
                    }
                    Console.WriteLine($"[Disp #{Thread.CurrentThread.ManagedThreadId}]: empty, go wait next fetch");

                reset:
                    continue;
                }
            }
            Console.WriteLine($"[Disp #{Thread.CurrentThread.ManagedThreadId}]: exit");
        }
    }
    */
}
