CREATE PROCEDURE [dbo].[usp_deactivateEmployee]
    @var_Guid UNIQUEIDENTIFIER
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
        DECLARE @currDate DATETIME2(3) = SYSUTCDATETIME();

        UPDATE tbl_employees
        SET 
            interaction_Date = @currDate,
            employee_Status = 1903
        WHERE PK_employee_guid = @var_Guid AND employee_Status <> 1903; -- Prevent update if already deactivated

        IF @@ROWCOUNT > 0
        BEGIN
            SET @result = 0;
            SET @message = 'Employee deactivated successfully.';
        END
        ELSE
        BEGIN
            SET @result = 409;
            SET @message = 'Employee already deactivated or update failed.';
        END
    END
    ELSE
    BEGIN
        SET @result = 404;
        SET @message = 'Employee not found.';
    END

    SELECT @result AS result, @message AS message;
END
