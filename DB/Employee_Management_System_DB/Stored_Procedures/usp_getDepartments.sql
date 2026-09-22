CREATE PROCEDURE [dbo].[usp_getDepartments]
AS
BEGIN
    SET NOCOUNT ON;

    SELECT PK_department_guid, department_name
    FROM dbo.tbl_departments
    ORDER BY department_name;
END
