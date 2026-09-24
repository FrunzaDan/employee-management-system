CREATE TABLE [dbo].[EmployeeAddress]
(
    [EmployeeId] UNIQUEIDENTIFIER NOT NULL,
    [Country] NVARCHAR (100) NOT NULL,
    [County] NVARCHAR (100) NOT NULL,
    [PostalCode] VARCHAR (20) NOT NULL,
    [City] NVARCHAR (100) NOT NULL,
    [Street] NVARCHAR (100) NOT NULL,
    [StreetNumber] NVARCHAR (50) NOT NULL,
    CONSTRAINT [PK_EmployeeAddress] PRIMARY KEY CLUSTERED ([EmployeeId]),
    CONSTRAINT [FK_EmployeeAddress_Employee]
        FOREIGN KEY ([EmployeeId]) REFERENCES [dbo].[Employee] ([EmployeeId])
);
