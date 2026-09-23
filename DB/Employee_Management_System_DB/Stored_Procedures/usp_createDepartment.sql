CREATE PROCEDURE [dbo].[usp_createDepartment]
    @var_Guid UNIQUEIDENTIFIER,
    @var_DepartmentName NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @result INT;
    DECLARE @message NVARCHAR(255);

    BEGIN TRY
        INSERT INTO dbo.tbl_departments (PK_department_guid, department_name)
        VALUES (@var_Guid, @var_DepartmentName);

        SET @result = 0;
        SET @message = CONCAT('Department created successfully. GUID: ', @var_Guid);
    END TRY
    BEGIN CATCH
        SET @result = 500;
        SET @message = CONCAT('Failed to create department: ', ERROR_MESSAGE());
    END CATCH

    SELECT @result AS result, @message AS message;
END
