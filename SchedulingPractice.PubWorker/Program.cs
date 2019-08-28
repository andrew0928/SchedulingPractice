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
            }
        }
    }
}
