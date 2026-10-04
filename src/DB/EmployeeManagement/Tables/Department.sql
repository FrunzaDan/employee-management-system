CREATE TABLE [dbo].[Department]
(
    [DepartmentId] UNIQUEIDENTIFIER NOT NULL
        CONSTRAINT [DF_Department_DepartmentId] DEFAULT NEWSEQUENTIALID(),
    [Name] NVARCHAR (100) NOT NULL,
    CONSTRAINT [PK_Department] PRIMARY KEY CLUSTERED ([DepartmentId])
);
