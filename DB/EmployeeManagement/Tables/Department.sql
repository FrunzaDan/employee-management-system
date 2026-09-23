CREATE TABLE [dbo].[Department]
(
    -- Same reasoning as Employee.EmployeeId: a real UNIQUEIDENTIFIER, generated
    -- sequentially here; Department_Create hands the new value back.
    [DepartmentId] UNIQUEIDENTIFIER NOT NULL
        CONSTRAINT [DF_Department_DepartmentId] DEFAULT NEWSEQUENTIALID(),
    [Name] NVARCHAR (100) NOT NULL,
    CONSTRAINT [PK_Department] PRIMARY KEY CLUSTERED ([DepartmentId])
);
