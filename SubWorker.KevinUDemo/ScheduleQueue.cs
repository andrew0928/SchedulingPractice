using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SubWorker.KevinUDemo
{

    public class ScheduleQueue<T> : IDisposable
    {
        public Action<T> Processing;
        private ConcurrentDictionary<int, T> _infos = new();
        private ConcurrentQueue<T> _queue = new();

        private int _sn = 0;
        private ManualResetEvent _reset = new ManualResetEvent(false);
        public bool Set()
        {
            try
            {
                _isWaiting = false;
                return _reset.Set();
            }
            catch { }
            return false;
        }
        public bool WaitOne(TimeSpan timeout)
        {

            try
            {
                _reset.Reset();
                _isWaiting = true;
                return _reset.WaitOne(timeout);
            }
            catch { }
            return false;
        }
        bool isdisposed = false;
        object isdisposedLock = new object();
        public void End()
        {
            if (isdisposed) return;
            lock (isdisposedLock)
            {
                if (isdisposed) return;
                isdisposed = true;
            }

            Set();
            _reset.Dispose();
        }
        public bool IsWaiting => _isWaiting;
        private bool _isWaiting = false;

        //將待處裡任務enqueue
        //用.net 自己調度的task
        public Task Enqueue(T thisJob)
        {
            _queue.Enqueue(thisJob);
            if (IsWaiting)
            {
                Set();
                return Task.CompletedTask;
            }
            if (_infos.Count > 8)
            {
                return Task.CompletedTask;
            }
            if (_queue.Count > 0)
            {
                var serial = Interlocked.Increment(ref _sn);

                _infos.TryAdd(serial, thisJob);

                new Thread(delegate ()
             {
                 while (isdisposed)
                 {
                     if (_queue.TryDequeue(out var jobItem))
                     {
                         Processing.Invoke(jobItem);
 
                     }
                     if (_queue.Count == 0)
                     {
                         if (WaitOne(TimeSpan.FromSeconds(30))) continue;
                         if (_queue.Count == 0) break;
                     }
                 }
                 _infos.TryRemove(serial, out var jobItemRemoved);
                 End();


             }).Start();

            }
            return Task.CompletedTask;

        }


        public void Dispose()
        {
            if (isdisposed) return;
            lock (isdisposedLock)
            {
                if (isdisposed) return;
                isdisposed = true;
            }

            while (_queue.TryDequeue(out var waitToDo)) ;

        }
    }


}
