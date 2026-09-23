CREATE PROCEDURE [dbo].[Office_Update]
    @OfficeId UNIQUEIDENTIFIER,
    @Name NVARCHAR(100) = NULL,
    @City NVARCHAR(100) = NULL,
    @Country NVARCHAR(100) = NULL
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

    BEGIN TRY
        UPDATE dbo.Office
        SET
            Name = ISNULL(@Name, Name),
            City = ISNULL(@City, City),
            Country = ISNULL(@Country, Country)
        WHERE OfficeId = @OfficeId;

        SET @Result = 0;
        SET @Message = 'Office updated successfully.';
    END TRY
    BEGIN CATCH
        SET @Result = 500;
        SET @Message = CONCAT('Failed to update office: ', ERROR_MESSAGE());
    END CATCH

    SELECT @Result AS Result, @Message AS Message;
END
