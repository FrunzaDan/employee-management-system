CREATE PROCEDURE [dbo].[usp_getCostCenters]
AS
BEGIN
    SET NOCOUNT ON;

    -- employee_count/total_brutto_salary are computed over every employee assigned to the
    -- cost center regardless of status, same as usp_getEmployeesByCostCenter's unfiltered
    -- list. Each employee's current salary is its most recent tbl_employee_salary_history
    -- row, same OUTER APPLY pattern usp_getEmployees uses.
    SELECT
        cc.PK_cost_center_guid,
        cc.cost_center_code,
        cc.cost_center_name,
        COUNT(e.PK_employee_guid) AS employee_count,
        ISNULL(SUM(s.brutto_salary), 0) AS total_brutto_salary
    FROM dbo.tbl_cost_centers AS cc
    LEFT JOIN dbo.tbl_employees AS e ON e.FK_cost_center_guid = cc.PK_cost_center_guid
    OUTER APPLY (
        SELECT TOP 1 brutto_salary
        FROM dbo.tbl_employee_salary_history
        WHERE FK_employee_guid = e.PK_employee_guid
        ORDER BY effective_Date DESC
    ) AS s
    GROUP BY cc.PK_cost_center_guid, cc.cost_center_code, cc.cost_center_name
    ORDER BY cc.cost_center_code;
END
