CREATE TABLE [dbo].[CostCenter]
(
    -- Same reasoning as Employee.EmployeeId: a real UNIQUEIDENTIFIER, generated
    -- sequentially here; CostCenter_Create hands the new value back.
    [CostCenterId] UNIQUEIDENTIFIER NOT NULL
        CONSTRAINT [DF_CostCenter_CostCenterId] DEFAULT NEWSEQUENTIALID(),
    [Code] NVARCHAR (50) NOT NULL,
    [Name] NVARCHAR (100) NULL,
    CONSTRAINT [PK_CostCenter] PRIMARY KEY CLUSTERED ([CostCenterId]),
    CONSTRAINT [UQ_CostCenter_Code] UNIQUE ([Code])
);
