CREATE PROCEDURE [dbo].[usp_editCostCenter]
    @var_Guid UNIQUEIDENTIFIER,
    @var_CostCenterCode NVARCHAR(50) = NULL,
    @var_CostCenterName NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @result INT;
    DECLARE @message NVARCHAR(255);

    IF NOT EXISTS (SELECT 1 FROM tbl_cost_centers WHERE PK_cost_center_guid = @var_Guid)
    BEGIN
        SET @result = 404;
        SET @message = 'Cost center not found.';

        SELECT @result AS result, @message AS message;
        RETURN;
    END

    IF @var_CostCenterCode IS NOT NULL AND EXISTS (
        SELECT 1 FROM tbl_cost_centers WHERE cost_center_code = @var_CostCenterCode AND PK_cost_center_guid <> @var_Guid
    )
    BEGIN
        SET @result = 400;
        SET @message = 'Cost center code already exists.';

        SELECT @result AS result, @message AS message;
        RETURN;
    END

    BEGIN TRY
        UPDATE tbl_cost_centers
        SET
            cost_center_code = ISNULL(@var_CostCenterCode, cost_center_code),
            cost_center_name = ISNULL(@var_CostCenterName, cost_center_name)
        WHERE PK_cost_center_guid = @var_Guid;

        SET @result = 0;
        SET @message = 'Cost center updated successfully.';
    END TRY
    BEGIN CATCH
        SET @result = 500;
        SET @message = CONCAT('Failed to update cost center: ', ERROR_MESSAGE());
    END CATCH

    SELECT @result AS result, @message AS message;
END
