CREATE PROCEDURE [dbo].[EmployeeAuditLog_DeleteAll]
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @Result INT;
    DECLARE @Message NVARCHAR(255);

    DELETE FROM dbo.EmployeeAuditLog;

    SET @Result = 0;
    SET @Message = 'Audit log cleared successfully.';

    SELECT @Result AS Result, @Message AS Message;
END
