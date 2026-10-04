CREATE TABLE [dbo].[EmployeeAuditLog]
(
    [EmployeeAuditLogId] INT IDENTITY (1, 1) NOT NULL,
    [EmployeeId] UNIQUEIDENTIFIER NOT NULL,
    [PerformedBy] NVARCHAR (50) NOT NULL,
    [ActionType] VARCHAR (20) NOT NULL,
    [Details] NVARCHAR (500) NULL,
    [OccurredAt] DATETIME2 (3) NOT NULL
        CONSTRAINT [DF_EmployeeAuditLog_OccurredAt] DEFAULT SYSUTCDATETIME(),
    CONSTRAINT [PK_EmployeeAuditLog] PRIMARY KEY CLUSTERED ([EmployeeAuditLogId]),
    CONSTRAINT [CK_EmployeeAuditLog_ActionType] CHECK ([ActionType] IN
        ('Created', 'Edited', 'Deactivated', 'Reactivated', 'Deleted', 'SalaryChanged'))
);
GO

CREATE INDEX [IX_EmployeeAuditLog_EmployeeId_OccurredAt_EmployeeAuditLogId]
    ON [dbo].[EmployeeAuditLog] ([EmployeeId], [OccurredAt] DESC, [EmployeeAuditLogId] DESC);
GO

CREATE INDEX [IX_EmployeeAuditLog_OccurredAt_EmployeeAuditLogId]
    ON [dbo].[EmployeeAuditLog] ([OccurredAt] DESC, [EmployeeAuditLogId] DESC);
