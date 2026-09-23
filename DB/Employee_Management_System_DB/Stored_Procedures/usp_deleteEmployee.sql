CREATE PROCEDURE [dbo].[usp_deleteEmployee]
    @var_Guid UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @result INT;
    DECLARE @message NVARCHAR(255);

    IF NOT EXISTS (
        SELECT 1
        FROM tbl_employees
        WHERE PK_employee_guid = @var_Guid
    )
    BEGIN
        SET @result = 404;
        SET @message = 'Employee not found.';
    END
    ELSE IF NOT EXISTS (
        -- 1903 (deactivated): the normal deactivate-then-delete lifecycle.
        -- 1904 (test): fictitious demo data, exempt from that guardrail so it
        -- can be deleted directly.
        SELECT 1
        FROM tbl_employees
        WHERE PK_employee_guid = @var_Guid AND employee_Status IN (1903, 1904)
    )
    BEGIN
        SET @result = 409;
        SET @message = 'Employee must be deactivated before it can be deleted.';
    END
    ELSE
    BEGIN
        -- Both deletes must succeed together: usp_getEmployee/usp_getEmployees
        -- INNER JOIN to tbl_addresses, so a tbl_employees row left behind without
        -- its tbl_addresses row (e.g. the second DELETE fails after the first
        -- already committed) would silently disappear from every read despite
        -- still existing — mirrors usp_createEmployee's TRY/CATCH for the same
        -- two-table-consistency reason.
        BEGIN TRY
            BEGIN TRANSACTION;

            DELETE FROM tbl_addresses
            WHERE FK_employee_guid = @var_Guid;

            -- No FK cascade on tbl_employee_salary_history (deliberate, see the
            -- table's definition) — deleted explicitly here, same reasoning as
            -- tbl_addresses above.
            DELETE FROM tbl_employee_salary_history
            WHERE FK_employee_guid = @var_Guid;

            DELETE FROM tbl_employees
            WHERE PK_employee_guid = @var_Guid;

            COMMIT TRANSACTION;

            SET @result = 0;
            SET @message = 'Employee deleted successfully.';
        END TRY
        BEGIN CATCH
            IF @@TRANCOUNT > 0
                ROLLBACK TRANSACTION;

            SET @result = 500;
            SET @message = CONCAT('Failed to delete employee: ', ERROR_MESSAGE());
        END CATCH
    END

    SELECT @result AS result, @message AS message;
END
