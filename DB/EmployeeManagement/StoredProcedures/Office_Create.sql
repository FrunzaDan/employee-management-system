CREATE PROCEDURE [dbo].[Office_Create]
    @Name NVARCHAR(100),
    @City NVARCHAR(100) = NULL,
    @Country NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Result INT;
    DECLARE @Message NVARCHAR(255);
    DECLARE @OfficeId UNIQUEIDENTIFIER = NULL;
    DECLARE @Inserted TABLE (OfficeId UNIQUEIDENTIFIER);

    BEGIN TRY
        -- OfficeId comes from the table's NEWSEQUENTIALID() default.
        INSERT INTO dbo.Office (Name, City, Country)
        OUTPUT inserted.OfficeId INTO @Inserted
        VALUES (@Name, @City, @Country);

        SELECT @OfficeId = OfficeId FROM @Inserted;

        SET @Result = 0;
        SET @Message = 'Office created successfully.';
    END TRY
    BEGIN CATCH
        SET @Result = 500;
        SET @Message = CONCAT('Failed to create office: ', ERROR_MESSAGE());
    END CATCH

    -- OfficeId: the new office's server-generated key; only meaningful when Result = 0.
    SELECT @Result AS Result, @Message AS Message, @OfficeId AS OfficeId;
END
