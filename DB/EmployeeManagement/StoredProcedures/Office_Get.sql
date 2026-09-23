CREATE PROCEDURE [dbo].[Office_Get]
    @OfficeId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT OfficeId, Name, City, Country
    FROM dbo.Office
    WHERE OfficeId = @OfficeId;
END
