CREATE PROCEDURE [dbo].[Office_Delete]
    @OfficeId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @Result INT;
    DECLARE @Message NVARCHAR(255);

    IF NOT EXISTS (SELECT 1 FROM dbo.Office WHERE OfficeId = @OfficeId)
    BEGIN
        SET @Result = 404;
        SET @Message = 'Office not found.';

        SELECT @Result AS Result, @Message AS Message;
        RETURN;
    END

    IF EXISTS (SELECT 1 FROM dbo.Employee WHERE OfficeId = @OfficeId)
    BEGIN
        SET @Result = 409;
        SET @Message = 'Office is assigned to one or more employees.';

        SELECT @Result AS Result, @Message AS Message;
        RETURN;
    END

    DELETE FROM dbo.Office WHERE OfficeId = @OfficeId;

    SET @Result = 0;
    SET @Message = 'Office deleted successfully.';

    SELECT @Result AS Result, @Message AS Message;
END
