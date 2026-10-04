CREATE TABLE [dbo].[EmployeeSalary]
(
    [EmployeeSalaryId] INT IDENTITY (1, 1) NOT NULL,
    [EmployeeId] UNIQUEIDENTIFIER NOT NULL,
    [GrossSalary] DECIMAL (12, 2) NOT NULL,
    [EffectiveDate] DATE NOT NULL,
    [CreatedAt] DATETIME2 (3) NOT NULL
        CONSTRAINT [DF_EmployeeSalary_CreatedAt] DEFAULT SYSUTCDATETIME(),
    CONSTRAINT [PK_EmployeeSalary] PRIMARY KEY CLUSTERED ([EmployeeSalaryId]),
    CONSTRAINT [CK_EmployeeSalary_GrossSalary] CHECK ([GrossSalary] > 0),
    CONSTRAINT [FK_EmployeeSalary_Employee]
        FOREIGN KEY ([EmployeeId]) REFERENCES [dbo].[Employee] ([EmployeeId])
);
GO

CREATE INDEX [IX_EmployeeSalary_EmployeeId_EffectiveDate_CreatedAt]
    ON [dbo].[EmployeeSalary] ([EmployeeId], [EffectiveDate] DESC, [CreatedAt] DESC)
    INCLUDE ([GrossSalary]);
