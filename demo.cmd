cls
set RUNNER=%1
:: available runner: demo | andrew0928 | andy19900208 | julian-chu | borischin | levichen | jwchen-dev | toyo0103 | acetaxxxx 

set SINCE=30
set DURATION=600
set TOTAL_DURATION=660
:: set DURATION=60
:: set TOTAL_DURATION=120
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




set MODE=WORKERS01

start /min dotnet SchedulingPractice.SubWorkerRunner\bin\Debug\netcoreapp2.2\SchedulingPractice.SubWorkerRunner.dll %RUNNER% 0 0 %TOTAL_DURATION%

dotnet SchedulingPractice.PubWorker\bin\Debug\netcoreapp2.2\SchedulingPractice.PubWorker.dll %SINCE% %DURATION% %RUNNER% %MODE% %STATISTIC_CSV% > logs\result-%RUNNER%-%MODE%.txt
powershell sleep 30




set MODE=WORKERS02

start /min dotnet SchedulingPractice.SubWorkerRunner\bin\Debug\netcoreapp2.2\SchedulingPractice.SubWorkerRunner.dll %RUNNER% 0 0 %TOTAL_DURATION%
start /min dotnet SchedulingPractice.SubWorkerRunner\bin\Debug\netcoreapp2.2\SchedulingPractice.SubWorkerRunner.dll %RUNNER% 0 0 %TOTAL_DURATION%

dotnet SchedulingPractice.PubWorker\bin\Debug\netcoreapp2.2\SchedulingPractice.PubWorker.dll %SINCE% %DURATION% %RUNNER% %MODE% %STATISTIC_CSV% > logs\result-%RUNNER%-%MODE%.txt
powershell sleep 30




set MODE=WORKERS03

start /min dotnet SchedulingPractice.SubWorkerRunner\bin\Debug\netcoreapp2.2\SchedulingPractice.SubWorkerRunner.dll %RUNNER% 0 0 %TOTAL_DURATION%
start /min dotnet SchedulingPractice.SubWorkerRunner\bin\Debug\netcoreapp2.2\SchedulingPractice.SubWorkerRunner.dll %RUNNER% 0 0 %TOTAL_DURATION%
start /min dotnet SchedulingPractice.SubWorkerRunner\bin\Debug\netcoreapp2.2\SchedulingPractice.SubWorkerRunner.dll %RUNNER% 0 0 %TOTAL_DURATION%

dotnet SchedulingPractice.PubWorker\bin\Debug\netcoreapp2.2\SchedulingPractice.PubWorker.dll %SINCE% %DURATION% %RUNNER% %MODE% %STATISTIC_CSV% > logs\result-%RUNNER%-%MODE%.txt
powershell sleep 30




set MODE=WORKERS04

start /min dotnet SchedulingPractice.SubWorkerRunner\bin\Debug\netcoreapp2.2\SchedulingPractice.SubWorkerRunner.dll %RUNNER% 0 0 %TOTAL_DURATION%
start /min dotnet SchedulingPractice.SubWorkerRunner\bin\Debug\netcoreapp2.2\SchedulingPractice.SubWorkerRunner.dll %RUNNER% 0 0 %TOTAL_DURATION%
start /min dotnet SchedulingPractice.SubWorkerRunner\bin\Debug\netcoreapp2.2\SchedulingPractice.SubWorkerRunner.dll %RUNNER% 0 0 %TOTAL_DURATION%
start /min dotnet SchedulingPractice.SubWorkerRunner\bin\Debug\netcoreapp2.2\SchedulingPractice.SubWorkerRunner.dll %RUNNER% 0 0 %TOTAL_DURATION%

dotnet SchedulingPractice.PubWorker\bin\Debug\netcoreapp2.2\SchedulingPractice.PubWorker.dll %SINCE% %DURATION% %RUNNER% %MODE% %STATISTIC_CSV% > logs\result-%RUNNER%-%MODE%.txt
powershell sleep 30




set MODE=WORKERS05

