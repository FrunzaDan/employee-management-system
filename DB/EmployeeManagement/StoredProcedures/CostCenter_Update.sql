CREATE PROCEDURE [dbo].[CostCenter_Update]
    @CostCenterId UNIQUEIDENTIFIER,
    @Code NVARCHAR(50) = NULL,
    @Name NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Result INT;
    DECLARE @Message NVARCHAR(255);

    IF NOT EXISTS (SELECT 1 FROM dbo.CostCenter WHERE CostCenterId = @CostCenterId)
    BEGIN
        SET @Result = 404;
        SET @Message = 'Cost center not found.';

        SELECT @Result AS Result, @Message AS Message;
        RETURN;
    END

    IF @Code IS NOT NULL AND EXISTS (
        SELECT 1 FROM dbo.CostCenter WHERE Code = @Code AND CostCenterId <> @CostCenterId
    )
    BEGIN
        SET @Result = 400;
        SET @Message = 'Cost center code already exists.';

        SELECT @Result AS Result, @Message AS Message;
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
        SET @Result = 500;
        SET @Message = CONCAT('Failed to update cost center: ', ERROR_MESSAGE());
    END CATCH

    SELECT @Result AS Result, @Message AS Message;
END
