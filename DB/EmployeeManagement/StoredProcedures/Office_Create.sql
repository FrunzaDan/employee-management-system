CREATE PROCEDURE [dbo].[Office_Create]
    @Name NVARCHAR(100),
    @City NVARCHAR(100) = NULL,
    @Country NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @Result INT;
    DECLARE @Message NVARCHAR(255);
    DECLARE @OfficeId UNIQUEIDENTIFIER = NULL;
    DECLARE @Inserted TABLE (OfficeId UNIQUEIDENTIFIER);

    INSERT INTO dbo.Office (Name, City, Country)
    OUTPUT inserted.OfficeId INTO @Inserted
    VALUES (@Name, @City, @Country);

    SELECT @OfficeId = OfficeId FROM @Inserted;

    SET @Result = 0;
    SET @Message = 'Office created successfully.';

    SELECT @Result AS Result, @Message AS Message, @OfficeId AS OfficeId;
END
