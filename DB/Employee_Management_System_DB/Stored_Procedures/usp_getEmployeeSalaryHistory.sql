-- Unpaginated, same reasoning as usp_getEmployeeAuditLog: bounded by one employee's
-- history, which stays small in practice.
CREATE PROCEDURE [dbo].[usp_getEmployeeSalaryHistory]
    @var_EmployeeGuid UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT PK_salary_guid, FK_employee_guid, brutto_salary, effective_Date, created_Date
    FROM dbo.tbl_employee_salary_history
    WHERE FK_employee_guid = @var_EmployeeGuid
    ORDER BY effective_Date DESC, created_Date DESC;
END
