CREATE PROCEDURE [dbo].[Department_Delete]
    @DepartmentId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @Result INT;
    DECLARE @Message NVARCHAR(255);

    IF NOT EXISTS (SELECT 1 FROM dbo.Department WHERE DepartmentId = @DepartmentId)
    BEGIN
        SET @Result = 404;
        SET @Message = 'Department not found.';

        SELECT @Result AS Result, @Message AS Message;
        RETURN;
    END

    IF EXISTS (SELECT 1 FROM dbo.Employee WHERE DepartmentId = @DepartmentId)
    BEGIN
        SET @Result = 409;
        SET @Message = 'Department is assigned to one or more employees.';

        SELECT @Result AS Result, @Message AS Message;
        RETURN;
    END

    DELETE FROM dbo.Department WHERE DepartmentId = @DepartmentId;

    SET @Result = 0;
    SET @Message = 'Department deleted successfully.';

    SELECT @Result AS Result, @Message AS Message;
END
