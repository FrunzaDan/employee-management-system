CREATE PROCEDURE [dbo].[Office_Delete]
    @OfficeId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Result INT;
    DECLARE @Message NVARCHAR(255);

    IF NOT EXISTS (SELECT 1 FROM dbo.Office WHERE OfficeId = @OfficeId)
    BEGIN
        SET @Result = 404;
        SET @Message = 'Office not found.';

        SELECT @Result AS Result, @Message AS Message;
        RETURN;
    END

    -- Friendly pre-check instead of letting Employee's FK throw a raw 500.
    IF EXISTS (SELECT 1 FROM dbo.Employee WHERE OfficeId = @OfficeId)
    BEGIN
        SET @Result = 409;
        SET @Message = 'Office is assigned to one or more employees.';

        SELECT @Result AS Result, @Message AS Message;
        RETURN;
    END

    BEGIN TRY
        DELETE FROM dbo.Office WHERE OfficeId = @OfficeId;

        SET @Result = 0;
        SET @Message = 'Office deleted successfully.';
    END TRY
    BEGIN CATCH
        SET @Result = 500;
        SET @Message = CONCAT('Failed to delete office: ', ERROR_MESSAGE());
    END CATCH

    SELECT @Result AS Result, @Message AS Message;
END
