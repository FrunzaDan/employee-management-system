CREATE PROCEDURE [dbo].[EmployeeAuditLog_List]
    @PageNumber INT = 1,
    @PageSize INT = 20
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    -- Result set 1: the total, counted on its own so it's right even when the
    -- requested page has no rows (past the last page, or the log was just cleared).
    -- A COUNT(*) OVER() column on the page query only carries it when a row comes back.
    SELECT COUNT(*) AS TotalCount
    FROM dbo.EmployeeAuditLog;

    -- Result set 2: the page, newest first.
    -- LEFT JOIN, not INNER: EmployeeAuditLog has no FK to Employee
    -- (a deleted employee's history must survive the delete — see
    -- EmployeeAuditLog.sql), so FirstName/LastName come back NULL for
    -- an employee that no longer exists rather than dropping that row.
    SELECT
        l.EmployeeAuditLogId,
        l.EmployeeId,
        e.FirstName,
        e.LastName,
        l.PerformedBy,
        l.ActionType,
        l.Details,
        l.OccurredAt
    FROM
        dbo.EmployeeAuditLog AS l
    LEFT JOIN
        dbo.Employee AS e
        ON e.EmployeeId = l.EmployeeId
    ORDER BY
        l.OccurredAt DESC, l.EmployeeAuditLogId DESC
    OFFSET (@PageNumber - 1) * @PageSize ROWS
    FETCH NEXT @PageSize ROWS ONLY;
END
