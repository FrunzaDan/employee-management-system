CREATE PROCEDURE [dbo].[usp_getAllEmployeeAuditLog]
    @PageNumber INT = 1,
    @PageSize INT = 10
AS
BEGIN
    SET NOCOUNT ON;

    -- LEFT JOIN, not INNER: tbl_employee_audit_log has no FK to tbl_employees
    -- (a deleted employee's history must survive the delete — see
    -- tbl_employee_audit_log.sql), so first_name/last_name come back NULL for
    -- a employee that no longer exists rather than dropping that row.
    SELECT
        l.audit_id,
        l.employee_guid,
        c.first_name,
        c.last_name,
        l.employer_id,
        l.action,
        l.details,
        l.action_Date,
        COUNT(*) OVER() AS total_count
    FROM
        dbo.tbl_employee_audit_log AS l
    LEFT JOIN
        dbo.tbl_employees AS c
        ON c.PK_employee_guid = l.employee_guid
    ORDER BY
        l.action_Date DESC, l.audit_id DESC
    OFFSET (@PageNumber - 1) * @PageSize ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END
