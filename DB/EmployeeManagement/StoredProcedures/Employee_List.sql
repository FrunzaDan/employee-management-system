CREATE PROCEDURE [dbo].[Employee_List]
    @PageNumber INT = 1,
    @PageSize INT = 10,
    @SearchTerm NVARCHAR(254) = NULL,
    @SortColumn VARCHAR(20) = 'name',
    @SortDirection VARCHAR(4) = 'asc'
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    -- % and _ are LIKE wildcards; a literal search for either would otherwise match far
    -- more than the user typed (e.g. a search for "_" matching almost every employee).
    -- Still fully parameterized (no string concatenation of SQL) — this only escapes the
    -- pattern characters inside the parameter's own value.
    DECLARE @EscapedSearchTerm NVARCHAR(508) =
        REPLACE(REPLACE(REPLACE(@SearchTerm, '\', '\\'), '%', '\%'), '_', '\_');

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
        s.GrossSalary AS CurrentGrossSalary,
        COUNT(*) OVER() AS TotalCount
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
        WHERE EmployeeId = e.EmployeeId
        ORDER BY EffectiveDate DESC, CreatedAt DESC
    ) AS s
    WHERE
        @SearchTerm IS NULL
        OR e.FirstName LIKE '%' + @EscapedSearchTerm + '%' ESCAPE '\'
        OR e.LastName LIKE '%' + @EscapedSearchTerm + '%' ESCAPE '\'
        OR e.Email LIKE '%' + @EscapedSearchTerm + '%' ESCAPE '\'
        OR e.PhoneNumber LIKE '%' + @EscapedSearchTerm + '%' ESCAPE '\'
    ORDER BY
        -- Parameterized sorting without dynamic SQL: for a given
        -- @SortColumn/@SortDirection, exactly one pair of CASE expressions
        -- below evaluates to non-NULL for every row, so it's the only pair
        -- that actually influences row order — the rest are NULL for every
        -- row and are no-ops. Ties within name always break by FirstName.
        -- The sort keys ('name', 'email', 'phonenumber') are the API's EmployeeSortColumn values,
        -- not column names.
        CASE WHEN @SortColumn = 'name' AND @SortDirection = 'asc' THEN e.LastName END ASC,
        CASE WHEN @SortColumn = 'name' AND @SortDirection = 'asc' THEN e.FirstName END ASC,
        CASE WHEN @SortColumn = 'name' AND @SortDirection = 'desc' THEN e.LastName END DESC,
        CASE WHEN @SortColumn = 'name' AND @SortDirection = 'desc' THEN e.FirstName END DESC,
        CASE WHEN @SortColumn = 'email' AND @SortDirection = 'asc' THEN e.Email END ASC,
        CASE WHEN @SortColumn = 'email' AND @SortDirection = 'desc' THEN e.Email END DESC,
        CASE WHEN @SortColumn = 'phonenumber' AND @SortDirection = 'asc' THEN e.PhoneNumber END ASC,
        CASE WHEN @SortColumn = 'phonenumber' AND @SortDirection = 'desc' THEN e.PhoneNumber END DESC
    OFFSET (@PageNumber - 1) * @PageSize ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END
