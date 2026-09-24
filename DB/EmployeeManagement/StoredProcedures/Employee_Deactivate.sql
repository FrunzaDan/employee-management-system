CREATE PROCEDURE [dbo].[Employee_Deactivate]
    @EmployeeId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @Result INT;
    DECLARE @Message NVARCHAR(255);

    IF EXISTS (
        SELECT 1
        FROM dbo.Employee
        WHERE EmployeeId = @EmployeeId
    )
    BEGIN
        DECLARE @Now DATETIME2(3) = SYSUTCDATETIME();

        UPDATE dbo.Employee
        SET 
            LastInteractionAt = @Now,
            StatusCode = 1903
        WHERE EmployeeId = @EmployeeId AND StatusCode <> 1903;

        IF @@ROWCOUNT > 0
        BEGIN
            SET @Result = 0;
            SET @Message = 'Employee deactivated successfully.';
        END
        ELSE
        BEGIN
            SET @Result = 409;
            SET @Message = 'Employee is already deactivated.';
        END
    END
    ELSE
    BEGIN
        SET @Result = 404;
        SET @Message = 'Employee not found.';
    END

    SELECT @Result AS Result, @Message AS Message;
END
