using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using SchedulingPractice.Core;

namespace SubWorker.LeviDemo
{
    public class LeviSubWorkerBackgroundService : BackgroundService
    {
//        private readonly string _connectionString = "Server=.;Database=master;user id=SA;password=Levi123456";
        private readonly string _connectionString = null;
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

            using (var repo = new JobsRepo(_connectionString))
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
                            intervalSeconds =
                                TimeSpan.FromSeconds((lastJobInThisRound.RunAt - DateTime.Now).Seconds + 1);

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