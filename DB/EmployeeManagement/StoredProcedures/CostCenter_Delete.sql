CREATE PROCEDURE [dbo].[CostCenter_Delete]
    @CostCenterId UNIQUEIDENTIFIER
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

    IF EXISTS (SELECT 1 FROM dbo.Employee WHERE CostCenterId = @CostCenterId)
    BEGIN
        SET @Result = 409;
        SET @Message = 'Cost center is assigned to one or more employees.';

        SELECT @Result AS Result, @Message AS Message;
        RETURN;
    END

    BEGIN TRY
        DELETE FROM dbo.CostCenter WHERE CostCenterId = @CostCenterId;

        SET @Result = 0;
        SET @Message = 'Cost center deleted successfully.';
    END TRY
    BEGIN CATCH
        SET @Result = 500;
        SET @Message = CONCAT('Failed to delete cost center: ', ERROR_MESSAGE());
    END CATCH

    SELECT @Result AS Result, @Message AS Message;
END
