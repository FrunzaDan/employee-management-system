CREATE PROCEDURE [dbo].[Employee_Delete]
    @EmployeeId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @Result INT;
    DECLARE @Message NVARCHAR(255);

    IF NOT EXISTS (
        SELECT 1
        FROM dbo.Employee
        WHERE EmployeeId = @EmployeeId
    )
    BEGIN
        SET @Result = 404;
        SET @Message = 'Employee not found.';
    END
    ELSE IF NOT EXISTS (
        SELECT 1
        FROM dbo.Employee
        WHERE EmployeeId = @EmployeeId AND StatusCode IN (1903, 1904)
    )
    BEGIN
        SET @Result = 409;
        SET @Message = 'Employee must be deactivated before it can be deleted.';
    END
    ELSE
    BEGIN
        BEGIN TRY
            BEGIN TRANSACTION;

            DELETE FROM dbo.EmployeeAddress
            WHERE EmployeeId = @EmployeeId;

            DELETE FROM dbo.EmployeeSalary
            WHERE EmployeeId = @EmployeeId;

            DELETE FROM dbo.Employee
            WHERE EmployeeId = @EmployeeId;

            COMMIT TRANSACTION;

            SET @Result = 0;
            SET @Message = 'Employee deleted successfully.';
        END TRY
        BEGIN CATCH
            IF @@TRANCOUNT > 0
                ROLLBACK TRANSACTION;

            THROW;
        END CATCH
    END

    SELECT @Result AS Result, @Message AS Message;
END
