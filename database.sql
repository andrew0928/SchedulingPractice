CREATE TABLE [dbo].[Jobs] (
    [Id]        INT      IDENTITY (1, 1) NOT NULL,
    [RunAt]     DATETIME NOT NULL,
    [CreateAt]  DATETIME DEFAULT (getdate()) NOT NULL,
    [LockAt]    DATETIME NULL,
    [ExecuteAt] DATETIME NULL,
    [State]     INT      DEFAULT ((0)) NOT NULL,
    PRIMARY KEY CLUSTERED ([Id] ASC)
);


GO
CREATE NONCLUSTERED INDEX [IX_Table_Column]
    ON [dbo].[Jobs]([RunAt] ASC, [State] ASC);



CREATE TABLE [dbo].[WorkerLogs] (
    [Sn]       INT           IDENTITY (1, 1) NOT NULL,
    [JobId]   INT           NULL,
    [LogDate]  DATETIME      DEFAULT (getdate()) NOT NULL,
    [Action]   NVARCHAR (50) NOT NULL,
    [ClientId] NVARCHAR (50) NULL,
    [Results] INT NULL, 
    PRIMARY KEY CLUSTERED ([Sn] ASC)
);


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'FK, Jobs ID', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'WorkerLogs', @level2type = N'COLUMN', @level2name = N'JobId';


GO
EXECUTE sp_addextendedproperty @name = N'MS_Description', @value = N'執行哪個動作留下的 LOG, CREATE | ACQUIRE_SUCCESS | ACQUIRE_FAILURE | COMPLETE | QUERYJOB | QUERYLIST', @level0type = N'SCHEMA', @level0name = N'dbo', @level1type = N'TABLE', @level1name = N'WorkerLogs', @level2type = N'COLUMN', @level2name = N'Action';

