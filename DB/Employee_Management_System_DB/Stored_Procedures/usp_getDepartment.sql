CREATE PROCEDURE [dbo].[usp_getDepartment]
    @var_Guid NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT PK_department_guid, department_name
    FROM dbo.tbl_departments
    WHERE PK_department_guid = @var_Guid;
END
