CREATE PROCEDURE [dbo].[usp_deleteCostCenter]
    @var_Guid UNIQUEIDENTIFIER
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

    IF EXISTS (SELECT 1 FROM tbl_employees WHERE FK_cost_center_guid = @var_Guid)
    BEGIN
        SET @result = 409;
        SET @message = 'Cost center is assigned to one or more employees.';

        SELECT @result AS result, @message AS message;
        RETURN;
    END

    BEGIN TRY
        DELETE FROM tbl_cost_centers WHERE PK_cost_center_guid = @var_Guid;

        SET @result = 0;
        SET @message = 'Cost center deleted successfully.';
    END TRY
    BEGIN CATCH
        SET @result = 500;
        SET @message = CONCAT('Failed to delete cost center: ', ERROR_MESSAGE());
    END CATCH

    SELECT @result AS result, @message AS message;
END
