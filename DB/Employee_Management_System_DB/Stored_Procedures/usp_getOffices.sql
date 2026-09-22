-- Unpaginated: reference data at company-org scale, not a growing operational
-- table like tbl_employees (see usp_getEmployees for the paginated convention).
CREATE PROCEDURE [dbo].[usp_getOffices]
AS
BEGIN
    SET NOCOUNT ON;

    SELECT PK_office_guid, office_name, city, country
    FROM dbo.tbl_offices
    ORDER BY office_name;
END
