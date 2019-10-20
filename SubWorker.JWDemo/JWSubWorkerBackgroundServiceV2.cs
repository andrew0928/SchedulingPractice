using Microsoft.Extensions.Hosting;
using SchedulingPractice.Core;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

/**
 * 啟動多個Process時,降低Process間發生碰撞的情況:
 *  1. 讓每個Process交錯查詢Table,以RANDOM實現.
 *  2. 以QueryJob先確認Job狀態,再Acquire,減少碰撞.
 *  
 * [最佳組合]
 * 我希望Avg Delay時間<1sec,並且碰撞在可以接受的範圍,雖然Process越少分數越低,但相對Avg Delay時間也拉高,建議3組較佳.
 */
namespace SubWorker.JWDemo
{
    public class JWSubWorkerBackgroundServiceV2 : BackgroundService
    {
        private static int MAX_CONCURRENT_SIZE = 5;

        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            ThreadPool.SetMinThreads(MAX_CONCURRENT_SIZE, MAX_CONCURRENT_SIZE);

            Random random = new Random();

            TimeSpan sleepSpan;

            double runAtRange = JobSettings.MinPrepareTime.TotalSeconds;

            for (int i = 0; i < MAX_CONCURRENT_SIZE; i++)
            {
                Task.Run(() =>
                {
                    JobWorker jobWorker = new JobWorker(MAX_CONCURRENT_SIZE);
                });
            }

            await Task.Delay(1);


            using (JobsRepo repo = new JobsRepo())
            {

                while (stoppingToken.IsCancellationRequested == false)
                {
                    JobInfo tmpJobInfo = null;

                    foreach (JobInfo job in repo.GetReadyJobs(TimeSpan.FromSeconds(runAtRange)))
                    {
                        Queue.push(job);
                        //Console.WriteLine(job.Id + "\t" + job.RunAt);

                        tmpJobInfo = job;
                    }


                    //計算SLEEP多久
                    if (tmpJobInfo == null)
                    {

                        sleepSpan = TimeSpan.FromMilliseconds((10 * 1000) + random.Next(5 * 1000));

                        //Console.WriteLine("[random]" + sleepSpan.Seconds + "\t" + sleepSpan.TotalSeconds);
                    }
                    else
                    {
                        sleepSpan = TimeSpan.FromSeconds((tmpJobInfo.RunAt.AddSeconds(random.Next(15)) - DateTime.Now).Seconds + 1);

                        //Console.WriteLine("[next]" + sleepSpan.Seconds + "\t" + sleepSpan.TotalSeconds);
                    }

                    await Task.Delay(sleepSpan, stoppingToken);

                    Console.Write("_");
                }
            }
        }

        internal class Queue
        {

            private static BlockingCollection<JobInfo> buffer = new BlockingCollection<JobInfo>();

            public static void push(JobInfo job)
            {
                buffer.Add(job);
            }

            public static JobInfo pop()
            {
                return buffer.Take();
            }
        }


        internal class JobWorker
        {

            public JobWorker(int concurrentSize)
            {
                using (JobsRepo repo = new JobsRepo())
                {
                    while (true)
                    {
                        JobInfo job = Queue.pop();

                        long delay = (job.RunAt.Ticks / TimeSpan.TicksPerMillisecond) - (DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond);
                        //Console.WriteLine("[" + job.Id + "] job.RunAt=> " + job.RunAt + ", DateTime.Now=>" + DateTime.Now + ", delay =>" + delay);
                        if (delay > 0)
                        {
                            Task.Delay(TimeSpan.FromMilliseconds(delay)).Wait();
                        }

                        if (repo.GetJob(job.Id).State == 0)
                        {
                            if (repo.AcquireJobLock(job.Id))
                            {
                                repo.ProcessLockedJob(job.Id);
                                Console.Write("[" + job.Id + "]" + "O");
                            }
                            else
                            {
                                Console.Write("[" + job.Id + "]" + "X");
                            }
                        }
                    }
                }
            }
        }
    }
}



//DURATION: 1 MIN, 5個INSTANCE, 每個INSTANCE CONCURRENT 5, 1秒查TABLE 1次
//Jobs Scheduling Metrics:

//--(action count)----------------------------------------------
//- CREATE:             187
//- ACQUIRE_SUCCESS:    187
//- ACQUIRE_FAILURE:    132
//- COMPLETE:           187
//- QUERYJOB:           0
//- QUERYLIST:          18

//--(state count)----------------------------------------------
//- COUNT(CREATE) :      0
//- COUNT(LOCK) :        0
//- COUNT(COMPLETE) :    187

//--(statistics)----------------------------------------------
//- DELAY(Average) :     1722
//- DELAY(Stdev) :       2641.33869433886

//--(test result)----------------------------------------------
//- Complete Job:       True, 187 / 187
//- Delay Too Long:     0
//- Fail Job:           True, 0

//--(benchmark score)----------------------------------------------
//- Exec Cost Score:      3120 (querylist x 100 + acquire-failure x 10 + queryjob x 1)
//- Efficient Score:      4363.34 (average + stdev)


//DURATION: 1 MIN, 3個INSTANCE, 每個INSTANCE CONCURRENT 5, 查TABLE=>(RunAt+RANDOM(20)-Now)+1 & DEFAULT: 10sec+RANDOM(5sec)
//Jobs Scheduling Metrics:

//--(action count)----------------------------------------------
//- CREATE:             187
//- ACQUIRE_SUCCESS:    187
//- ACQUIRE_FAILURE:    22
//- COMPLETE:           187
//- QUERYJOB:           303
//- QUERYLIST:          23

//--(state count)----------------------------------------------
//- COUNT(CREATE) :      0
//- COUNT(LOCK) :        0
//- COUNT(COMPLETE) :    187

//--(statistics)----------------------------------------------
//- DELAY(Average) :     512
//- DELAY(Stdev) :       1208.29197769571

//--(test result)----------------------------------------------
//- Complete Job:       True, 187 / 187
//- Delay Too Long:     0
//- Fail Job:           True, 0

//--(benchmark score)----------------------------------------------
//- Exec Cost Score:      2823 (querylist x 100 + acquire-failure x 10 + queryjob x 1)
//- Efficient Score:      1720.29 (average + stdev)



//DURATION: 10 MIN, 5個INSTANCE, 1MIN後隨機關2個, 每個INSTANCE CONCURRENT 5, 查TABLE=>(RunAt+RANDOM(15)-Now)+1 & DEFAULT: 10sec+RANDOM(5sec)
//Jobs Scheduling Metrics:

//--(action count)----------------------------------------------
//- CREATE:             1750
//- ACQUIRE_SUCCESS:    1750
//- ACQUIRE_FAILURE:    391
//- COMPLETE:           1750
//- QUERYJOB:           3357
//- QUERYLIST:          129

//--(state count)----------------------------------------------
//- COUNT(CREATE) :      0
//- COUNT(LOCK) :        0
//- COUNT(COMPLETE) :    1750

//--(statistics)----------------------------------------------
//- DELAY(Average) :     252
//- DELAY(Stdev) :       591.211818703911

//--(test result)----------------------------------------------
//- Complete Job:       True, 1750 / 1750
//- Delay Too Long:     0
//- Fail Job:           True, 0

//--(benchmark score)----------------------------------------------
//- Exec Cost Score:      20167 (querylist x 100 + acquire-failure x 10 + queryjob x 1)
//- Efficient Score:      843.21 (average + stdev)