start /min dotnet SchedulingPractice.SubWorkerRunner\bin\Debug\netcoreapp2.2\SchedulingPractice.SubWorkerRunner.dll %RUNNER% 0 0 %TOTAL_DURATION%
start /min dotnet SchedulingPractice.SubWorkerRunner\bin\Debug\netcoreapp2.2\SchedulingPractice.SubWorkerRunner.dll %RUNNER% 0 0 %TOTAL_DURATION%
start /min dotnet SchedulingPractice.SubWorkerRunner\bin\Debug\netcoreapp2.2\SchedulingPractice.SubWorkerRunner.dll %RUNNER% 0 0 %TOTAL_DURATION%
start /min dotnet SchedulingPractice.SubWorkerRunner\bin\Debug\netcoreapp2.2\SchedulingPractice.SubWorkerRunner.dll %RUNNER% 0 0 %TOTAL_DURATION%
start /min dotnet SchedulingPractice.SubWorkerRunner\bin\Debug\netcoreapp2.2\SchedulingPractice.SubWorkerRunner.dll %RUNNER% 0 0 %TOTAL_DURATION%

dotnet SchedulingPractice.PubWorker\bin\Debug\netcoreapp2.2\SchedulingPractice.PubWorker.dll %SINCE% %DURATION% %RUNNER% %MODE% %STATISTIC_CSV% > logs\result-%RUNNER%-%MODE%.txt
powershell sleep 30



:: goto end



set MODE=WORKERS06

start /min dotnet SchedulingPractice.SubWorkerRunner\bin\Debug\netcoreapp2.2\SchedulingPractice.SubWorkerRunner.dll %RUNNER% 0 0 %TOTAL_DURATION%
start /min dotnet SchedulingPractice.SubWorkerRunner\bin\Debug\netcoreapp2.2\SchedulingPractice.SubWorkerRunner.dll %RUNNER% 0 0 %TOTAL_DURATION%
start /min dotnet SchedulingPractice.SubWorkerRunner\bin\Debug\netcoreapp2.2\SchedulingPractice.SubWorkerRunner.dll %RUNNER% 0 0 %TOTAL_DURATION%
start /min dotnet SchedulingPractice.SubWorkerRunner\bin\Debug\netcoreapp2.2\SchedulingPractice.SubWorkerRunner.dll %RUNNER% 0 0 %TOTAL_DURATION%
start /min dotnet SchedulingPractice.SubWorkerRunner\bin\Debug\netcoreapp2.2\SchedulingPractice.SubWorkerRunner.dll %RUNNER% 0 0 %TOTAL_DURATION%
start /min dotnet SchedulingPractice.SubWorkerRunner\bin\Debug\netcoreapp2.2\SchedulingPractice.SubWorkerRunner.dll %RUNNER% 0 0 %TOTAL_DURATION%

dotnet SchedulingPractice.PubWorker\bin\Debug\netcoreapp2.2\SchedulingPractice.PubWorker.dll %SINCE% %DURATION% %RUNNER% %MODE% %STATISTIC_CSV% > logs\result-%RUNNER%-%MODE%.txt
powershell sleep 30



set MODE=WORKERS07

start /min dotnet SchedulingPractice.SubWorkerRunner\bin\Debug\netcoreapp2.2\SchedulingPractice.SubWorkerRunner.dll %RUNNER% 0 0 %TOTAL_DURATION%
start /min dotnet SchedulingPractice.SubWorkerRunner\bin\Debug\netcoreapp2.2\SchedulingPractice.SubWorkerRunner.dll %RUNNER% 0 0 %TOTAL_DURATION%
start /min dotnet SchedulingPractice.SubWorkerRunner\bin\Debug\netcoreapp2.2\SchedulingPractice.SubWorkerRunner.dll %RUNNER% 0 0 %TOTAL_DURATION%
start /min dotnet SchedulingPractice.SubWorkerRunner\bin\Debug\netcoreapp2.2\SchedulingPractice.SubWorkerRunner.dll %RUNNER% 0 0 %TOTAL_DURATION%
start /min dotnet SchedulingPractice.SubWorkerRunner\bin\Debug\netcoreapp2.2\SchedulingPractice.SubWorkerRunner.dll %RUNNER% 0 0 %TOTAL_DURATION%
start /min dotnet SchedulingPractice.SubWorkerRunner\bin\Debug\netcoreapp2.2\SchedulingPractice.SubWorkerRunner.dll %RUNNER% 0 0 %TOTAL_DURATION%
start /min dotnet SchedulingPractice.SubWorkerRunner\bin\Debug\netcoreapp2.2\SchedulingPractice.SubWorkerRunner.dll %RUNNER% 0 0 %TOTAL_DURATION%

