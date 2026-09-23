CREATE TABLE [dbo].[tbl_employee_audit_log]
(
    [audit_id] INT IDENTITY (1, 1) NOT NULL,
    [employee_guid] UNIQUEIDENTIFIER NOT NULL,
    [employer_id] NVARCHAR (50) NOT NULL,
    -- Fixed, app-defined ASCII verbs ("Created", "Salary changed", ...).
    [action] VARCHAR (50) NOT NULL,
    [details] NVARCHAR (500) NULL,
    -- UTC; defaulted here so no proc has to remember to supply it.
    [action_Date] DATETIME2 (3) NOT NULL CONSTRAINT [DF_tbl_employee_audit_log_action_Date] DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT [PK_tbl_employee_audit_log] PRIMARY KEY CLUSTERED ([audit_id])
);
GO

-- No FK to tbl_employees: audit history must survive a employee being hard-deleted
-- (see usp_deleteEmployee / ai_docs/database.md), so it's a plain column, indexed for
-- the per-employee lookup usp_getEmployeeAuditLog does — keyed in that proc's ORDER BY
-- order so the "newest first" listing is read straight off the index with no sort.
CREATE INDEX [IX_tbl_employee_audit_log_employee_guid]
    ON [dbo].[tbl_employee_audit_log] ([employee_guid], [action_Date] DESC, [audit_id] DESC);
GO

-- Supports usp_getAllEmployeeAuditLog's global, unfiltered "newest first" scan
-- across every employee — the index above only helps once a employee_guid is
-- known, which the global admin view doesn't have.
CREATE INDEX [IX_tbl_employee_audit_log_action_Date]
    ON [dbo].[tbl_employee_audit_log] ([action_Date] DESC, [audit_id] DESC);
