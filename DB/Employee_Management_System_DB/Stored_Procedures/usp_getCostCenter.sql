CREATE PROCEDURE [dbo].[usp_getCostCenter]
    @var_Guid NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT PK_cost_center_guid, cost_center_code, cost_center_name
    FROM dbo.tbl_cost_centers
    WHERE PK_cost_center_guid = @var_Guid;
END
