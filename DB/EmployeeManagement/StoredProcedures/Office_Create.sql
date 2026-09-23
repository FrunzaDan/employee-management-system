CREATE PROCEDURE [dbo].[Office_Create]
    @OfficeId UNIQUEIDENTIFIER,
    @Name NVARCHAR(100),
    @City NVARCHAR(100) = NULL,
    @Country NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Result INT;
    DECLARE @Message NVARCHAR(255);

    BEGIN TRY
        INSERT INTO dbo.Office (OfficeId, Name, City, Country)
        VALUES (@OfficeId, @Name, @City, @Country);

        SET @Result = 0;
        SET @Message = CONCAT('Office created successfully. GUID: ', @OfficeId);
    END TRY
    BEGIN CATCH
        SET @Result = 500;
        SET @Message = CONCAT('Failed to create office: ', ERROR_MESSAGE());
    END CATCH

    SELECT @Result AS Result, @Message AS Message;
END
