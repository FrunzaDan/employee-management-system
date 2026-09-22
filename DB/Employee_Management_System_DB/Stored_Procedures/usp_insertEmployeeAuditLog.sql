CREATE PROCEDURE [dbo].[usp_insertEmployeeAuditLog]
    @var_EmployeeGuid NVARCHAR(50),
    @var_EmployerID NVARCHAR(50),
    @var_Action NVARCHAR(50),
    @var_Details NVARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @result INT;
    DECLARE @message NVARCHAR(255);

    INSERT INTO dbo.tbl_employee_audit_log
    (
        employee_guid, employer_id, action, details, action_Date
    )
    VALUES
    (
        @var_EmployeeGuid, @var_EmployerID, @var_Action, @var_Details, GETDATE()
    );

    SET @result = 0;
    SET @message = 'Audit log entry created.';

    SELECT @result AS result, @message AS message;
END
