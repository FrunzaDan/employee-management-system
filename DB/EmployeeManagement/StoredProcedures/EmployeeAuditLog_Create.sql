CREATE PROCEDURE [dbo].[EmployeeAuditLog_Create]
    @EmployeeId UNIQUEIDENTIFIER,
    @PerformedBy NVARCHAR(50),
    @ActionType VARCHAR(50),
    @Details NVARCHAR(500) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Result INT;
    DECLARE @Message NVARCHAR(255);

    INSERT INTO dbo.EmployeeAuditLog
    (
        EmployeeId, PerformedBy, ActionType, Details, OccurredAt
    )
    VALUES
    (
        @EmployeeId, @PerformedBy, @ActionType, @Details, SYSUTCDATETIME()
    );

    SET @Result = 0;
    SET @Message = 'Audit log entry created.';

    SELECT @Result AS Result, @Message AS Message;
END