dotnet SchedulingPractice.PubWorker\bin\Debug\netcoreapp2.2\SchedulingPractice.PubWorker.dll %SINCE% %DURATION% %RUNNER% %MODE% %STATISTIC_CSV% > logs\result-%RUNNER%-%MODE%.txt
powershell sleep 30



set MODE=WORKERS08

start /min dotnet SchedulingPractice.SubWorkerRunner\bin\Debug\netcoreapp2.2\SchedulingPractice.SubWorkerRunner.dll %RUNNER% 0 0 %TOTAL_DURATION%
start /min dotnet SchedulingPractice.SubWorkerRunner\bin\Debug\netcoreapp2.2\SchedulingPractice.SubWorkerRunner.dll %RUNNER% 0 0 %TOTAL_DURATION%
start /min dotnet SchedulingPractice.SubWorkerRunner\bin\Debug\netcoreapp2.2\SchedulingPractice.SubWorkerRunner.dll %RUNNER% 0 0 %TOTAL_DURATION%
start /min dotnet SchedulingPractice.SubWorkerRunner\bin\Debug\netcoreapp2.2\SchedulingPractice.SubWorkerRunner.dll %RUNNER% 0 0 %TOTAL_DURATION%
start /min dotnet SchedulingPractice.SubWorkerRunner\bin\Debug\netcoreapp2.2\SchedulingPractice.SubWorkerRunner.dll %RUNNER% 0 0 %TOTAL_DURATION%
start /min dotnet SchedulingPractice.SubWorkerRunner\bin\Debug\netcoreapp2.2\SchedulingPractice.SubWorkerRunner.dll %RUNNER% 0 0 %TOTAL_DURATION%
start /min dotnet SchedulingPractice.SubWorkerRunner\bin\Debug\netcoreapp2.2\SchedulingPractice.SubWorkerRunner.dll %RUNNER% 0 0 %TOTAL_DURATION%
start /min dotnet SchedulingPractice.SubWorkerRunner\bin\Debug\netcoreapp2.2\SchedulingPractice.SubWorkerRunner.dll %RUNNER% 0 0 %TOTAL_DURATION%

dotnet SchedulingPractice.PubWorker\bin\Debug\netcoreapp2.2\SchedulingPractice.PubWorker.dll %SINCE% %DURATION% %RUNNER% %MODE% %STATISTIC_CSV% > logs\result-%RUNNER%-%MODE%.txt
powershell sleep 30



set MODE=WORKERS09

