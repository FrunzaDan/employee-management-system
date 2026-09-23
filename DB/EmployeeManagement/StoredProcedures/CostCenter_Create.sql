CREATE PROCEDURE [dbo].[CostCenter_Create]
    @Code NVARCHAR(50),
    @Name NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Result INT;
    DECLARE @Message NVARCHAR(255);
    DECLARE @CostCenterId UNIQUEIDENTIFIER = NULL;
    DECLARE @Inserted TABLE (CostCenterId UNIQUEIDENTIFIER);

    IF EXISTS (SELECT 1 FROM dbo.CostCenter WHERE Code = @Code)
    BEGIN
        SET @Result = 400;
        SET @Message = 'Cost center code already exists.';

        -- Same row shape as the final SELECT, so the API reads every outcome the same way.
        SELECT @Result AS Result, @Message AS Message, @CostCenterId AS CostCenterId;
        RETURN;
    END

    BEGIN TRY
        -- CostCenterId comes from the table's NEWSEQUENTIALID() default.
        INSERT INTO dbo.CostCenter (Code, Name)
        OUTPUT inserted.CostCenterId INTO @Inserted
        VALUES (@Code, @Name);

        SELECT @CostCenterId = CostCenterId FROM @Inserted;

        SET @Result = 0;
        SET @Message = 'Cost center created successfully.';
    END TRY
    BEGIN CATCH
        SET @Result = 500;
        SET @Message = CONCAT('Failed to create cost center: ', ERROR_MESSAGE());
    END CATCH

    -- CostCenterId: the new cost center's server-generated key; only meaningful when Result = 0.
    SELECT @Result AS Result, @Message AS Message, @CostCenterId AS CostCenterId;
END
