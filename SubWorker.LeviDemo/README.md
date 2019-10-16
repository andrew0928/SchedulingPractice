
# Levi's Solution

## Best Configuration

* Best Performance: 1 Process
* Consider HA: 2-3 processes
    
## Mindset

* Architecture: Producer -> Job Queue <- Consumers (use thread pool)
* Action Cost: query job list x 100 + acquire-failure x 10 + query job x 1

## Producer

1. Action: Fetch job then put to job queue
2. Fetching job is very expensive action, need to chose right time 
3. There are two fetching strategies
    1. If there is no job, fetch jobs after MinPrepareTime (10 seconds)
    2. If there is job in database, fetch jobs after LastJob.RunAt + 1 seconds. Because fetch job method already sort by RunAt Time.
        It could help us to reduce waiting time and get closer next job run time to fetch jobs

example:

    now: 00:00:00
    last job: 00:00:10
    wait for 11 seconds
    
## Consumer

1. Action get job from queue then process it
2. Check job status before Acquire and Process
3. Using job lock between threads to reduce the Acquire fail rate

## Test Report

Efficient Score is negative, because my PC and MSSQL Docker time is unsync, it should be find if use LocalDB on Windows

* 1 process
```
Jobs Scheduling Metrics:

--(action count)----------------------------------------------
- CREATE:             1743
- ACQUIRE_SUCCESS:    1743
- ACQUIRE_FAILURE:    0
- COMPLETE:           1743
- QUERYJOB:           1743
- QUERYLIST:          66

--(state count)----------------------------------------------
- COUNT(CREATE):      0
- COUNT(LOCK):        0
- COUNT(COMPLETE):    1743

--(statistics)----------------------------------------------
- DELAY(Average):     -4553
- DELAY(Stdev):       2729.29159429883

--(test result)----------------------------------------------
- Complete Job:       True, 1743 / 1743
- Delay Too Long:     0
- Fail Job:           True, 0

--(benchmark score)----------------------------------------------
- Exec Cost Score:      8343 (querylist x 100 + acquire-failure x 10 + queryjob x 1)
- Efficient Score:      -1823.71 (average + stdev)
```
    
* 2 processes
```
Jobs Scheduling Metrics:

--(action count)----------------------------------------------
- CREATE:             1738
- ACQUIRE_SUCCESS:    1738
- ACQUIRE_FAILURE:    336
- COMPLETE:           1738
- QUERYJOB:           3326
- QUERYLIST:          132

--(state count)----------------------------------------------
- COUNT(CREATE):      0
- COUNT(LOCK):        0
- COUNT(COMPLETE):    1738

--(statistics)----------------------------------------------
- DELAY(Average):     -4722
- DELAY(Stdev):       2831.11662610769

--(test result)----------------------------------------------
- Complete Job:       True, 1738 / 1738
- Delay Too Long:     0
- Fail Job:           True, 0

--(benchmark score)----------------------------------------------
- Exec Cost Score:      19886 (querylist x 100 + acquire-failure x 10 + queryjob x 1)
- Efficient Score:      -1890.88 (average + stdev)
```
   
* 3 processes
```
Jobs Scheduling Metrics:

--(action count)----------------------------------------------
- CREATE:             1725
- ACQUIRE_SUCCESS:    1725
- ACQUIRE_FAILURE:    149
- COMPLETE:           1725
- QUERYJOB:           2441
- QUERYLIST:          197

--(state count)----------------------------------------------
- COUNT(CREATE):      0
- COUNT(LOCK):        0
- COUNT(COMPLETE):    1725

--(statistics)----------------------------------------------
- DELAY(Average):     -6274
- DELAY(Stdev):       2397.41420469202

--(test result)----------------------------------------------
- Complete Job:       True, 1725 / 1725
- Delay Too Long:     0
- Fail Job:           True, 0

--(benchmark score)----------------------------------------------
- Exec Cost Score:      23631 (querylist x 100 + acquire-failure x 10 + queryjob x 1)
- Efficient Score:      -3876.59 (average + stdev)
```

4 processes
```
Jobs Scheduling Metrics:

--(action count)----------------------------------------------
- CREATE:             1740
- ACQUIRE_SUCCESS:    1740
- ACQUIRE_FAILURE:    27
- COMPLETE:           1740
- QUERYJOB:           1839
- QUERYLIST:          264

--(state count)----------------------------------------------
- COUNT(CREATE):      0
- COUNT(LOCK):        0
- COUNT(COMPLETE):    1740

--(statistics)----------------------------------------------
- DELAY(Average):     -7882
- DELAY(Stdev):       1261.61942364732

--(test result)----------------------------------------------
- Complete Job:       True, 1740 / 1740
- Delay Too Long:     0
- Fail Job:           True, 0

--(benchmark score)----------------------------------------------
- Exec Cost Score:      28509 (querylist x 100 + acquire-failure x 10 + queryjob x 1)
- Efficient Score:      -6620.38 (average + stdev)
```
     
* 5 processes

```
Jobs Scheduling Metrics:

--(action count)----------------------------------------------
- CREATE:             1749
- ACQUIRE_SUCCESS:    1749
- ACQUIRE_FAILURE:    85
- COMPLETE:           1749
- QUERYJOB:           2040
- QUERYLIST:          326

--(state count)----------------------------------------------
- COUNT(CREATE):      0
- COUNT(LOCK):        0
- COUNT(COMPLETE):    1749

--(statistics)----------------------------------------------
- DELAY(Average):     -7873
- DELAY(Stdev):       1004.50419146997

--(test result)----------------------------------------------
- Complete Job:       True, 1749 / 1749
- Delay Too Long:     0
- Fail Job:           True, 0

--(benchmark score)----------------------------------------------
- Exec Cost Score:      35490 (querylist x 100 + acquire-failure x 10 + queryjob x 1)
- Efficient Score:      -6868.5 (average + stdev)
```

