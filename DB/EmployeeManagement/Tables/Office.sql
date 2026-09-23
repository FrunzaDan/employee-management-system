CREATE TABLE [dbo].[Office]
(
    -- Same reasoning as Employee.EmployeeId: a real UNIQUEIDENTIFIER, generated
    -- sequentially here; Office_Create hands the new value back.
    [OfficeId] UNIQUEIDENTIFIER NOT NULL
        CONSTRAINT [DF_Office_OfficeId] DEFAULT NEWSEQUENTIALID(),
    [Name] NVARCHAR (100) NOT NULL,
    [City] NVARCHAR (100) NULL,
    [Country] NVARCHAR (100) NULL,
    CONSTRAINT [PK_Office] PRIMARY KEY CLUSTERED ([OfficeId])
);
