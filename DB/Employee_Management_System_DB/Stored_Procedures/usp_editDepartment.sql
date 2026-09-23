CREATE PROCEDURE [dbo].[usp_editDepartment]
    @var_Guid UNIQUEIDENTIFIER,
    @var_DepartmentName NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @result INT;
    DECLARE @message NVARCHAR(255);

    IF NOT EXISTS (SELECT 1 FROM tbl_departments WHERE PK_department_guid = @var_Guid)
    BEGIN
        SET @result = 404;
        SET @message = 'Department not found.';

        SELECT @result AS result, @message AS message;
        RETURN;
    END

    BEGIN TRY
        UPDATE tbl_departments
        SET department_name = ISNULL(@var_DepartmentName, department_name)
        WHERE PK_department_guid = @var_Guid;

        SET @result = 0;
        SET @message = 'Department updated successfully.';
    END TRY
    BEGIN CATCH
        SET @result = 500;
        SET @message = CONCAT('Failed to update department: ', ERROR_MESSAGE());
    END CATCH

    SELECT @result AS result, @message AS message;
END
