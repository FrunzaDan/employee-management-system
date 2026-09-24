CREATE PROCEDURE [dbo].[Employee_ListByCostCenter]
    @CostCenterId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    SELECT EmployeeId, FirstName, LastName, Email, StatusCode
    FROM dbo.Employee
    WHERE CostCenterId = @CostCenterId
    ORDER BY LastName, FirstName;
END
