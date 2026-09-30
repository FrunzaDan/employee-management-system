CREATE PROCEDURE [dbo].[CostCenter_Update]
    @CostCenterId UNIQUEIDENTIFIER,
    @Code NVARCHAR(50) = NULL,
    @Name NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @Result INT;
    DECLARE @Message NVARCHAR(255);
    DECLARE @Field VARCHAR(50) = NULL;

    IF NOT EXISTS (SELECT 1 FROM dbo.CostCenter WHERE CostCenterId = @CostCenterId)
    BEGIN
        SET @Result = 404;
        SET @Message = 'Cost center not found.';

        SELECT @Result AS Result, @Message AS Message, @Field AS Field;
        RETURN;
    END

    IF @Code IS NOT NULL AND EXISTS (
        SELECT 1 FROM dbo.CostCenter WHERE Code = @Code AND CostCenterId <> @CostCenterId
    )
    BEGIN
        SET @Result = 409;
        SET @Message = 'Cost center code already exists.';
        SET @Field = 'Code';

        SELECT @Result AS Result, @Message AS Message, @Field AS Field;
        RETURN;
    END

    BEGIN TRY
        UPDATE dbo.CostCenter
        SET
            Code = ISNULL(@Code, Code),
            Name = ISNULL(@Name, Name)
        WHERE CostCenterId = @CostCenterId;

        SET @Result = 0;
        SET @Message = 'Cost center updated successfully.';
    END TRY
    BEGIN CATCH
        IF ERROR_NUMBER() NOT IN (2601, 2627)
            THROW;

        SET @Result = 409;
        SET @Message = 'Cost center code already exists.';
        SET @Field = 'Code';
    END CATCH

    SELECT @Result AS Result, @Message AS Message, @Field AS Field;
END
