cls
set RUNNER=jim

set SINCE=30
set DURATION=600
set TOTAL_DURATION=660
set STATISTIC_CSV=%CD%\logs\statistics.csv

mkdir logs
del /f logs\result-%RUNNER%-*.txt




set MODE=HATEST
start /min dotnet SchedulingPractice.SubWorkerRunner\bin\Debug\netcoreapp2.2\SchedulingPractice.SubWorkerRunner.dll %RUNNER% 10 30 %TOTAL_DURATION%
start /min dotnet SchedulingPractice.SubWorkerRunner\bin\Debug\netcoreapp2.2\SchedulingPractice.SubWorkerRunner.dll %RUNNER% 10 30 %TOTAL_DURATION%
start /min dotnet SchedulingPractice.SubWorkerRunner\bin\Debug\netcoreapp2.2\SchedulingPractice.SubWorkerRunner.dll %RUNNER% 10 30 %TOTAL_DURATION%
start /min dotnet SchedulingPractice.SubWorkerRunner\bin\Debug\netcoreapp2.2\SchedulingPractice.SubWorkerRunner.dll %RUNNER% 10 30 %TOTAL_DURATION%
start /min dotnet SchedulingPractice.SubWorkerRunner\bin\Debug\netcoreapp2.2\SchedulingPractice.SubWorkerRunner.dll %RUNNER% 10 30 %TOTAL_DURATION%

dotnet SchedulingPractice.PubWorker\bin\Debug\netcoreapp2.2\SchedulingPractice.PubWorker.dll %SINCE% %DURATION% %RUNNER% %MODE% %STATISTIC_CSV% > logs\result-%RUNNER%-%MODE%.txt
powershell sleep 30


:end