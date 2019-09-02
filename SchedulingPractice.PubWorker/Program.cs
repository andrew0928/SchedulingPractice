using SchedulingPractice.Core;
using System;
using System.Threading.Tasks;

namespace SchedulingPractice.PubWorker
{
    class Program
    {
        static void Main(string[] args)
        {
            TimeSpan duration = TimeSpan.FromMinutes(10);
            DateTime since = DateTime.Now.AddSeconds(10);
            DateTime until = since + duration;
            int total = 0;
            
            

            using (var repo = new JobsRepo(@"Data Source=localhost\SQLEXPRESS01;Initial Catalog=JobsDB;Integrated Security=True;Pooling=False"))
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
                    //DateTime current = since;

                    while (DateTime.Now.AddSeconds(10) < until)
                    {
                        Task.Delay(1000 + rnd.Next(2000)).Wait();
                        count++;
                        Console.Write(".");
                        repo.CreateJob(DateTime.Now.AddSeconds(10));
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
                Task.Delay(30 * 1000).Wait();
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
            }
        }
    }
}
