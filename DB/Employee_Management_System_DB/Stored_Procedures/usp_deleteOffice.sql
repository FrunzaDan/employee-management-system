CREATE PROCEDURE [dbo].[usp_deleteOffice]
    @var_Guid NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @result INT;
    DECLARE @message NVARCHAR(255);

    IF NOT EXISTS (SELECT 1 FROM tbl_offices WHERE PK_office_guid = @var_Guid)
    BEGIN
        SET @result = 404;
        SET @message = 'Office not found.';

        SELECT @result AS result, @message AS message;
        RETURN;
    END

    -- Friendly pre-check instead of letting tbl_employees' FK throw a raw 500.
    IF EXISTS (SELECT 1 FROM tbl_employees WHERE FK_office_guid = @var_Guid)
    BEGIN
        SET @result = 409;
        SET @message = 'Office is assigned to one or more employees.';

        SELECT @result AS result, @message AS message;
        RETURN;
    END

    BEGIN TRY
        DELETE FROM tbl_offices WHERE PK_office_guid = @var_Guid;

        SET @result = 0;
        SET @message = 'Office deleted successfully.';
    END TRY
    BEGIN CATCH
        SET @result = 500;
        SET @message = CONCAT('Failed to delete office: ', ERROR_MESSAGE());
    END CATCH

    SELECT @result AS result, @message AS message;
END
