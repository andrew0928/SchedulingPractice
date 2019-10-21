using SchedulingPractice.Core;
using System;
using System.IO;
using System.Threading.Tasks;

namespace SchedulingPractice.PubWorker
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 5)
            {
                Console.WriteLine("Usage: PubWorker.exe [since] [duration] [runner] [mode] [csv-path]");
            }

            int since_sec = int.Parse(args[0]);
            int duration_sec = int.Parse(args[1]);

            string runner = args[2];
            string mode = args[3];
            string path = args[4];

            // 設定: 預定測試開始時間
            DateTime since = DateTime.Now.AddSeconds(since_sec);

            // 設定: 預定測試持續時間
            TimeSpan duration = TimeSpan.FromSeconds(duration_sec);



            // 計算: 預定測試預計結束時間
            DateTime until = since + duration;

            // 統計: 累計總任務數
            int total = 0;
            
            

            using (var repo = new JobsRepo())
            {
                Console.WriteLine($"Init test database...");
                Console.WriteLine($"- now:   {DateTime.Now}");
                Console.WriteLine($"- since: {since}");
                Console.WriteLine($"- until: {until}");

                Console.WriteLine();
                Console.WriteLine($"Step 0: reset database...");
                repo.ResetDatabase();



                // step 1: add one job per 3 sec
                {
                    Console.WriteLine();
                    Console.WriteLine($"Step 1: add job per 3 sec");

                    int count = 0;
                    DateTime current = since;
                    TimeSpan period = TimeSpan.FromSeconds(3);

                    while(current < until)
                    {
                        count++;
                        current = current + period;
                        Console.Write(".");
                        repo.CreateJob(current);
                    }

                    total += count;
                    Console.WriteLine();
                    Console.WriteLine($"- complete({count}).");
                }

                // step 2: add 20 job per 13 sec
                {
                    Console.WriteLine();
                    Console.WriteLine($"Step 2: add 20 jobs per 13 sec");

                    int count = 0;
                    DateTime current = since;
                    TimeSpan period = TimeSpan.FromSeconds(13);
                    int loop = 20;

                    while (current < until)
                    {
                        current = current + period;
                        for (int i = 0; i < loop; i++)
                        {
                            count++;
                            Console.Write(".");
                            repo.CreateJob(current);
                        }
                    }

                    total += count;
                    Console.WriteLine();
                    Console.WriteLine($"- complete({count}).");
                }

                // step 3: random add job per 1 ~ 3 sec
                {
                    Console.WriteLine();
                    Console.WriteLine($"Step 3: random add job per 1 ~ 3 sec");

                    int count = 0;
                    Random rnd = new Random();
                    DateTime current = since;

                    while (current < until)
                    {
                        current = current + TimeSpan.FromMilliseconds(1000 + rnd.Next(2000));
                        count++;
                        Console.Write(".");
                        repo.CreateJob(current);
                    }

                    total += count;
                    Console.WriteLine();
                    Console.WriteLine($"- complete({count}).");
                }





                // step 4, realtime: add a job scheduled after 10 sec, and random waiting 1 ~ 3 sec.
                {
                    Console.WriteLine();
                    Console.WriteLine($"Step 4: realtime: add a job scheduled after 10 sec, and random waiting 1 ~ 3 sec.");

                    int count = 0;
                    Random rnd = new Random();
                    

                    while (DateTime.Now.AddSeconds(10) < until)
                    {
                        Task.Delay(1000 + rnd.Next(2000)).Wait();
                        count++;
                        Console.Write(".");
                        repo.CreateJob(DateTime.Now + JobSettings.MinPrepareTime + TimeSpan.FromMilliseconds(100));
                    }

                    total += count;
                    Console.WriteLine();
                    Console.WriteLine($"- complete({count}).");
                }


                Console.WriteLine();
                Console.WriteLine($"Database initialization complete.");
                Console.WriteLine($"- total jobs: {total}");
                Console.WriteLine($"- now:   {DateTime.Now}");
                Console.WriteLine($"- since: {since}");
                Console.WriteLine($"- until: {until}");


                // step 5, wait 30 sec, show statistic
                Task.Delay(JobSettings.MaxDelayTime + JobSettings.MinPrepareTime).Wait();
                var metrics = repo.GetStatstics();

                Console.WriteLine();
                Console.WriteLine($"Jobs Scheduling Metrics:");

                Console.WriteLine();
                Console.WriteLine("--(action count)----------------------------------------------");
                Console.WriteLine($"- CREATE:             {metrics.count_action_create}");
                Console.WriteLine($"- ACQUIRE_SUCCESS:    {metrics.count_action_acquire_success}");
                Console.WriteLine($"- ACQUIRE_FAILURE:    {metrics.count_action_acquire_failure}");
                Console.WriteLine($"- COMPLETE:           {metrics.count_action_complete}");
                Console.WriteLine($"- QUERYJOB:           {metrics.count_action_queryjob}");
                Console.WriteLine($"- QUERYLIST:          {metrics.count_action_querylist}");

                Console.WriteLine();
                Console.WriteLine("--(state count)----------------------------------------------");
                Console.WriteLine($"- COUNT(CREATE):      {metrics.count_state_create}");
                Console.WriteLine($"- COUNT(LOCK):        {metrics.count_state_lock}");
                Console.WriteLine($"- COUNT(COMPLETE):    {metrics.count_action_complete}");

                Console.WriteLine();
                Console.WriteLine("--(statistics)----------------------------------------------");
                Console.WriteLine($"- DELAY(Average):     {metrics.stat_average_delay}");
                Console.WriteLine($"- DELAY(Stdev):       {metrics.stat_stdev_delay}");

                Console.WriteLine();
                Console.WriteLine("--(test result)----------------------------------------------");
                Console.WriteLine($"- Complete Job:       {metrics.count_action_complete == metrics.count_action_create}, {metrics.count_action_complete} / {metrics.count_action_create}");
                Console.WriteLine($"- Delay Too Long:     {metrics.stat_delay_exceed_count}");
                Console.WriteLine($"- Fail Job:           {metrics.count_state_lock == 0}, {metrics.count_state_lock}");

                Console.WriteLine();
                Console.WriteLine("--(benchmark score)----------------------------------------------");
                Console.WriteLine($"- Exec Cost Score:      {metrics.count_action_querylist * 100.0 + metrics.count_action_acquire_failure * 10.0 + metrics.count_action_queryjob * 1.0:#.##} (querylist x 100 + acquire-failure x 10 + queryjob x 1)");
                Console.WriteLine($"- Efficient Score:      {metrics.stat_average_delay + metrics.stat_stdev_delay:#.##} (average + stdev)");

                // RUNNER, MODE,
                // CREATE, ACQUIRE_SUCCESS, ACQUIRE_FAILURE, COMPLETE, QUERYJOB, QUERYLIST,
                // CREATE_COUNT, LOCK_COUNT, COMPLETE_COUNT,
                // DELAY_AVERAGE, DELAY_STDEV,
                // DELAY_EXCEED, EARLY_LOCK, EARLY_EXEC,
                // EXEC_COST_SCORE, EFFICIENT_SCORE,

                //string path = @"result-stat.csv";

                if (File.Exists(path) == false)
                {
                    File.AppendAllText(
                        path,
                        @"RUNNER, MODE, " +
                        @"CREATE, ACQUIRE_SUCCESS, ACQUIRE_FAILURE, COMPLETE, QUERYJOB, QUERYLIST, " +
                        @"CREATE_COUNT, LOCK_COUNT, COMPLETE_COUNT, " +
                        @"DELAY_AVERAGE, DELAY_STDEV, " +
                        @"DELAY_EXCEED, EARLY_LOCK, EARLY_EXEC, " +
                        "\n");
                }

                File.AppendAllText(
                    path, 
                    $"{Environment.GetEnvironmentVariable("RUNNER")}, {Environment.GetEnvironmentVariable("MODE")}," +
                    $"{metrics.count_action_create}, {metrics.count_action_acquire_success}, {metrics.count_action_acquire_failure}, {metrics.count_action_complete}, {metrics.count_action_queryjob}, {metrics.count_action_querylist}, " +
                    $"{metrics.count_state_create}, {metrics.count_state_lock}, {metrics.count_state_complete}, " +
                    $"{metrics.stat_average_delay}, {metrics.stat_stdev_delay}, " +
                    $"{metrics.stat_delay_exceed_count}, {metrics.count_state_early_lock}, {metrics.count_state_early_exec}, " +
                    "\n");
            }
        }
    }
}
