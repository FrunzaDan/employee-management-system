-- One row per salary entry in an employee's history; the "current" salary is the latest one.
CREATE TABLE [dbo].[EmployeeSalary]
(
    [EmployeeSalaryId] UNIQUEIDENTIFIER NOT NULL,
    [EmployeeId]       UNIQUEIDENTIFIER NOT NULL,
    -- Gross ("brutto") salary. Exact decimal, never FLOAT, for money. Max 9,999,999,999.99 —
    -- the API's EmployeeSalary (BusinessLogic) rejects anything larger (or with more than 2
    -- decimals) with a clean 400.
    [GrossSalary]      DECIMAL (12, 2)  NOT NULL,
    -- A real DATE, so "ORDER BY EffectiveDate DESC" is chronological. As NVARCHAR it was
    -- only correct while every client happened to send zero-padded ISO text.
    [EffectiveDate]    DATE             NOT NULL,
    -- UTC. Also the tie-breaker when two entries share an effective date: the later-recorded
    -- one wins as "current".
    [CreatedAt]        DATETIME2 (3)    NOT NULL CONSTRAINT [DF_EmployeeSalary_CreatedAt] DEFAULT (SYSUTCDATETIME()),
    CONSTRAINT [PK_EmployeeSalary] PRIMARY KEY CLUSTERED ([EmployeeSalaryId]),
    CONSTRAINT [CK_EmployeeSalary_GrossSalary] CHECK ([GrossSalary] > 0),
    CONSTRAINT [FK_EmployeeSalary_Employee] FOREIGN KEY ([EmployeeId])
        REFERENCES [dbo].[Employee] ([EmployeeId])
);
GO

-- Backs both "current salary" (TOP 1 ... ORDER BY EffectiveDate DESC, CreatedAt DESC in
-- Employee_Get/Employee_List/<Entity>_List) and EmployeeSalary_ListByEmployee's listing.
-- INCLUDE (GrossSalary) makes it covering: the per-employee TOP 1 is a single index seek
-- with no key lookup back into the clustered index.
CREATE INDEX [IX_EmployeeSalary_EmployeeId_EffectiveDate_CreatedAt]
    ON [dbo].[EmployeeSalary] ([EmployeeId], [EffectiveDate] DESC, [CreatedAt] DESC)
    INCLUDE ([GrossSalary]);
