CREATE TABLE [dbo].[Employee]
(
    -- UNIQUEIDENTIFIER (16 bytes), not NVARCHAR(50) (72 bytes for a 36-char GUID): this key is
    -- the clustered index, so every nonclustered index below — and every FK pointing here —
    -- carries a copy of it. Values come from SequentialGuid (DataAccess), which orders the way
    -- SQL Server sorts GUIDs, so inserts append instead of splitting pages at random.
    [EmployeeId] UNIQUEIDENTIFIER NOT NULL,
    [FirstName] NVARCHAR (50) NOT NULL,
    [LastName] NVARCHAR (50) NOT NULL,
    -- NOT NULL (not just the UQ_ constraints below): SQL Server treats every NULL as
    -- distinct under UNIQUE, so a NULL Email/PhoneNumber would silently bypass both the unique
    -- constraint and Employee_Create's own duplicate pre-check (`= NULL` never matches).
    -- 254 = the longest address RFC 5321 allows through SMTP.
    [Email] NVARCHAR (254) NOT NULL,
    -- The MSISDN: E.164 caps a phone number at 15 digits, and digits never need Unicode.
    [PhoneNumber] VARCHAR (15) NOT NULL,
    -- 0 = not declared, 1 = male, 2 = female (see EmployeeModel's Gender enum).
    [Gender] TINYINT NULL,
    [BirthDate] DATE NULL,
    [StatusCode] SMALLINT NOT NULL CONSTRAINT [DF_Employee_StatusCode] DEFAULT (1901),
    -- UTC. DATETIME2(3) (millisecond precision, 7 bytes) instead of DATETIME (8 bytes, 1/300s
    -- rounding) — and never a string: the old NVARCHAR columns silently stored GETDATE()'s
    -- default 'Sep 22 2026 12:53PM' text, which loses seconds and doesn't sort chronologically.
    [CreatedAt] DATETIME2 (3) NOT NULL CONSTRAINT [DF_Employee_CreatedAt] DEFAULT (SYSUTCDATETIME()),
    [LastInteractionAt] DATETIME2 (3) NOT NULL CONSTRAINT [DF_Employee_LastInteractionAt] DEFAULT (SYSUTCDATETIME()),
    -- Job info: independent of the address/status fields above and always optional at
    -- the schema level — Employee_Create/Employee_Update pre-check these keys exist
    -- (friendly 400) before they'd ever hit these FKs.
    [HireDate] DATE NULL,
    [OfficeId] UNIQUEIDENTIFIER NULL,
    [DepartmentId] UNIQUEIDENTIFIER NULL,
    [CostCenterId] UNIQUEIDENTIFIER NULL,
    CONSTRAINT [PK_Employee] PRIMARY KEY CLUSTERED ([EmployeeId]),
    CONSTRAINT [UQ_Employee_Email] UNIQUE ([Email]),
    CONSTRAINT [UQ_Employee_PhoneNumber] UNIQUE ([PhoneNumber]),
    -- The code sets live here, not just in C#/TypeScript, so a bad value can't reach the
    -- table by any path (a hand-written script, a future proc) — see EmployeeStatus/Gender.
    CONSTRAINT [CK_Employee_Gender] CHECK ([Gender] IN (0, 1, 2)),
    CONSTRAINT [CK_Employee_StatusCode] CHECK ([StatusCode] IN (1901, 1903, 1904)),
    -- No ON DELETE CASCADE: deleting an office/department/cost-center while an employee
    -- still references it should be blocked (see <Entity>_Delete's friendly pre-check), not
    -- silently null out the employee's assignment.
    CONSTRAINT [FK_Employee_Office] FOREIGN KEY ([OfficeId]) REFERENCES [dbo].[Office] ([OfficeId]),
    CONSTRAINT [FK_Employee_Department] FOREIGN KEY ([DepartmentId]) REFERENCES [dbo].[Department] ([DepartmentId]),
    CONSTRAINT [FK_Employee_CostCenter] FOREIGN KEY ([CostCenterId]) REFERENCES [dbo].[CostCenter] ([CostCenterId])
);
GO

-- Supports Employee_List's default (no @SearchTerm) listing and its 'name' sort —
-- the common, unfiltered page-load path. Doesn't help the LIKE '%...%' search branches
-- (a leading wildcard can't seek any index), only the plain listing/sort case.
CREATE INDEX [IX_Employee_LastName_FirstName]
    ON [dbo].[Employee] ([LastName], [FirstName]);
GO

-- A FK doesn't index its own (referencing) column. Each of these backs, for its org
-- entity: Employee_ListBy*'s filter + ORDER BY LastName, FirstName (fully covered via
-- INCLUDE, so no key lookups), <Entity>_List's per-entity employee join, and <Entity>_Delete's
-- "still assigned?" pre-check — all of which would otherwise scan Employee.
CREATE INDEX [IX_Employee_OfficeId_LastName_FirstName]
    ON [dbo].[Employee] ([OfficeId], [LastName], [FirstName])
    INCLUDE ([Email], [StatusCode]);
GO

CREATE INDEX [IX_Employee_DepartmentId_LastName_FirstName]
    ON [dbo].[Employee] ([DepartmentId], [LastName], [FirstName])
    INCLUDE ([Email], [StatusCode]);
GO

CREATE INDEX [IX_Employee_CostCenterId_LastName_FirstName]
    ON [dbo].[Employee] ([CostCenterId], [LastName], [FirstName])
    INCLUDE ([Email], [StatusCode]);