start /min dotnet SchedulingPractice.SubWorkerRunner\bin\Debug\netcoreapp2.2\SchedulingPractice.SubWorkerRunner.dll %RUNNER% 0 0 %TOTAL_DURATION%
start /min dotnet SchedulingPractice.SubWorkerRunner\bin\Debug\netcoreapp2.2\SchedulingPractice.SubWorkerRunner.dll %RUNNER% 0 0 %TOTAL_DURATION%
start /min dotnet SchedulingPractice.SubWorkerRunner\bin\Debug\netcoreapp2.2\SchedulingPractice.SubWorkerRunner.dll %RUNNER% 0 0 %TOTAL_DURATION%
start /min dotnet SchedulingPractice.SubWorkerRunner\bin\Debug\netcoreapp2.2\SchedulingPractice.SubWorkerRunner.dll %RUNNER% 0 0 %TOTAL_DURATION%
start /min dotnet SchedulingPractice.SubWorkerRunner\bin\Debug\netcoreapp2.2\SchedulingPractice.SubWorkerRunner.dll %RUNNER% 0 0 %TOTAL_DURATION%
start /min dotnet SchedulingPractice.SubWorkerRunner\bin\Debug\netcoreapp2.2\SchedulingPractice.SubWorkerRunner.dll %RUNNER% 0 0 %TOTAL_DURATION%
start /min dotnet SchedulingPractice.SubWorkerRunner\bin\Debug\netcoreapp2.2\SchedulingPractice.SubWorkerRunner.dll %RUNNER% 0 0 %TOTAL_DURATION%
start /min dotnet SchedulingPractice.SubWorkerRunner\bin\Debug\netcoreapp2.2\SchedulingPractice.SubWorkerRunner.dll %RUNNER% 0 0 %TOTAL_DURATION%
start /min dotnet SchedulingPractice.SubWorkerRunner\bin\Debug\netcoreapp2.2\SchedulingPractice.SubWorkerRunner.dll %RUNNER% 0 0 %TOTAL_DURATION%

dotnet SchedulingPractice.PubWorker\bin\Debug\netcoreapp2.2\SchedulingPractice.PubWorker.dll %SINCE% %DURATION% %RUNNER% %MODE% %STATISTIC_CSV% > logs\result-%RUNNER%-%MODE%.txt
powershell sleep 30



set MODE=WORKERS10

start /min dotnet SchedulingPractice.SubWorkerRunner\bin\Debug\netcoreapp2.2\SchedulingPractice.SubWorkerRunner.dll %RUNNER% 0 0 %TOTAL_DURATION%
start /min dotnet SchedulingPractice.SubWorkerRunner\bin\Debug\netcoreapp2.2\SchedulingPractice.SubWorkerRunner.dll %RUNNER% 0 0 %TOTAL_DURATION%
start /min dotnet SchedulingPractice.SubWorkerRunner\bin\Debug\netcoreapp2.2\SchedulingPractice.SubWorkerRunner.dll %RUNNER% 0 0 %TOTAL_DURATION%
start /min dotnet SchedulingPractice.SubWorkerRunner\bin\Debug\netcoreapp2.2\SchedulingPractice.SubWorkerRunner.dll %RUNNER% 0 0 %TOTAL_DURATION%
start /min dotnet SchedulingPractice.SubWorkerRunner\bin\Debug\netcoreapp2.2\SchedulingPractice.SubWorkerRunner.dll %RUNNER% 0 0 %TOTAL_DURATION%
start /min dotnet SchedulingPractice.SubWorkerRunner\bin\Debug\netcoreapp2.2\SchedulingPractice.SubWorkerRunner.dll %RUNNER% 0 0 %TOTAL_DURATION%
start /min dotnet SchedulingPractice.SubWorkerRunner\bin\Debug\netcoreapp2.2\SchedulingPractice.SubWorkerRunner.dll %RUNNER% 0 0 %TOTAL_DURATION%
start /min dotnet SchedulingPractice.SubWorkerRunner\bin\Debug\netcoreapp2.2\SchedulingPractice.SubWorkerRunner.dll %RUNNER% 0 0 %TOTAL_DURATION%
start /min dotnet SchedulingPractice.SubWorkerRunner\bin\Debug\netcoreapp2.2\SchedulingPractice.SubWorkerRunner.dll %RUNNER% 0 0 %TOTAL_DURATION%
start /min dotnet SchedulingPractice.SubWorkerRunner\bin\Debug\netcoreapp2.2\SchedulingPractice.SubWorkerRunner.dll %RUNNER% 0 0 %TOTAL_DURATION%

dotnet SchedulingPractice.PubWorker\bin\Debug\netcoreapp2.2\SchedulingPractice.PubWorker.dll %SINCE% %DURATION% %RUNNER% %MODE% %STATISTIC_CSV% > logs\result-%RUNNER%-%MODE%.txt
powershell sleep 30


:end