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
