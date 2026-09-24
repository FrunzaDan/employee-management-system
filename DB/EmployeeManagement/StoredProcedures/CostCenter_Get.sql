CREATE PROCEDURE [dbo].[CostCenter_Get]
    @CostCenterId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    SELECT CostCenterId, Code, Name
    FROM dbo.CostCenter
    WHERE CostCenterId = @CostCenterId;
END
