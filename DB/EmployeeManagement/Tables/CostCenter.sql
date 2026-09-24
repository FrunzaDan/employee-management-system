CREATE TABLE [dbo].[CostCenter]
(
    [CostCenterId] UNIQUEIDENTIFIER NOT NULL
        CONSTRAINT [DF_CostCenter_CostCenterId] DEFAULT NEWSEQUENTIALID(),
    [Code] NVARCHAR (50) NOT NULL,
    [Name] NVARCHAR (100) NULL,
    CONSTRAINT [PK_CostCenter] PRIMARY KEY CLUSTERED ([CostCenterId]),
    CONSTRAINT [UQ_CostCenter_Code] UNIQUE ([Code])
);
