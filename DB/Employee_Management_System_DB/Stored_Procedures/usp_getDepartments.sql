CREATE PROCEDURE [dbo].[usp_getDepartments]
AS
BEGIN
    SET NOCOUNT ON;

    -- employee_count/total_brutto_salary are computed over every employee assigned to the
    -- department regardless of status, same as usp_getEmployeesByDepartment's unfiltered
    -- list. Each employee's current salary is its most recent tbl_employee_salary_history
    -- row, same OUTER APPLY pattern usp_getEmployees uses.
    SELECT
        d.PK_department_guid,
        d.department_name,
        COUNT(e.PK_employee_guid) AS employee_count,
        ISNULL(SUM(s.brutto_salary), 0) AS total_brutto_salary
    FROM dbo.tbl_departments AS d
    LEFT JOIN dbo.tbl_employees AS e ON e.FK_department_guid = d.PK_department_guid
    OUTER APPLY (
        SELECT TOP 1 brutto_salary
        FROM dbo.tbl_employee_salary_history
        WHERE FK_employee_guid = e.PK_employee_guid
        ORDER BY effective_Date DESC
    ) AS s
    GROUP BY d.PK_department_guid, d.department_name
    ORDER BY d.department_name;
END
