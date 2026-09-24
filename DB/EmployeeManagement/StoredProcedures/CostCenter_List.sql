CREATE PROCEDURE [dbo].[CostCenter_List]
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    SELECT
        cc.CostCenterId,
        cc.Code,
        cc.Name,
        COUNT(e.EmployeeId) AS EmployeeCount,
        ISNULL(SUM(s.GrossSalary), 0) AS TotalGrossSalary
    FROM dbo.CostCenter AS cc
    LEFT JOIN dbo.Employee AS e ON e.CostCenterId = cc.CostCenterId
    OUTER APPLY (
        SELECT TOP 1 GrossSalary
        FROM dbo.EmployeeSalary
        WHERE EmployeeId = e.EmployeeId
        ORDER BY EffectiveDate DESC, CreatedAt DESC
    ) AS s
    GROUP BY cc.CostCenterId, cc.Code, cc.Name
    ORDER BY cc.Code;
END
