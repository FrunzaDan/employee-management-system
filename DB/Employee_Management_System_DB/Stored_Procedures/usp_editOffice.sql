CREATE PROCEDURE [dbo].[usp_editOffice]
    @var_Guid UNIQUEIDENTIFIER,
    @var_OfficeName NVARCHAR(100) = NULL,
    @var_City NVARCHAR(100) = NULL,
    @var_Country NVARCHAR(100) = NULL
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

    BEGIN TRY
        UPDATE tbl_offices
        SET
            office_name = ISNULL(@var_OfficeName, office_name),
            city = ISNULL(@var_City, city),
            country = ISNULL(@var_Country, country)
        WHERE PK_office_guid = @var_Guid;

        SET @result = 0;
        SET @message = 'Office updated successfully.';
    END TRY
    BEGIN CATCH
        SET @result = 500;
        SET @message = CONCAT('Failed to update office: ', ERROR_MESSAGE());
    END CATCH

    SELECT @result AS result, @message AS message;
END
