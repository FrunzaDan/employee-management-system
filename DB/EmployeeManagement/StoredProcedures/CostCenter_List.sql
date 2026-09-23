CREATE PROCEDURE [dbo].[CostCenter_List]
AS
BEGIN
    SET NOCOUNT ON;

    -- EmployeeCount/TotalGrossSalary are computed over every employee assigned to the
    -- cost center regardless of status, same as Employee_ListByCostCenter's unfiltered
    -- list. Each employee's current salary is its most recent EmployeeSalary
    -- row, same OUTER APPLY pattern Employee_List uses.
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
