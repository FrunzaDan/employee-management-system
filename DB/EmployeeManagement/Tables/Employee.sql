CREATE TABLE [dbo].[Employee]
(
    [EmployeeId] UNIQUEIDENTIFIER NOT NULL
        CONSTRAINT [DF_Employee_EmployeeId] DEFAULT NEWSEQUENTIALID(),
    [FirstName] NVARCHAR (100) NOT NULL,
    [LastName] NVARCHAR (100) NOT NULL,
    [Email] NVARCHAR (254) NOT NULL,
    [PhoneNumber] VARCHAR (15) NOT NULL,
    [Gender] TINYINT NOT NULL
        CONSTRAINT [DF_Employee_Gender] DEFAULT 0,
    [BirthDate] DATE NULL,
    [StatusCode] SMALLINT NOT NULL
        CONSTRAINT [DF_Employee_StatusCode] DEFAULT 1901,
    [StatusCodeBeforeDeactivation] SMALLINT NULL,
    [CreatedAt] DATETIME2 (3) NOT NULL
        CONSTRAINT [DF_Employee_CreatedAt] DEFAULT SYSUTCDATETIME(),
    [LastInteractionAt] DATETIME2 (3) NOT NULL
        CONSTRAINT [DF_Employee_LastInteractionAt] DEFAULT SYSUTCDATETIME(),
    [HireDate] DATE NULL,
    [OfficeId] UNIQUEIDENTIFIER NULL,
    [DepartmentId] UNIQUEIDENTIFIER NULL,
    [CostCenterId] UNIQUEIDENTIFIER NULL,
    CONSTRAINT [PK_Employee] PRIMARY KEY CLUSTERED ([EmployeeId]),
    CONSTRAINT [UQ_Employee_Email] UNIQUE ([Email]),
    CONSTRAINT [UQ_Employee_PhoneNumber] UNIQUE ([PhoneNumber]),
    CONSTRAINT [CK_Employee_Gender] CHECK ([Gender] IN (0, 1, 2)),
    CONSTRAINT [CK_Employee_StatusCode] CHECK ([StatusCode] IN (1901, 1903, 1904)),
    CONSTRAINT [CK_Employee_StatusCodeBeforeDeactivation]
        CHECK ([StatusCodeBeforeDeactivation] IN (1901, 1904)),
    CONSTRAINT [FK_Employee_Office]
        FOREIGN KEY ([OfficeId]) REFERENCES [dbo].[Office] ([OfficeId]),
    CONSTRAINT [FK_Employee_Department]
        FOREIGN KEY ([DepartmentId]) REFERENCES [dbo].[Department] ([DepartmentId]),
    CONSTRAINT [FK_Employee_CostCenter]
        FOREIGN KEY ([CostCenterId]) REFERENCES [dbo].[CostCenter] ([CostCenterId])
);
GO

CREATE INDEX [IX_Employee_LastName_FirstName]
    ON [dbo].[Employee] ([LastName], [FirstName]);
GO

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
