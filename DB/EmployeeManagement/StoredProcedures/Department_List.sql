CREATE PROCEDURE [dbo].[Department_List]
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    SELECT
        d.DepartmentId,
        d.Name,
        COUNT(e.EmployeeId) AS EmployeeCount,
        ISNULL(SUM(s.GrossSalary), 0) AS TotalGrossSalary
    FROM dbo.Department AS d
    LEFT JOIN dbo.Employee AS e ON e.DepartmentId = d.DepartmentId
    OUTER APPLY (
        SELECT TOP 1 GrossSalary
        FROM dbo.EmployeeSalary
        WHERE EmployeeId = e.EmployeeId
        ORDER BY EffectiveDate DESC, CreatedAt DESC
    ) AS s
    GROUP BY d.DepartmentId, d.Name
    ORDER BY d.Name;
END
