using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SchedulingPractice.Core;

namespace SubWorker.JolinDemo
{
    public class Scheduler
    {
        private BlockingCollection<JobInfo> _jobList;

        private ManualResetEvent _stopAre = new ManualResetEvent(false);
        private ManualResetEvent _doJobAre = new ManualResetEvent(false);

        private List<Thread> _threadList = new List<Thread>();

        private string _connectString;

        private bool _stop;

        private int _intervalSecond = 10;

        public Scheduler(string connectString, int workerCount)
        {
            _connectString = connectString;
            _jobList = new BlockingCollection<JobInfo>();
            _stop = false;

            // Fetch Thread
            var fetchThread = new Thread(FetchJob);
            fetchThread.Start();
            _threadList.Add(fetchThread);

            for (int i = 0; i < workerCount; i++)
            {
                var excuteJobThread = new Thread(ExuteBody);
                excuteJobThread.Start();

                _threadList.Add(excuteJobThread);
            }
        }

        public void Stop()
        {
            this._stop = true;
            this._stopAre.Set();

            foreach (var thread in _threadList)
            {
                thread.Join();
            }
        }

        private void ExuteBody()
        {
            while (_stop == false)
            {
                JobInfo job = null;
                while (this._jobList.TryTake(out job))
                {
                    if (job.RunAt > DateTime.Now)
                    {
                        var sleepTime = job.RunAt.Subtract(DateTime.Now);

                        int index = Task.WaitAny(
                                        Task.Delay(sleepTime),
                                        Task.Run(() => _stopAre.WaitOne()));

                        if (index == 1)
                        {
                            break;
                        }
                    }

                    using (JobsRepo repo = new JobsRepo(this._connectString))
                    {
                        if (repo.AcquireJobLock(job.Id))
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

                Task.WaitAny(
                            Task.Run(() => _stopAre.WaitOne()),
                            Task.Run(() => _doJobAre.WaitOne()));
            }
        }

        private void FetchJob()
        {
            while (_stop == false)
            {
                using (JobsRepo repo = new JobsRepo(_connectString))
                {
                    var jobs = repo.GetReadyJobs(TimeSpan.FromSeconds(_intervalSecond));

                    foreach (var job in jobs.OrderBy(X => X.RunAt))
                    {
                        this._jobList.Add(job);

                        Console.WriteLine(job.Id);
                    }

                    _doJobAre.Set();
                    _doJobAre.Reset();
                }

                Task.WaitAny(Task.Delay(TimeSpan.FromSeconds(_intervalSecond)),
                             Task.Run(() => _stopAre.WaitOne()));
            }
        }
    }
}