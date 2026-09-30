CREATE PROCEDURE [dbo].[CostCenter_Create]
    @Code NVARCHAR(50),
    @Name NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @Result INT;
    DECLARE @Message NVARCHAR(255);
    DECLARE @Field VARCHAR(50) = NULL;
    DECLARE @CostCenterId UNIQUEIDENTIFIER = NULL;
    DECLARE @Inserted TABLE (CostCenterId UNIQUEIDENTIFIER);

    IF EXISTS (SELECT 1 FROM dbo.CostCenter WHERE Code = @Code)
    BEGIN
        SET @Result = 409;
        SET @Message = 'Cost center code already exists.';
        SET @Field = 'Code';

        SELECT @Result AS Result, @Message AS Message, @CostCenterId AS CostCenterId, @Field AS Field;
        RETURN;
    END

    BEGIN TRY
        INSERT INTO dbo.CostCenter (Code, Name)
        OUTPUT inserted.CostCenterId INTO @Inserted
        VALUES (@Code, @Name);

        SELECT @CostCenterId = CostCenterId FROM @Inserted;

        SET @Result = 0;
        SET @Message = 'Cost center created successfully.';
    END TRY
    BEGIN CATCH
        IF ERROR_NUMBER() NOT IN (2601, 2627)
            THROW;

        SET @Result = 409;
        SET @Message = 'Cost center code already exists.';
        SET @Field = 'Code';
    END CATCH

    SELECT @Result AS Result, @Message AS Message, @CostCenterId AS CostCenterId, @Field AS Field;
END
