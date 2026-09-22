CREATE PROCEDURE [dbo].[usp_getEmployeeAuditLog]
    @var_EmployeeGuid NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        audit_id,
        employee_guid,
        employer_id,
        action,
        details,
        action_Date
    FROM dbo.tbl_employee_audit_log
    WHERE employee_guid = @var_EmployeeGuid
    ORDER BY action_Date DESC, audit_id DESC;
END
