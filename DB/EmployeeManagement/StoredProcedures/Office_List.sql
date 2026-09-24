CREATE PROCEDURE [dbo].[Office_List]
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    SELECT
        o.OfficeId,
        o.Name,
        o.City,
        o.Country,
        COUNT(e.EmployeeId) AS EmployeeCount,
        ISNULL(SUM(s.GrossSalary), 0) AS TotalGrossSalary
    FROM dbo.Office AS o
    LEFT JOIN dbo.Employee AS e ON e.OfficeId = o.OfficeId
    OUTER APPLY (
        SELECT TOP 1 GrossSalary
        FROM dbo.EmployeeSalary
        WHERE EmployeeId = e.EmployeeId
        ORDER BY EffectiveDate DESC, CreatedAt DESC
    ) AS s
    GROUP BY o.OfficeId, o.Name, o.City, o.Country
    ORDER BY o.Name;
END
