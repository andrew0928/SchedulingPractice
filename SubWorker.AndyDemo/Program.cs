using SchedulingPractice.Core;
using System.Linq;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace SubWorker.AndyDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            var host = new HostBuilder()
                .ConfigureServices((context, services) =>
                {
                    services.AddHostedService<AndySubWorkerBackgroundService>();
                })
                .Build();

            using (host)
            {
                host.Start();
                host.WaitForShutdown();
            }
        }
    }
    /* <<< Best Performance : 1 Process >>>
    Jobs Scheduling Metrics:

    --(action count)----------------------------------------------
    - CREATE:             1747
    - ACQUIRE_SUCCESS:    1747
    - ACQUIRE_FAILURE:    0
    - COMPLETE:           1747
    - QUERYJOB:           1747
    - QUERYLIST:          66

    --(state count)----------------------------------------------
    - COUNT(CREATE):      0
    - COUNT(LOCK):        0
    - COUNT(COMPLETE):    1747

    --(statistics)----------------------------------------------
    - DELAY(Average):     188
    - DELAY(Stdev):       116.2361386313

    --(test result)----------------------------------------------
    - Complete Job:       True, 1747 / 1747
    - Delay Too Long:     0
    - Fail Job:           True, 0

    --(benchmark score)----------------------------------------------
    - Exec Cost Score:      8347 (querylist x 100 + acquire-failure x 10 + queryjob x 1)
    - Efficient Score:      304.24 (average + stdev) 
    */
    /* <<< High Availability : 5 Process >>>
    Jobs Scheduling Metrics:

    --(action count)----------------------------------------------
    - CREATE:             1746
    - ACQUIRE_SUCCESS:    1746
    - ACQUIRE_FAILURE:    2541
    - COMPLETE:           1746
    - QUERYJOB:           7637
    - QUERYLIST:          330

    --(state count)----------------------------------------------
    - COUNT(CREATE):      0
    - COUNT(LOCK):        0
    - COUNT(COMPLETE):    1746

    --(statistics)----------------------------------------------
    - DELAY(Average):     129
    - DELAY(Stdev):       62.2094090562968

    --(test result)----------------------------------------------
    - Complete Job:       True, 1746 / 1746
    - Delay Too Long:     0
    - Fail Job:           True, 0

    --(benchmark score)----------------------------------------------
    - Exec Cost Score:      66047 (querylist x 100 + acquire-failure x 10 + queryjob x 1)
    - Efficient Score:      191.21 (average + stdev)
    */
}
