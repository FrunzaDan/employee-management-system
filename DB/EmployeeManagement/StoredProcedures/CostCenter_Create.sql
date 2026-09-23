CREATE PROCEDURE [dbo].[CostCenter_Create]
    @CostCenterId UNIQUEIDENTIFIER,
    @Code NVARCHAR(50),
    @Name NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Result INT;
    DECLARE @Message NVARCHAR(255);

    IF EXISTS (SELECT 1 FROM dbo.CostCenter WHERE Code = @Code)
    BEGIN
        SET @Result = 400;
        SET @Message = 'Cost center code already exists.';

        SELECT @Result AS Result, @Message AS Message;
        RETURN;
    END

    BEGIN TRY
        INSERT INTO dbo.CostCenter (CostCenterId, Code, Name)
        VALUES (@CostCenterId, @Code, @Name);

        SET @Result = 0;
        SET @Message = CONCAT('Cost center created successfully. GUID: ', @CostCenterId);
    END TRY
    BEGIN CATCH
        SET @Result = 500;
        SET @Message = CONCAT('Failed to create cost center: ', ERROR_MESSAGE());
    END CATCH

    SELECT @Result AS Result, @Message AS Message;
END
