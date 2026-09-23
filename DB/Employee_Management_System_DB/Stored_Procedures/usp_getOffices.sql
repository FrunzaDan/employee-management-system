-- Unpaginated: reference data at company-org scale, not a growing operational
-- table like tbl_employees (see usp_getEmployees for the paginated convention).
CREATE PROCEDURE [dbo].[usp_getOffices]
AS
BEGIN
    SET NOCOUNT ON;

    -- employee_count/total_brutto_salary are computed over every employee assigned to the
    -- office regardless of status, same as usp_getEmployeesByOffice's unfiltered list. Each
    -- employee's current salary is its most recent tbl_employee_salary_history row, same
    -- OUTER APPLY pattern usp_getEmployees uses.
    SELECT
        o.PK_office_guid,
        o.office_name,
        o.city,
        o.country,
        COUNT(e.PK_employee_guid) AS employee_count,
        ISNULL(SUM(s.brutto_salary), 0) AS total_brutto_salary
    FROM dbo.tbl_offices AS o
    LEFT JOIN dbo.tbl_employees AS e ON e.FK_office_guid = o.PK_office_guid
    OUTER APPLY (
        SELECT TOP 1 brutto_salary
        FROM dbo.tbl_employee_salary_history
        WHERE FK_employee_guid = e.PK_employee_guid
        ORDER BY effective_Date DESC, created_Date DESC
    ) AS s
    GROUP BY o.PK_office_guid, o.office_name, o.city, o.country
    ORDER BY o.office_name;
END
