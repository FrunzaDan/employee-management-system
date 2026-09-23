CREATE TABLE [dbo].[EmployeeAuditLog]
(
    [EmployeeAuditLogId] INT IDENTITY (1, 1) NOT NULL,
    [EmployeeId] UNIQUEIDENTIFIER NOT NULL,
    -- The Employer.Username of whoever performed the action.
    [PerformedBy] NVARCHAR (50) NOT NULL,
    -- A fixed set of values (the API's AuditAction enum), so a short VARCHAR with a CHECK
    -- rather than free text.
    [ActionType] VARCHAR (20) NOT NULL,
    [Details] NVARCHAR (500) NULL,
    -- UTC; defaulted here so no proc has to remember to supply it.
    [OccurredAt] DATETIME2 (3) NOT NULL CONSTRAINT [DF_EmployeeAuditLog_OccurredAt] DEFAULT SYSUTCDATETIME(),
    CONSTRAINT [PK_EmployeeAuditLog] PRIMARY KEY CLUSTERED ([EmployeeAuditLogId]),
    CONSTRAINT [CK_EmployeeAuditLog_ActionType] CHECK ([ActionType] IN
        ('Created', 'Edited', 'Deactivated', 'Reactivated', 'Deleted', 'SalaryChanged'))
);
GO

-- No FK to Employee: audit history must survive an employee being hard-deleted
-- (see Employee_Delete / ai_docs/database.md), so it's a plain column, indexed for
-- the per-employee lookup EmployeeAuditLog_ListByEmployee does — keyed in that proc's
-- ORDER BY order so the "newest first" listing is read straight off the index with no sort.
CREATE INDEX [IX_EmployeeAuditLog_EmployeeId_OccurredAt_EmployeeAuditLogId]
    ON [dbo].[EmployeeAuditLog] ([EmployeeId], [OccurredAt] DESC, [EmployeeAuditLogId] DESC);
GO

-- Supports EmployeeAuditLog_List's global, unfiltered "newest first" scan
-- across every employee — the index above only helps once an EmployeeId is
-- known, which the global admin view doesn't have.
CREATE INDEX [IX_EmployeeAuditLog_OccurredAt_EmployeeAuditLogId]
    ON [dbo].[EmployeeAuditLog] ([OccurredAt] DESC, [EmployeeAuditLogId] DESC);
