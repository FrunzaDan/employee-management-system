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
        -- 1903 (deactivated): the normal deactivate-then-delete lifecycle.
        -- 1904 (test): fictitious demo data, exempt from that guardrail so it
        -- can be deleted directly.
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
        -- Both deletes must succeed together: Employee_Get/Employee_List
        -- INNER JOIN to EmployeeAddress, so a Employee row left behind without
        -- its EmployeeAddress row (e.g. the second DELETE fails after the first
        -- already committed) would silently disappear from every read despite
        -- still existing — mirrors Employee_Create's TRY/CATCH for the same
        -- two-table-consistency reason.
        BEGIN TRY
            BEGIN TRANSACTION;

            DELETE FROM dbo.EmployeeAddress
            WHERE EmployeeId = @EmployeeId;

            -- No FK cascade on EmployeeSalary (deliberate, see the
            -- table's definition) — deleted explicitly here, same reasoning as
            -- EmployeeAddress above.
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
