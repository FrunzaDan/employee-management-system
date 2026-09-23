CREATE TABLE [dbo].[CostCenter]
(
    [CostCenterId] UNIQUEIDENTIFIER NOT NULL,
    [Code] NVARCHAR (50) NOT NULL,
    [Name] NVARCHAR (100) NULL,
    CONSTRAINT [PK_CostCenter] PRIMARY KEY CLUSTERED ([CostCenterId]),
    CONSTRAINT [UQ_CostCenter_Code] UNIQUE ([Code])
);
