using Microsoft.Extensions.Hosting;
using SchedulingPractice.Core;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

/**
 * 忘了考慮HA,純搞笑,不要理這份~ XD
 */
namespace SubWorker.JWDemo
{
    class JWSubWorkerBackgroundServiceV1 : BackgroundService
    {
        private static int MAX_CONCURRENT_SIZE = 5;

        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            ThreadPool.SetMinThreads(MAX_CONCURRENT_SIZE, MAX_CONCURRENT_SIZE);

            for (int i = 0; i < MAX_CONCURRENT_SIZE; i++)
            {
                Task.Run(() =>
                {
                    JobWorker jobWorker = new JobWorker(MAX_CONCURRENT_SIZE);
                });
            }

            await Task.Delay(1);

            TimeSpan sleepSpan;

            using (JobsRepo repo = new JobsRepo("Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=D:\\workspace\\dotnet\\SchedulingPractice\\SubWorker.AndrewDemo\\JobsDB.mdf;Integrated Security=True"))
            {
                while (stoppingToken.IsCancellationRequested == false)
                {
                    JobInfo tmpJobInfo = null;

                    foreach (JobInfo job in repo.GetReadyJobs())
                    {
                        if (repo.AcquireJobLock(job.Id))
                        {
                            Console.Write("O");
                            Queue.push(job);
                            Console.WriteLine(job.Id + "\t" + job.RunAt);

                            tmpJobInfo = job;
                        }
                        else
                        {
                            Console.Write("X");
                        }
                    }


                    //計算SLEEP多久
                    if (tmpJobInfo == null)
                    {
                        Random random = new Random();
                        sleepSpan = TimeSpan.FromMilliseconds((10 * 1000) + random.Next(5 * 1000));

                        Console.WriteLine("[random]" + sleepSpan.Seconds + "\t" + sleepSpan.TotalSeconds);
                    }
                    else
                    {
                        sleepSpan = TimeSpan.FromSeconds((tmpJobInfo.RunAt.AddSeconds(10) - DateTime.Now).Seconds + 1);

                        Console.WriteLine("[next]" + sleepSpan.Seconds + "\t" + sleepSpan.TotalSeconds);

                        //Console.ReadKey();
                        //System.Environment.Exit(0);
                    }

                    //await Task.Delay(TimeSpan.FromSeconds(29), stoppingToken);
                    await Task.Delay(sleepSpan, stoppingToken);
                    //Console.ReadKey();
                    //System.Environment.Exit(0);
                    Console.Write("_");
                }
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
            using (JobsRepo repo = new JobsRepo("Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename=D:\\workspace\\dotnet\\SchedulingPractice\\SubWorker.AndrewDemo\\JobsDB.mdf;Integrated Security=True"))
            {
                while (true)
                {
                    JobInfo job = Queue.pop();

                    repo.ProcessLockedJob(job.Id);
                }
            }
        }
    }
}



//DURATION: 1 MIN, 5個INSTANCE, 每個INSTANCE CONCURRENT 5, 1秒查TABLE 1次
//Jobs Scheduling Metrics:

//--(action count)----------------------------------------------
//- CREATE:             188
//- ACQUIRE_SUCCESS:    188
//- ACQUIRE_FAILURE:    169
//- COMPLETE:           188
//- QUERYJOB:           0
//- QUERYLIST:          268

//--(state count)----------------------------------------------
//- COUNT(CREATE) :      0
//- COUNT(LOCK) :        0
//- COUNT(COMPLETE) :    188

//--(statistics)----------------------------------------------
//- DELAY(Average) :     625
//- DELAY(Stdev) :       309.521729614663

//--(test result)----------------------------------------------
//- Complete Job:       True, 188 / 188
//- Delay Too Long:     0
//- Fail Job:           True, 0

//--(benchmark score)----------------------------------------------
//- Exec Cost Score:      28490 (querylist x 100 + acquire-failure x 10 + queryjob x 1)
//- Efficient Score:      934.52 (average + stdev)



//DURATION: 1 MIN, 5個INSTANCE, 每個INSTANCE CONCURRENT 5, 29秒查TABLE 1次
//Jobs Scheduling Metrics:

//--(action count)----------------------------------------------
//- CREATE:             192
//- ACQUIRE_SUCCESS:    192
//- ACQUIRE_FAILURE:    437
//- COMPLETE:           192
//- QUERYJOB:           0
//- QUERYLIST:          25

//--(state count)----------------------------------------------
//- COUNT(CREATE) :      0
//- COUNT(LOCK) :        0
//- COUNT(COMPLETE) :    192

//--(statistics)----------------------------------------------
//- DELAY(Average) :     14777
//- DELAY(Stdev) :       8064.65536567453

//--(test result)----------------------------------------------
//- Complete Job:       True, 192 / 192
//- Delay Too Long:     0
//- Fail Job:           True, 0

//--(benchmark score)----------------------------------------------
//- Exec Cost Score:      6870 (querylist x 100 + acquire-failure x 10 + queryjob x 1)
//- Efficient Score:      22841.66 (average + stdev)