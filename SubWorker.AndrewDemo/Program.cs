using SchedulingPractice.Core;
using System.Linq;
using System;
using System.Threading.Tasks;

namespace SubWorker.AndrewDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            Task.Delay(3000).Wait();

            using(JobsRepo repo= new JobsRepo())
            {
                JobInfo job = null;
                int max_retry_count = 30;
                int retry = 0;

                while (retry < max_retry_count)
                {
                    while ((job = repo.GetReadyJobs(TimeSpan.Zero).FirstOrDefault()) != null)
                    {
                        retry = 0;
                        if (repo.AcquireJobLock(job.Id))
                        {
                            repo.ProcessLockedJob(job.Id);
                            Console.Write(".");
                        }
                    }

                    retry++;
                    Task.Delay(5000).Wait();
                    Console.Write("_");
                }
            }
        }
    }
}
