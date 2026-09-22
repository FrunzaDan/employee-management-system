-- Unpaginated, same reasoning as usp_getEmployeeSalaryHistory: bounded by how
-- many employees a single office realistically has, not a growing top-level list.
CREATE PROCEDURE [dbo].[usp_getEmployeesByOffice]
    @var_Guid NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT PK_employee_guid, first_name, last_name, email, employee_Status
    FROM dbo.tbl_employees
    WHERE FK_office_guid = @var_Guid
    ORDER BY last_name, first_name;
END
