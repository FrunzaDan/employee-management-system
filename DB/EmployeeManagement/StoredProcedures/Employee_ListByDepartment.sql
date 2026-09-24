-- Unpaginated, same reasoning as EmployeeSalary_ListByEmployee: bounded by how
-- many employees a single department realistically has, not a growing top-level list.
CREATE PROCEDURE [dbo].[Employee_ListByDepartment]
    @DepartmentId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    SELECT EmployeeId, FirstName, LastName, Email, StatusCode
    FROM dbo.Employee
    WHERE DepartmentId = @DepartmentId
    ORDER BY LastName, FirstName;
END
