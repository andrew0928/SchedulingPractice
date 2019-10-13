using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using SchedulingPractice.Core;

/*
 * Mindset
 *     Architecture: Producer -> Job Queue <- ThreadPool
 *     Action Cost: query job list x 100 + acquire-failure x 10 + query job x 1
 * Producer
 *     0. Action: Fetch job then put to job queue
 *     1. Fetching job is very expensive action, need to chose right time 
 *     2. There are two fetching strategies
 *         1. If there is no job, fetch jobs after MinPrepareTime (10 seconds)
 *         2. If there is job in database, fetch jobs after NextJob.RunAt + 1 seconds
 *
 *     examples:
 *         now: 00:00:00
 *         next job: 00:00:10
 *         wait for 11 seconds
 * Consumer
 *     0. Action get job from queue then process it
 *     1. Check job status before Acquire and Process
 */

namespace SubWorker.LeviDemo
{
    public class LeviSubWorkerBackgroundService : BackgroundService
    {
        private const string ConnectionString = "Server=.;Database=master;user id=SA;password=Levi123456";
        private readonly SimpleThreadPool _stp;
        private readonly TimeSpan _defaultFetchJobsIntervalSeconds = JobSettings.MinPrepareTime;
        private readonly TimeSpan _fetchJobsRunAtRange = JobSettings.MinPrepareTime;

        public LeviSubWorkerBackgroundService()
        {
            _stp = new SimpleThreadPool(5);

            Console.WriteLine("[System] In Levi's Background Service'");
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.Delay(1, stoppingToken);

            using (var repo = new JobsRepo(ConnectionString))
            {
                while (stoppingToken.IsCancellationRequested == false)
                {
                    JobInfo lastJobInThisRound = null;
                    TimeSpan intervalSeconds = _defaultFetchJobsIntervalSeconds;

                    Console.WriteLine("[Producer][FetchJobs]");
                    var jobs = repo.GetReadyJobs(_fetchJobsRunAtRange);

                    foreach (var job in jobs)
                    {
                        if (stoppingToken.IsCancellationRequested) goto shutdown;

                        Console.WriteLine($"[Producer][PutJobToQueue] #{job.Id} run at {job.RunAt}");
                        _stp.QueueUserWorkerItem(job);
                        lastJobInThisRound = job;
                    }

                    try
                    {
                        if (lastJobInThisRound != null)
                        {
                            intervalSeconds = TimeSpan.FromSeconds((lastJobInThisRound.RunAt - DateTime.Now).Seconds + 1);
                            
                            Console.WriteLine($"[Producer][WaitForNextJob] {intervalSeconds.TotalSeconds} seconds");
                        }
                        else
                        {
                            Console.WriteLine($"[Producer][Wait] {intervalSeconds.TotalSeconds} seconds");
                        }

                        await Task.Delay(intervalSeconds, stoppingToken);
                    }
                    catch (TaskCanceledException)
                    {
                        _stp.Dispose();
                        break;
                    }
                }
            }

            shutdown:
            Console.WriteLine("[System] In Levi's BackgroundService'");
            Console.WriteLine("[System] Shutdown event detected, stop worker service...");
        }
    }
}