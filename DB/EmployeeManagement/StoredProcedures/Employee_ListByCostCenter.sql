-- Unpaginated, same reasoning as EmployeeSalary_ListByEmployee: bounded by how
-- many employees a single cost center realistically has, not a growing top-level list.
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
