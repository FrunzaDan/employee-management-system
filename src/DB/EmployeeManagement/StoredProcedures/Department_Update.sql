CREATE PROCEDURE [dbo].[Department_Update]
    @DepartmentId UNIQUEIDENTIFIER,
    @Name NVARCHAR(100) = NULL
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

    UPDATE dbo.Department
    SET Name = ISNULL(@Name, Name)
    WHERE DepartmentId = @DepartmentId;

    SET @Result = 0;
    SET @Message = 'Department updated successfully.';

    SELECT @Result AS Result, @Message AS Message;
END
