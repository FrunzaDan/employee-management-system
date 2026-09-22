CREATE PROCEDURE [dbo].[usp_createCostCenter]
    @var_Guid NVARCHAR(50),
    @var_CostCenterCode NVARCHAR(50),
    @var_CostCenterName NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @result INT;
    DECLARE @message NVARCHAR(255);

    IF EXISTS (SELECT 1 FROM tbl_cost_centers WHERE cost_center_code = @var_CostCenterCode)
    BEGIN
        SET @result = 400;
        SET @message = 'Cost center code already exists.';

        SELECT @result AS result, @message AS message;
        RETURN;
    END

    BEGIN TRY
        INSERT INTO dbo.tbl_cost_centers (PK_cost_center_guid, cost_center_code, cost_center_name)
        VALUES (@var_Guid, @var_CostCenterCode, @var_CostCenterName);

        SET @result = 0;
        SET @message = CONCAT('Cost center created successfully. GUID: ', @var_Guid);
    END TRY
    BEGIN CATCH
        SET @result = 500;
        SET @message = CONCAT('Failed to create cost center: ', ERROR_MESSAGE());
    END CATCH

    SELECT @result AS result, @message AS message;
END
