CREATE TABLE [dbo].[Department]
(
    [DepartmentId] UNIQUEIDENTIFIER NOT NULL,
    [Name] NVARCHAR (100) NOT NULL,
    CONSTRAINT [PK_Department] PRIMARY KEY CLUSTERED ([DepartmentId])
);
