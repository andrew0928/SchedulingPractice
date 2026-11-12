using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace SubWorker.JimDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            var host = new HostBuilder()
                .ConfigureServices((context, services) =>
                {
                    services.AddHostedService<JimSubWorkerBackgroundService>();
                })
                .Build();

            using (host)
            {
                host.Start();
                host.WaitForShutdown();
            }
        }
    }
}
/* <<< Best Performance : 1 Process >>>
Jobs Scheduling Metrics:

--(action count)----------------------------------------------
- CREATE:             1767
- ACQUIRE_SUCCESS:    1767
- ACQUIRE_FAILURE:    0
- COMPLETE:           1767
- QUERYJOB:           1767
- QUERYLIST:          66

--(state count)----------------------------------------------
- COUNT(CREATE):      0
- COUNT(LOCK):        0
- COUNT(COMPLETE):    1767

--(statistics)----------------------------------------------
- DELAY(Average):     189
- DELAY(Stdev):       113.67526669485

--(test result)----------------------------------------------
- Complete Job:       True, 1767 / 1767
- Delay Too Long:     0
- Fail Job:           True, 0

--(benchmark score)----------------------------------------------
- Exec Cost Score:      8367 (querylist x 100 + acquire-failure x 10 + queryjob x 1)
- Efficient Score:      302.68 (average + stdev)
*/
/* <<< High Availability : 5 Process >>>
Jobs Scheduling Metrics:

--(action count)----------------------------------------------
- CREATE:             1752
- ACQUIRE_SUCCESS:    1752
- ACQUIRE_FAILURE:    1598
- COMPLETE:           1752
- QUERYJOB:           6069
- QUERYLIST:          314

--(state count)----------------------------------------------
- COUNT(CREATE):      0
- COUNT(LOCK):        0
- COUNT(COMPLETE):    1752

--(statistics)----------------------------------------------
- DELAY(Average):     141
- DELAY(Stdev):       134.610298136809

--(test result)----------------------------------------------
- Complete Job:       True, 1752 / 1752
- Delay Too Long:     0
- Fail Job:           True, 0

--(benchmark score)----------------------------------------------
- Exec Cost Score:      53449 (querylist x 100 + acquire-failure x 10 + queryjob x 1)
- Efficient Score:      275.61 (average + stdev)

*/
