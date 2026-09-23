CREATE PROCEDURE [dbo].[usp_createOffice]
    @var_Guid UNIQUEIDENTIFIER,
    @var_OfficeName NVARCHAR(100),
    @var_City NVARCHAR(100) = NULL,
    @var_Country NVARCHAR(100) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @result INT;
    DECLARE @message NVARCHAR(255);

    BEGIN TRY
        INSERT INTO dbo.tbl_offices (PK_office_guid, office_name, city, country)
        VALUES (@var_Guid, @var_OfficeName, @var_City, @var_Country);

        SET @result = 0;
        SET @message = CONCAT('Office created successfully. GUID: ', @var_Guid);
    END TRY
    BEGIN CATCH
        SET @result = 500;
        SET @message = CONCAT('Failed to create office: ', ERROR_MESSAGE());
    END CATCH

    SELECT @result AS result, @message AS message;
END
