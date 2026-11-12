## Version 1.0

Without early lock.

Environment :

| Item     | Configuration        |
|----------|----------------------|
| OS       | Windows 11 Pro       |
| CPU      | AMD Ryzen 3700X      |
| RAM      | ADATA DDR4 3200 32GB |
| Storage  | ADATA 1TB M.2        |
| Database | LocalDB              | 
| Runtime  | Net 6                |

### One Instance

```
Jobs Scheduling Metrics:

--(action count)----------------------------------------------
- CREATE:             1736
- ACQUIRE_SUCCESS:    1736
- ACQUIRE_FAILURE:    0
- COMPLETE:           1736
- QUERYJOB:           1737
- QUERYLIST:          65

--(state count)----------------------------------------------
- COUNT(CREATE):      0
- COUNT(LOCK):        0
- COUNT(COMPLETE):    1736

--(statistics)----------------------------------------------
- DELAY(Average):     113
- DELAY(Stdev):       31.2237044096519

--(test result)----------------------------------------------
- Complete Job:       True, 1736 / 1736
- Delay Too Long:     0
- Fail Job:           True, 0

--(benchmark score)----------------------------------------------
- Exec Cost Score:      8237 (querylist x 100 + acquire-failure x 10 + queryjob x 1)
- Efficient Score:      144.22 (average + stdev)

```

### Three Instance

```Jobs Scheduling Metrics:

--(action count)----------------------------------------------
- CREATE:             1758
- ACQUIRE_SUCCESS:    1758
- ACQUIRE_FAILURE:    2445
- COMPLETE:           1758
- QUERYJOB:           5275
- QUERYLIST:          195

--(state count)----------------------------------------------
- COUNT(CREATE):      0
- COUNT(LOCK):        0
- COUNT(COMPLETE):    1758

--(statistics)----------------------------------------------
- DELAY(Average):     101
- DELAY(Stdev):       6.79669291539491

--(test result)----------------------------------------------
- Complete Job:       True, 1758 / 1758
- Delay Too Long:     0
- Fail Job:           True, 0

--(benchmark score)----------------------------------------------
- Exec Cost Score:      49225 (querylist x 100 + acquire-failure x 10 + queryjob x 1)
- Efficient Score:      107.8 (average + stdev)

Process finished with exit code 0.
```
Environment :

| Item     | Configuration        |
|----------|----------------------|
| OS       | Windows 11 Pro       |
| CPU      | AMD Ryzen 3700X      |
| RAM      | ADATA DDR4 3200 32GB |
| Storage  | ADATA 1TB M.2        |
| Database | LocalDB              | 
| Runtime  | Net Core 2.2         |

### One Instance

```
Jobs Scheduling Metrics:

--(action count)----------------------------------------------
- CREATE:             1749
- ACQUIRE_SUCCESS:    1749
- ACQUIRE_FAILURE:    1
- COMPLETE:           1749
- QUERYJOB:           1754
- QUERYLIST:          65

--(state count)----------------------------------------------
- COUNT(CREATE):      0
- COUNT(LOCK):        0
- COUNT(COMPLETE):    1749

--(statistics)----------------------------------------------
- DELAY(Average):     113
- DELAY(Stdev):       30.7915205815142

--(test result)----------------------------------------------
- Complete Job:       True, 1749 / 1749
- Delay Too Long:     0
- Fail Job:           True, 0

--(benchmark score)----------------------------------------------
- Exec Cost Score:      8264 (querylist x 100 + acquire-failure x 10 + queryjob x 1)
- Efficient Score:      143.79 (average + stdev)

Process finished with exit code 0.
```

### Three Instance

```
Jobs Scheduling Metrics:

--(action count)----------------------------------------------
- CREATE:             1736
- ACQUIRE_SUCCESS:    1736
- ACQUIRE_FAILURE:    2767
- COMPLETE:           1736
- QUERYJOB:           5209
- QUERYLIST:          195

--(state count)----------------------------------------------
- COUNT(CREATE):      0
- COUNT(LOCK):        0
- COUNT(COMPLETE):    1736

--(statistics)----------------------------------------------
- DELAY(Average):     101
- DELAY(Stdev):       8.04448627812101

--(test result)----------------------------------------------
- Complete Job:       True, 1736 / 1736
- Delay Too Long:     0
- Fail Job:           True, 0

--(benchmark score)----------------------------------------------
- Exec Cost Score:      52379 (querylist x 100 + acquire-failure x 10 + queryjob x 1)
- Efficient Score:      109.04 (average + stdev)

Process finished with exit code 0.
```

### Five Instances
```
Jobs Scheduling Metrics:

--(action count)----------------------------------------------
- CREATE:             1740
- ACQUIRE_SUCCESS:    1740
- ACQUIRE_FAILURE:    5264
- COMPLETE:           1740
- QUERYJOB:           8699
- QUERYLIST:          323

--(state count)----------------------------------------------
- COUNT(CREATE):      0
- COUNT(LOCK):        0
- COUNT(COMPLETE):    1740

--(statistics)----------------------------------------------
- DELAY(Average):     102
- DELAY(Stdev):       8.63974181166169

--(test result)----------------------------------------------
- Complete Job:       True, 1740 / 1740
- Delay Too Long:     0
- Fail Job:           True, 0

--(benchmark score)----------------------------------------------
- Exec Cost Score:      93639 (querylist x 100 + acquire-failure x 10 + queryjob x 1)
- Efficient Score:      110.64 (average + stdev)

Process finished with exit code 0.

```
