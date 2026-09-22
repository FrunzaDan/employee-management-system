CREATE TABLE [dbo].[tbl_employee_audit_log]
(
    [audit_id] INT IDENTITY (1, 1) NOT NULL,
    [employee_guid] NVARCHAR (50) NOT NULL,
    [employer_id] NVARCHAR (50) NOT NULL,
    [action] NVARCHAR (50) NOT NULL,
    [details] NVARCHAR (500) NULL,
    [action_Date] DATETIME NOT NULL,
    PRIMARY KEY (audit_id)
);
GO

-- No FK to tbl_employees: audit history must survive a employee being hard-deleted
-- (see usp_deleteEmployee / ai_docs/database.md), so it's a plain
-- NVARCHAR column, indexed for the per-employee lookup usp_getEmployeeAuditLog does.
CREATE INDEX [IX_tbl_employee_audit_log_employee_guid]
    ON [dbo].[tbl_employee_audit_log] ([employee_guid]);
GO

-- Supports usp_getAllEmployeeAuditLog's global, unfiltered "newest first" scan
-- across every employee — the index above only helps once a employee_guid is
-- known, which the global admin view doesn't have.
CREATE INDEX [IX_tbl_employee_audit_log_action_Date]
    ON [dbo].[tbl_employee_audit_log] ([action_Date] DESC, [audit_id] DESC);
