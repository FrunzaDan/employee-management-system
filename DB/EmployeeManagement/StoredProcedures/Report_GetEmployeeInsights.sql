CREATE PROCEDURE [dbo].[Report_GetEmployeeInsights]
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    SELECT
        e.StatusCode,
        e.Gender,
        e.BirthDate,
        e.HireDate,
        d.Name AS DepartmentName,
        o.Name AS OfficeName,
        s.GrossSalary AS CurrentGrossSalary
    FROM
        dbo.Employee AS e
    LEFT JOIN dbo.Office AS o ON e.OfficeId = o.OfficeId
    LEFT JOIN dbo.Department AS d ON e.DepartmentId = d.DepartmentId
    OUTER APPLY (
        SELECT TOP 1 GrossSalary
        FROM dbo.EmployeeSalary
        WHERE EmployeeId = e.EmployeeId AND EffectiveDate <= CAST(SYSUTCDATETIME() AS DATE)
        ORDER BY EffectiveDate DESC, CreatedAt DESC
    ) AS s;
END
