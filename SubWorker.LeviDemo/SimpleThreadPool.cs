using System;
using System.Collections.Generic;
using System.Threading;
using SchedulingPractice.Core;

namespace SubWorker.LeviDemo
{
    public class SimpleThreadPool : IDisposable
    {
        private readonly List<Thread> _workerThreads = new List<Thread>();

        private bool _stopFlag = false;
        private bool _cancelFlag = false;

        private readonly TimeSpan _maxWorkerThreadTimeout = TimeSpan.FromMilliseconds(3000);
        private readonly int _maxWorkerThreadCount = 0;

        private readonly Queue<JobInfo> _workItems = new Queue<JobInfo>();
        private readonly ManualResetEvent _enqueueNotify = new ManualResetEvent(false);

        public SimpleThreadPool(int threads)
        {
            _maxWorkerThreadCount = threads;
        }

        public int GetWorkItemCount()
        {
            return _workItems.Count;
        }


        public void CreateMaxThreads()
        {
            for (var i = 0; i < _maxWorkerThreadCount; i = i + 1)
            {
                CreateMaxThreads();
            }
        }

        private void CreateWorkerThread()
        {
            var worker = new Thread(DoWorkerThread);
            _workerThreads.Add(worker);
            worker.Start();
        }

        public bool QueueUserWorkerItem(JobInfo job)
        {
            if (_stopFlag) return false;

            if (_workItems.Count > 0 && _workerThreads.Count < _maxWorkerThreadCount) CreateWorkerThread();

            _workItems.Enqueue(job);
            _enqueueNotify.Set();

            return true;
        }

        public void EndPool()
        {
            EndPool(false);
        }

        public void CancelPool()
        {
            EndPool(true);
        }

        private void EndPool(bool cancelQueueItem)
        {
            if (_workerThreads.Count == 0) return;

            _stopFlag = true;
            _cancelFlag = cancelQueueItem;
            _enqueueNotify.Set();

            do
            {
                var worker = _workerThreads[0];
                worker.Join();

                _workerThreads.Remove(worker);
            } while (_workerThreads.Count > 0);
        }

        private void DoWorkerThread()
        {
            using (var repo = new JobsRepo())
            {
                while (true)
                {
                    while (_workItems.Count > 0)
                    {
                        JobInfo job = null;
                        lock (_workItems)
                        {
                            if (_workItems.Count > 0)
                            {
                                job = _workItems.Dequeue();
                            }
                        }

                        if (job == null) continue;

                        if (repo.GetJob(job.Id).State == 0)
                        {
                            if (repo.AcquireJobLock(job.Id))
                            {
                                repo.ProcessLockedJob(job.Id);
                                Console.WriteLine($"[Consumer][Done] #{job.Id}");
                            }
                            else
                            {
                                Console.WriteLine($"[Consumer][Failed] #{job.Id}");
                            }
                        }


                        if (_cancelFlag) break;
                    }

                    if (_stopFlag || _cancelFlag) break;
                    if (_enqueueNotify.WaitOne(_maxWorkerThreadTimeout, true)) continue;
                    break;
                }
            }

            _workerThreads.Remove(Thread.CurrentThread);
        }

        public void Dispose()
        {
            EndPool(false);
        }
    }
}