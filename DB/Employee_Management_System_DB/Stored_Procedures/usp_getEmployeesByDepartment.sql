-- Unpaginated, same reasoning as usp_getEmployeeSalaryHistory: bounded by how
-- many employees a single department realistically has, not a growing top-level list.
CREATE PROCEDURE [dbo].[usp_getEmployeesByDepartment]
    @var_Guid UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT PK_employee_guid, first_name, last_name, email, employee_Status
    FROM dbo.tbl_employees
    WHERE FK_department_guid = @var_Guid
    ORDER BY last_name, first_name;
END
