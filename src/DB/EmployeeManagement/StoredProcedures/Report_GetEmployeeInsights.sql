CREATE PROCEDURE [dbo].[Report_GetEmployeeInsights]
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @Today DATE = CAST(SYSUTCDATETIME() AS DATE);

    SELECT
        e.EmployeeId,
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
        WHERE EmployeeId = e.EmployeeId AND EffectiveDate <= @Today
        ORDER BY EffectiveDate DESC, CreatedAt DESC
    ) AS s;

    -- Salary history as it took effect: one row per employee and date (the latest entry wins), none in the future.
    SELECT
        h.EmployeeId,
        h.EffectiveDate,
        h.GrossSalary
    FROM (
        SELECT
            EmployeeId,
            EffectiveDate,
            GrossSalary,
            ROW_NUMBER() OVER (PARTITION BY EmployeeId, EffectiveDate ORDER BY CreatedAt DESC) AS RowNumber
        FROM dbo.EmployeeSalary
        WHERE EffectiveDate <= @Today
    ) AS h
    WHERE h.RowNumber = 1
    ORDER BY h.EmployeeId, h.EffectiveDate;
END
