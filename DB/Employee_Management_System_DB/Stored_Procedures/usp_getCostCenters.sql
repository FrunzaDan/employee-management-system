CREATE PROCEDURE [dbo].[usp_getCostCenters]
AS
BEGIN
    SET NOCOUNT ON;

    SELECT PK_cost_center_guid, cost_center_code, cost_center_name
    FROM dbo.tbl_cost_centers
    ORDER BY cost_center_code;
END
