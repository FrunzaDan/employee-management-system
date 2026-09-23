-- Unpaginated, same reasoning as EmployeeAuditLog_ListByEmployee: bounded by one employee's
-- history, which stays small in practice.
CREATE PROCEDURE [dbo].[EmployeeSalary_ListByEmployee]
    @EmployeeId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT EmployeeSalaryId, EmployeeId, GrossSalary, EffectiveDate, CreatedAt
    FROM dbo.EmployeeSalary
    WHERE EmployeeId = @EmployeeId
    ORDER BY EffectiveDate DESC, CreatedAt DESC;
END
