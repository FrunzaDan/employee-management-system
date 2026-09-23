-- Unpaginated: reference data at company-org scale, not a growing operational
-- table like Employee (see Employee_List for the paginated convention).
CREATE PROCEDURE [dbo].[Office_List]
AS
BEGIN
    SET NOCOUNT ON;

    -- EmployeeCount/TotalGrossSalary are computed over every employee assigned to the
    -- office regardless of status, same as Employee_ListByOffice's unfiltered list. Each
    -- employee's current salary is its most recent EmployeeSalary row, same
    -- OUTER APPLY pattern Employee_List uses.
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
