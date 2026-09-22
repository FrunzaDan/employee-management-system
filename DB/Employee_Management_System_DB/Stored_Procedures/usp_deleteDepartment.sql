CREATE PROCEDURE [dbo].[usp_deleteDepartment]
    @var_Guid NVARCHAR(50)
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

    IF EXISTS (SELECT 1 FROM tbl_employees WHERE FK_department_guid = @var_Guid)
    BEGIN
        SET @result = 409;
        SET @message = 'Department is assigned to one or more employees.';

        SELECT @result AS result, @message AS message;
        RETURN;
    END

    BEGIN TRY
        DELETE FROM tbl_departments WHERE PK_department_guid = @var_Guid;

        SET @result = 0;
        SET @message = 'Department deleted successfully.';
    END TRY
    BEGIN CATCH
        SET @result = 500;
        SET @message = CONCAT('Failed to delete department: ', ERROR_MESSAGE());
    END CATCH

    SELECT @result AS result, @message AS message;
END
