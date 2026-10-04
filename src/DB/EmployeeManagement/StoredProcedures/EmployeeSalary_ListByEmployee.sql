CREATE PROCEDURE [dbo].[EmployeeSalary_ListByEmployee]
    @EmployeeId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    SELECT EmployeeSalaryId, EmployeeId, GrossSalary, EffectiveDate, CreatedAt
    FROM dbo.EmployeeSalary
    WHERE EmployeeId = @EmployeeId
    ORDER BY EffectiveDate DESC, CreatedAt DESC;
END
