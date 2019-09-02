using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SchedulingPractice.Core;

namespace SubWorker.JolinDemo
{
    public class Scheduler
    {
        private Dictionary<int,JobInfo> _jobList;

        private AutoResetEvent _are = new AutoResetEvent(false);

        private Thread _excuteJobThread;

        private JobInfo _nextJob = JobInfo.Empty;

        private string _connectString;

        private bool _stop;

        public Scheduler(string connectString)
        {
            _connectString = connectString;
            _jobList = new Dictionary<int, JobInfo>();
            _stop = false;

            _excuteJobThread = new Thread(ExuteBody);
            _excuteJobThread.Start();
        }

        internal void SetSchedule(IEnumerable<JobInfo> jobs)
        {
            lock (_jobList)
            {
                foreach (var job in jobs)
                {
                    if (_jobList.ContainsKey(job.Id) == false)
                    {
                        _jobList.Add(job.Id, job);
                    }
                }

                this.ReCheckSort();
            }
        }

        public void Stop()
        {
            this._stop = true;
            this._are.Set();
            _excuteJobThread.Join();
        }

        private void ReCheckSort()
        {
            lock (_jobList)
            {
                lock (_nextJob)
                {
                    if (_jobList.Count == 0)
                    {
                        //表示沒東西可以排了
                        _nextJob = JobInfo.Empty;
                        return;
                    }

                    var firstOneInScheduler = _jobList.Select(x => x.Value).OrderBy(x => x.RunAt).First();

                    if (_nextJob == JobInfo.Empty || firstOneInScheduler.RunAt < _nextJob.RunAt)
                    {
                        _jobList.Remove(firstOneInScheduler.Id);

                        Console.WriteLine($"Change first scheduler : {_nextJob.Id}_{_nextJob.RunAt} to {firstOneInScheduler.Id}_{firstOneInScheduler.RunAt}");

                        if (_nextJob != JobInfo.Empty)
                        {
                            // 重排第一個
                            _jobList.Add(_nextJob.Id, _nextJob);
                        }

                        _nextJob = firstOneInScheduler;   
                    }
                }
            }

            _are.Set();
        }

        private void ExuteBody()
        {
            while (_stop == false)
            {
                var sleepTime = TimeSpan.FromSeconds(9999);
                bool needReSort = false;

                lock (_nextJob)
                {
                    if (_nextJob != JobInfo.Empty)
                    {
                        if (_nextJob.RunAt < DateTime.Now)
                        {
                            ExcuteSchedule();
                            needReSort = true;
                        }

                        if (_nextJob != JobInfo.Empty)
                        {
                            sleepTime = _nextJob.RunAt.Subtract(DateTime.Now);
                        }
                    }
                }

                if(needReSort)
                    this.ReCheckSort();

                Console.WriteLine($"Sleep TotalMilliseconds: {sleepTime.TotalMilliseconds}");

                int index = Task.WaitAny(
                                Task.Delay(sleepTime),
                                Task.Run(() => _are.WaitOne()));

                Console.WriteLine($"wake up : {index}");
            }

        }

        private void ExcuteSchedule()
        {
            using (JobsRepo repo = new JobsRepo(this._connectString))
            {
                if (repo.AcquireJobLock(_nextJob.Id))
                {
                    repo.ProcessLockedJob(_nextJob.Id);
                    Console.Write("O");
                }
                else
                {
                    Console.Write("X");
                }

                _nextJob = JobInfo.Empty;
            }
        }
    }
}
