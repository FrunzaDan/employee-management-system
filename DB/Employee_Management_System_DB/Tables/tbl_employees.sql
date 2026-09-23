CREATE TABLE [dbo].[tbl_employees]
(
    -- UNIQUEIDENTIFIER (16 bytes), not NVARCHAR(50) (72 bytes for a 36-char GUID): this key is
    -- the clustered index, so every nonclustered index below — and every FK pointing here —
    -- carries a copy of it. Values come from SequentialGuid (DataAccess), which orders the way
    -- SQL Server sorts GUIDs, so inserts append instead of splitting pages at random.
    [PK_employee_guid] UNIQUEIDENTIFIER NOT NULL,
    [first_name] NVARCHAR (50) NOT NULL,
    [last_name] NVARCHAR (50) NOT NULL,
    -- NOT NULL (not just the UQ_ constraints below): SQL Server treats every NULL as
    -- distinct under UNIQUE, so a NULL email/msisdn would silently bypass both the unique
    -- constraint and usp_createEmployee's own duplicate pre-check (`= NULL` never matches).
    -- 254 = the longest address RFC 5321 allows through SMTP.
    [email] NVARCHAR (254) NOT NULL,
    -- E.164 caps a phone number at 15 digits, and digits never need Unicode.
    [msisdn] VARCHAR (15) NOT NULL,
    -- 0 = not declared, 1 = male, 2 = female (see EmployeeModel's Gender enum).
    [gender] TINYINT NULL,
    [birthdate] DATE NULL,
    [employee_Status] SMALLINT NOT NULL CONSTRAINT [DF_tbl_employees_employee_Status] DEFAULT (1901),
    -- UTC. DATETIME2(3) (millisecond precision, 7 bytes) instead of DATETIME (8 bytes, 1/300s
    -- rounding) — and never a string: the old NVARCHAR columns silently stored GETDATE()'s
    -- default 'Sep 22 2026 12:53PM' text, which loses seconds and doesn't sort chronologically.
    [creation_Date] DATETIME2 (3) NOT NULL CONSTRAINT [DF_tbl_employees_creation_Date] DEFAULT (SYSUTCDATETIME()),
    [interaction_Date] DATETIME2 (3) NOT NULL CONSTRAINT [DF_tbl_employees_interaction_Date] DEFAULT (SYSUTCDATETIME()),
    -- Job info: independent of the address/status fields above and always optional at
    -- the schema level — usp_createEmployee/usp_editEmployee pre-check these guids exist
    -- (friendly 400) before they'd ever hit these FKs.
    [hire_Date] DATE NULL,
    [FK_office_guid] UNIQUEIDENTIFIER NULL,
    [FK_department_guid] UNIQUEIDENTIFIER NULL,
    [FK_cost_center_guid] UNIQUEIDENTIFIER NULL,
    CONSTRAINT [PK_tbl_employees] PRIMARY KEY CLUSTERED ([PK_employee_guid]),
    CONSTRAINT [UQ_tbl_employees_email] UNIQUE ([email]),
    CONSTRAINT [UQ_tbl_employees_msisdn] UNIQUE ([msisdn]),
    -- The code sets live here, not just in C#/TypeScript, so a bad value can't reach the
    -- table by any path (a hand-written script, a future proc) — see EmployeeStatus/Gender.
    CONSTRAINT [CK_tbl_employees_gender] CHECK ([gender] IN (0, 1, 2)),
    CONSTRAINT [CK_tbl_employees_employee_Status] CHECK ([employee_Status] IN (1901, 1903, 1904)),
    -- No ON DELETE CASCADE: deleting an office/department/cost-center while an employee
    -- still references it should be blocked (see usp_delete*'s friendly pre-check), not
    -- silently null out the employee's assignment.
    CONSTRAINT [FK_tbl_employees_tbl_offices] FOREIGN KEY ([FK_office_guid]) REFERENCES [dbo].[tbl_offices] ([PK_office_guid]),
    CONSTRAINT [FK_tbl_employees_tbl_departments] FOREIGN KEY ([FK_department_guid]) REFERENCES [dbo].[tbl_departments] ([PK_department_guid]),
    CONSTRAINT [FK_tbl_employees_tbl_cost_centers] FOREIGN KEY ([FK_cost_center_guid]) REFERENCES [dbo].[tbl_cost_centers] ([PK_cost_center_guid])
);
GO

-- Supports usp_getEmployees' default (no @SearchTerm) listing and its 'name' sort —
-- the common, unfiltered page-load path. Doesn't help the LIKE '%...%' search branches
-- (a leading wildcard can't seek any index), only the plain listing/sort case.
CREATE INDEX [IX_tbl_employees_last_first]
    ON [dbo].[tbl_employees] ([last_name], [first_name]);
GO

-- A FK doesn't index its own (referencing) column. Each of these backs, for its org
-- entity: usp_getEmployeesBy*'s filter + ORDER BY last_name, first_name (fully covered via
-- INCLUDE, so no key lookups), usp_get*s' per-entity employee join, and usp_delete*'s
-- "still assigned?" pre-check — all of which would otherwise scan tbl_employees.
CREATE INDEX [IX_tbl_employees_FK_office_guid]
    ON [dbo].[tbl_employees] ([FK_office_guid], [last_name], [first_name])
    INCLUDE ([email], [employee_Status]);
GO

CREATE INDEX [IX_tbl_employees_FK_department_guid]
    ON [dbo].[tbl_employees] ([FK_department_guid], [last_name], [first_name])
    INCLUDE ([email], [employee_Status]);
GO

CREATE INDEX [IX_tbl_employees_FK_cost_center_guid]
    ON [dbo].[tbl_employees] ([FK_cost_center_guid], [last_name], [first_name])
    INCLUDE ([email], [employee_Status]);
