CREATE PROCEDURE [dbo].[EmployeeAuditLog_List]
    @PageNumber INT = 1,
    @PageSize INT = 10
AS
BEGIN
    SET NOCOUNT ON;

    -- LEFT JOIN, not INNER: EmployeeAuditLog has no FK to Employee
    -- (a deleted employee's history must survive the delete — see
    -- EmployeeAuditLog.sql), so FirstName/LastName come back NULL for
    -- a employee that no longer exists rather than dropping that row.
    SELECT
        l.EmployeeAuditLogId,
        l.EmployeeId,
        e.FirstName,
        e.LastName,
        l.PerformedBy,
        l.ActionType,
        l.Details,
        l.OccurredAt,
        COUNT(*) OVER() AS TotalCount
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
