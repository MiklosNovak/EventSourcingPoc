IF NOT EXISTS (
    SELECT 1
    FROM sys.databases
    WHERE name = N'BankAccountWriterDb'
)
BEGIN
    PRINT N'Creating database [BankAccountWriterDb]...';
    CREATE DATABASE [BankAccountWriterDb];
END
GO

USE [BankAccountWriterDb];
GO

IF NOT EXISTS (
    SELECT 1
    FROM sys.tables
    WHERE name = N'AccountEvents'
      AND schema_id = SCHEMA_ID(N'dbo')
)
BEGIN
    PRINT N'Creating table [dbo].[AccountEvents]...';

    CREATE TABLE dbo.AccountEvents (

  -- Global, gapless ordering of all events
        SequenceId    BIGINT           IDENTITY(1,1)   PRIMARY KEY,
        
        -- Unique event identifier for idempotency
        EventId       UNIQUEIDENTIFIER NOT NULL,

        -- Aggregate key (email-based AccountId)
        AccountId     NVARCHAR(200)    NOT NULL,

        -- Per-aggregate, gapless version for concurrency
        Version       INT              NOT NULL,

        -- Domain event type (e.g. 'MoneyDeposited')
        EventType     NVARCHAR(100)    NOT NULL,

        -- JSON payload containing event data
        Data          NVARCHAR(MAX)    NOT NULL,

        -- Timestamp when the event occurred
        OccurredAt    DATETIME2        NOT NULL                CONSTRAINT DF_AccountEvents_OccurredAt
                       DEFAULT SYSUTCDATETIME(),

        -- Tracks payload schema version for evolution
        SchemaVersion INT              NOT NULL
                       CONSTRAINT DF_AccountEvents_SchemaVersion
                       DEFAULT 1,

        -- Enforce one Version per AccountId stream
        CONSTRAINT UQ_AccountEvents_AccountId_Version
            UNIQUE (AccountId, Version)
    );

    -- Index to speed up per-account stream replay
    CREATE NONCLUSTERED INDEX IX_AccountEvents_AccountId_SequenceId
        ON dbo.AccountEvents (AccountId, SequenceId);
END
GO
