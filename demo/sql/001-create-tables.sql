-- VIOLATION FILE
--
-- VIOLATES:
--   SQL-001 — Uses GUID (UNIQUEIDENTIFIER) as clustered primary key.
--             GUIDs cause severe index fragmentation and page splits.
--             Should use int/bigint IDENTITY as clustered PK with GUID
--             as a non-clustered unique index if needed externally.

CREATE TABLE [dbo].[Tasks] (
    -- VIOLATION SQL-001: GUID as clustered PK — causes fragmentation
    [Id]          UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    [Title]       NVARCHAR(200)    NOT NULL,
    [Description] NVARCHAR(2000)   NULL,
    [Status]      NVARCHAR(50)     NOT NULL DEFAULT 'Todo',
    [Priority]    INT              NOT NULL DEFAULT 5,
    [UserId]      NVARCHAR(100)    NOT NULL,
    [AssignedTo]  NVARCHAR(100)    NULL,
    [CreatedAt]   DATETIME2        NOT NULL DEFAULT SYSUTCDATETIME(),
    [DueDate]     DATETIME2        NULL,

    -- VIOLATION SQL-001: Clustered PK on GUID
    CONSTRAINT [PK_Tasks] PRIMARY KEY CLUSTERED ([Id])
);

-- Index on UserId for user scoping (correct)
CREATE NONCLUSTERED INDEX [IX_Tasks_UserId]
    ON [dbo].[Tasks] ([UserId]);

-- Index on Status for filtering (correct)
CREATE NONCLUSTERED INDEX [IX_Tasks_Status]
    ON [dbo].[Tasks] ([Status]);

-- Audit log table for Table Storage comparison
-- (actual audit logs go to Azure Table Storage, this is backup)
CREATE TABLE [dbo].[AuditLog] (
    -- VIOLATION SQL-001: Another GUID clustered PK
    [Id]        UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
    [UserId]    NVARCHAR(100)    NOT NULL,
    [Action]    NVARCHAR(100)    NOT NULL,
    [EntityId]  NVARCHAR(100)    NOT NULL,
    [Details]   NVARCHAR(500)    NULL,
    [Timestamp] DATETIME2        NOT NULL DEFAULT SYSUTCDATETIME(),

    CONSTRAINT [PK_AuditLog] PRIMARY KEY CLUSTERED ([Id])
);
