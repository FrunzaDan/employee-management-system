CREATE PROCEDURE [dbo].[EmployeeAuditLog_ListByEmployee]
    @EmployeeId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        EmployeeAuditLogId,
        EmployeeId,
        PerformedBy,
        ActionType,
        Details,
        OccurredAt
    FROM dbo.EmployeeAuditLog
    WHERE EmployeeId = @EmployeeId
    ORDER BY OccurredAt DESC, EmployeeAuditLogId DESC;
END
