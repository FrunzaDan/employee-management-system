CREATE PROCEDURE [dbo].[usp_getEmployee]
    -- Exactly one is supplied (EmployeeGetting detects which from the search term's shape).
    -- Each is typed like the column it's compared to, so every comparison is a straight
    -- index seek with no implicit conversion — and an email longer than 50 characters is
    -- no longer silently truncated before the lookup, as the old NVARCHAR(50) catch-all was.
    @var_Guid UNIQUEIDENTIFIER = NULL,
    @var_MSISDN VARCHAR(15) = NULL,
    @var_Email NVARCHAR(254) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    -- Split by search type (instead of one query with an OR across all three) so the
    -- optimizer can seek the specific unique index for whichever branch actually runs,
    -- rather than compiling one plan that has to cover all three possible predicates.
    IF @var_Guid IS NOT NULL
    BEGIN
        SELECT
            c.PK_employee_guid,
            c.first_name,
            c.last_name,
            c.email,
            c.msisdn,
            c.gender,
            c.birthdate,
            c.employee_Status,
            c.creation_Date,
            c.interaction_Date,
            a.country,
            a.county,
            a.town,
            a.zip_code,
            a.street,
            a.number,
            c.hire_Date,
            o.PK_office_guid,
            o.office_name,
            d.PK_department_guid,
            d.department_name,
            cc.PK_cost_center_guid,
            cc.cost_center_name,
            s.brutto_salary AS current_brutto_salary
        FROM
            tbl_employees AS c
        INNER JOIN
            tbl_addresses AS a
            ON c.PK_employee_guid = a.FK_employee_guid
        LEFT JOIN tbl_offices AS o ON c.FK_office_guid = o.PK_office_guid
        LEFT JOIN tbl_departments AS d ON c.FK_department_guid = d.PK_department_guid
        LEFT JOIN tbl_cost_centers AS cc ON c.FK_cost_center_guid = cc.PK_cost_center_guid
        OUTER APPLY (
            SELECT TOP 1 brutto_salary
            FROM tbl_employee_salary_history
            WHERE FK_employee_guid = c.PK_employee_guid
            ORDER BY effective_Date DESC, created_Date DESC
        ) AS s
        WHERE
            c.PK_employee_guid = @var_Guid;
    END
    ELSE IF @var_MSISDN IS NOT NULL
    BEGIN
        SELECT
            c.PK_employee_guid,
            c.first_name,
            c.last_name,
            c.email,
            c.msisdn,
            c.gender,
            c.birthdate,
            c.employee_Status,
            c.creation_Date,
            c.interaction_Date,
            a.country,
            a.county,
            a.town,
            a.zip_code,
            a.street,
            a.number,
            c.hire_Date,
            o.PK_office_guid,
            o.office_name,
            d.PK_department_guid,
            d.department_name,
            cc.PK_cost_center_guid,
            cc.cost_center_name,
            s.brutto_salary AS current_brutto_salary
        FROM
            tbl_employees AS c
        INNER JOIN
            tbl_addresses AS a
            ON c.PK_employee_guid = a.FK_employee_guid
        LEFT JOIN tbl_offices AS o ON c.FK_office_guid = o.PK_office_guid
        LEFT JOIN tbl_departments AS d ON c.FK_department_guid = d.PK_department_guid
        LEFT JOIN tbl_cost_centers AS cc ON c.FK_cost_center_guid = cc.PK_cost_center_guid
        OUTER APPLY (
            SELECT TOP 1 brutto_salary
            FROM tbl_employee_salary_history
            WHERE FK_employee_guid = c.PK_employee_guid
            ORDER BY effective_Date DESC, created_Date DESC
        ) AS s
        WHERE
            c.msisdn = @var_MSISDN;
    END
    ELSE IF @var_Email IS NOT NULL
    BEGIN
        SELECT
            c.PK_employee_guid,
            c.first_name,
            c.last_name,
            c.email,
            c.msisdn,
            c.gender,
            c.birthdate,
            c.employee_Status,
            c.creation_Date,
            c.interaction_Date,
            a.country,
            a.county,
            a.town,
            a.zip_code,
            a.street,
            a.number,
            c.hire_Date,
            o.PK_office_guid,
            o.office_name,
            d.PK_department_guid,
            d.department_name,
            cc.PK_cost_center_guid,
            cc.cost_center_name,
            s.brutto_salary AS current_brutto_salary
        FROM
            tbl_employees AS c
        INNER JOIN
            tbl_addresses AS a
            ON c.PK_employee_guid = a.FK_employee_guid
        LEFT JOIN tbl_offices AS o ON c.FK_office_guid = o.PK_office_guid
        LEFT JOIN tbl_departments AS d ON c.FK_department_guid = d.PK_department_guid
        LEFT JOIN tbl_cost_centers AS cc ON c.FK_cost_center_guid = cc.PK_cost_center_guid
        OUTER APPLY (
            SELECT TOP 1 brutto_salary
            FROM tbl_employee_salary_history
            WHERE FK_employee_guid = c.PK_employee_guid
            ORDER BY effective_Date DESC, created_Date DESC
        ) AS s
        WHERE
            c.email = @var_Email;
    END
END
