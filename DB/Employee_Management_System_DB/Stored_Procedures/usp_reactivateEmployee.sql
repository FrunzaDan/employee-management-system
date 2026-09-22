CREATE PROCEDURE [dbo].[usp_reactivateEmployee]
    @var_Guid NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @result INT;
    DECLARE @message NVARCHAR(255);

    IF EXISTS (
        SELECT 1
        FROM tbl_employees
        WHERE PK_employee_guid = @var_Guid
    )
    BEGIN
        DECLARE @currDate DATETIME = GETDATE();

        UPDATE tbl_employees
        SET 
            interaction_Date = @currDate,
            employee_Status = 1901
        WHERE PK_employee_guid = @var_Guid AND employee_Status <> 1901; -- Prevent update if already reactivated

        IF @@ROWCOUNT > 0
        BEGIN
            SET @result = 0;
            SET @message = 'Employee reactivated successfully.';
        END
        ELSE
        BEGIN
            SET @result = 409;
            SET @message = 'Employee already reactivated or update failed.';
        END
    END
    ELSE
    BEGIN
        SET @result = 404;
        SET @message = 'Employee not found.';
    END

    SELECT @result AS result, @message AS message;
END
