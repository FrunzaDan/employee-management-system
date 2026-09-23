CREATE TABLE [dbo].[tbl_employee_salary_history]
(
    [PK_salary_guid]   UNIQUEIDENTIFIER NOT NULL,
    [FK_employee_guid] UNIQUEIDENTIFIER NOT NULL,
    -- Exact decimal, never FLOAT, for money. Max 9,999,999,999.99 — EmployeeSalary
    -- rejects anything larger (or with more than 2 decimals) with a clean 400.
    [brutto_salary]    DECIMAL (12, 2)  NOT NULL,
    -- A real DATE, so "ORDER BY effective_Date DESC" is chronological. As NVARCHAR it was
    -- only correct while every client happened to send zero-padded ISO text.
    [effective_Date]   DATE             NOT NULL,
    -- UTC. Also the tie-breaker when two entries share an effective date: the later-recorded
    -- one wins as "current".
    [created_Date]     DATETIME2 (3)    NOT NULL CONSTRAINT [DF_tbl_employee_salary_history_created_Date] DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT [PK_tbl_employee_salary_history] PRIMARY KEY CLUSTERED ([PK_salary_guid]),
    CONSTRAINT [CK_tbl_employee_salary_history_brutto_salary] CHECK ([brutto_salary] > 0),
    CONSTRAINT [FK_tbl_employee_salary_history_tbl_employees] FOREIGN KEY ([FK_employee_guid])
        REFERENCES [dbo].[tbl_employees] ([PK_employee_guid])
);
GO

-- Backs both "current salary" (TOP 1 ... ORDER BY effective_Date DESC, created_Date DESC in
-- usp_getEmployee/usp_getEmployees/usp_get*s) and usp_getEmployeeSalaryHistory's listing.
-- INCLUDE (brutto_salary) makes it covering: the per-employee TOP 1 is a single index seek
-- with no key lookup back into the clustered index.
CREATE INDEX [IX_tbl_employee_salary_history_employee_effective]
    ON [dbo].[tbl_employee_salary_history] ([FK_employee_guid], [effective_Date] DESC, [created_Date] DESC)
    INCLUDE ([brutto_salary]);
