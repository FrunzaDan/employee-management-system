CREATE PROCEDURE [dbo].[Department_List]
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    -- EmployeeCount/TotalGrossSalary are computed over every employee assigned to the
    -- department regardless of status, same as Employee_ListByDepartment's unfiltered
    -- list. Each employee's current salary is its most recent EmployeeSalary
    -- row, same OUTER APPLY pattern Employee_List uses.
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
