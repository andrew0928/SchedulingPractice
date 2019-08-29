# 排程處理的練習



# 目標:

所有排程的時間安排紀錄，都放在 jobs 這 table 內 (schema 請參考下方說明)。這練習的主要目標是，在 jobs database 只支援被動查詢的限制下，
請設計 worker 完成任務。

任務的需求 (必須全部都滿足):

1. 所有 job 都必須在指定時間範圍內被啟動 (預約時間 + 可接受的延遲範圍)
1. 每個 job 都只能被執行一次, 不能有處理到一半掛掉的狀況
1. 必須支援分散式執行 (多組 worker, 能分散負載, 能互相備援, 支援動態 scaling)

評斷處理機制的品質指標 (按照優先順序):

1. 必須滿足所有上述任務需求
1. Jobs 清單查詢的次數越少越好
1. Jobs 嘗試執行失敗 (搶不到lock) 的次數越少越好
1. Jobs 延遲的時間越短越好 (延遲: 實際啟動時間 - 預約啟動時間)
  * 延遲平均值, 越小越好
  * 延遲標準差, 越小越好
1. 個別 job 狀態查詢的次數 (指定 job id) 越少越好

最終評分 = Sum(指標 x 權重):
1. 執行成本, 越低越好:
```querylist x 100.0 + acquire_failure x 10.0 + queryjob x 1.0```
1. 延遲程度, 越低越好:
```average + stdev```


# 環境準備

執行本範例程式，需要額外準備 SQL database. 我自己使用 LocalDB, 可以正常運行。

1. 請先建立 database: ```JobsDB```
2. 請用 sql script: [database.txt](database.txt) 建立 table (只有兩個表格: jobs / workerlogs)
3. 若未特別指定，程式碼預設會使用這連接字串: ```Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=JobsDB;Integrated Security=True;Pooling=False```


# 解題規範

沒有任何限制, 你只要參考 project: ```SchedulingPractice.Core``` 專案, 並且善用 ```JobsRepo``` 類別, 想辦法寫出滿足需求的程式即可。

# 驗證方式

本練習題提供兩個 project:

1. SchedulingPractice.Core, class library, 提供必要的函示庫 (主要是 JobsRepo)
1. SchedulingPRactice.PubWorker, console app, 建立測試資料，與顯示統計資訊的 console app

請用下列方式測試你的程式是否符合要求。

## 驗證可靠度 (HA)

1. 按照說明準備環境 (只需要做一次)
1. 啟動 SchedulingPRactice.PubWorker, 預設會執行 10 min, 每次執行會清除資料庫內容, 請耐心等候
1. 請同時啟動 5 份你的程式
1. 執行啟動 1 min 之後，請隨機按下 CTRL-C 任意中斷你的程式
1. 等待 SchedulingPRactice.PubWorker 執行完畢，顯示統計結果。確認 "test result" 數值:
  1. ```Complete Job``` 是否為 100% ? 
  1. ```Delay Too Long``` 是否為 0 ?
  1. ```Fail Job``` 是否為 0 ?


## 驗證效率

1. 按照說明準備環境 (只需要做一次)
1. 啟動 SchedulingPRactice.PubWorker, 預設會執行 10 min, 每次執行會清除資料庫內容, 請耐心等候
1. 請同時啟動 5 份你的程式
1. 等待 SchedulingPRactice.PubWorker 執行完畢，顯示統計結果。紀錄 cost / efficient score. 兩者的分數越低越好。

## 找出最佳組合

按照上述 "驗證效率" 的程序，改變同時啟動程式的套數 (預設 5), 找出 score 數值最低的組態, 連同 PR 時一起附上。