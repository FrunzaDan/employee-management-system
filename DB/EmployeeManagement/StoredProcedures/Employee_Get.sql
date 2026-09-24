CREATE PROCEDURE [dbo].[Employee_Get]
    @EmployeeId UNIQUEIDENTIFIER = NULL,
    @PhoneNumber VARCHAR(15) = NULL,
    @Email NVARCHAR(254) = NULL
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    IF @EmployeeId IS NOT NULL
    BEGIN
        SELECT
            e.EmployeeId,
            e.FirstName,
            e.LastName,
            e.Email,
            e.PhoneNumber,
            e.Gender,
            e.BirthDate,
            e.StatusCode,
            e.CreatedAt,
            e.LastInteractionAt,
            a.Country,
            a.County,
            a.City,
            a.PostalCode,
            a.Street,
            a.StreetNumber,
            e.HireDate,
            o.OfficeId,
            o.Name AS OfficeName,
            d.DepartmentId,
            d.Name AS DepartmentName,
            cc.CostCenterId,
            cc.Name AS CostCenterName,
            s.GrossSalary AS CurrentGrossSalary
        FROM
            dbo.Employee AS e
        INNER JOIN
            dbo.EmployeeAddress AS a
            ON e.EmployeeId = a.EmployeeId
        LEFT JOIN dbo.Office AS o ON e.OfficeId = o.OfficeId
        LEFT JOIN dbo.Department AS d ON e.DepartmentId = d.DepartmentId
        LEFT JOIN dbo.CostCenter AS cc ON e.CostCenterId = cc.CostCenterId
        OUTER APPLY (
            SELECT TOP 1 GrossSalary
            FROM dbo.EmployeeSalary
            WHERE EmployeeId = e.EmployeeId AND EffectiveDate <= CAST(SYSUTCDATETIME() AS DATE)
            ORDER BY EffectiveDate DESC, CreatedAt DESC
        ) AS s
        WHERE
            e.EmployeeId = @EmployeeId;
    END
    ELSE IF @PhoneNumber IS NOT NULL
    BEGIN
        SELECT
            e.EmployeeId,
            e.FirstName,
            e.LastName,
            e.Email,
            e.PhoneNumber,
            e.Gender,
            e.BirthDate,
            e.StatusCode,
            e.CreatedAt,
            e.LastInteractionAt,
            a.Country,
            a.County,
            a.City,
            a.PostalCode,
            a.Street,
            a.StreetNumber,
            e.HireDate,
            o.OfficeId,
            o.Name AS OfficeName,
            d.DepartmentId,
            d.Name AS DepartmentName,
            cc.CostCenterId,
            cc.Name AS CostCenterName,
            s.GrossSalary AS CurrentGrossSalary
        FROM
            dbo.Employee AS e
        INNER JOIN
            dbo.EmployeeAddress AS a
            ON e.EmployeeId = a.EmployeeId
        LEFT JOIN dbo.Office AS o ON e.OfficeId = o.OfficeId
        LEFT JOIN dbo.Department AS d ON e.DepartmentId = d.DepartmentId
        LEFT JOIN dbo.CostCenter AS cc ON e.CostCenterId = cc.CostCenterId
        OUTER APPLY (
            SELECT TOP 1 GrossSalary
            FROM dbo.EmployeeSalary
            WHERE EmployeeId = e.EmployeeId AND EffectiveDate <= CAST(SYSUTCDATETIME() AS DATE)
            ORDER BY EffectiveDate DESC, CreatedAt DESC
        ) AS s
        WHERE
            e.PhoneNumber = @PhoneNumber;
    END
    ELSE IF @Email IS NOT NULL
    BEGIN
        SELECT
            e.EmployeeId,
            e.FirstName,
            e.LastName,
            e.Email,
            e.PhoneNumber,
            e.Gender,
            e.BirthDate,
            e.StatusCode,
            e.CreatedAt,
            e.LastInteractionAt,
            a.Country,
            a.County,
            a.City,
            a.PostalCode,
            a.Street,
            a.StreetNumber,
            e.HireDate,
            o.OfficeId,
            o.Name AS OfficeName,
            d.DepartmentId,
            d.Name AS DepartmentName,
            cc.CostCenterId,
            cc.Name AS CostCenterName,
            s.GrossSalary AS CurrentGrossSalary
        FROM
            dbo.Employee AS e
        INNER JOIN
            dbo.EmployeeAddress AS a
            ON e.EmployeeId = a.EmployeeId
        LEFT JOIN dbo.Office AS o ON e.OfficeId = o.OfficeId
        LEFT JOIN dbo.Department AS d ON e.DepartmentId = d.DepartmentId
        LEFT JOIN dbo.CostCenter AS cc ON e.CostCenterId = cc.CostCenterId
        OUTER APPLY (
            SELECT TOP 1 GrossSalary
            FROM dbo.EmployeeSalary
            WHERE EmployeeId = e.EmployeeId AND EffectiveDate <= CAST(SYSUTCDATETIME() AS DATE)
            ORDER BY EffectiveDate DESC, CreatedAt DESC
        ) AS s
        WHERE
            e.Email = @Email;
    END
END
