CREATE PROCEDURE [dbo].[EmployeeAuditLog_List]
    @PageNumber INT = 1,
    @PageSize INT = 20
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    SELECT COUNT(*) AS TotalCount
    FROM dbo.EmployeeAuditLog;

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
