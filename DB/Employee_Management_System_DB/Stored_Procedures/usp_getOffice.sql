CREATE PROCEDURE [dbo].[usp_getOffice]
    @var_Guid UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT PK_office_guid, office_name, city, country
    FROM dbo.tbl_offices
    WHERE PK_office_guid = @var_Guid;
END
