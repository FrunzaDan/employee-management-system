CREATE PROCEDURE [dbo].[usp_editEmployee]
    @var_Guid UNIQUEIDENTIFIER,
    @var_FirstName NVARCHAR(50) = NULL,
    @var_LastName NVARCHAR(50) = NULL,
    @var_Email NVARCHAR(254) = NULL,
    @var_MSISDN VARCHAR(15) = NULL,
    @var_Gender TINYINT = NULL,
    @var_Birthdate DATE = NULL,
    @var_Country NVARCHAR(100) = NULL,
    @var_County NVARCHAR(100) = NULL,
    @var_Town NVARCHAR(100) = NULL,
    @var_ZIP VARCHAR(20) = NULL,
    @var_Street NVARCHAR(100) = NULL,
    @var_Number NVARCHAR(50) = NULL,
    @var_HireDate DATE = NULL,
    @var_OfficeGuid UNIQUEIDENTIFIER = NULL,
    @var_DepartmentGuid UNIQUEIDENTIFIER = NULL,
    @var_CostCenterGuid UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @result INT;
    DECLARE @message NVARCHAR(255);
    DECLARE @currDate DATETIME2(3) = SYSUTCDATETIME();

    IF NOT EXISTS (SELECT 1 FROM tbl_employees WHERE PK_employee_guid = @var_Guid)
    BEGIN
        SET @result = 404;
        SET @message = 'Employee not found.';

        SELECT @result AS result, @message AS message;
        RETURN;
    END

    -- Same duplicate pre-check usp_createEmployee does, excluding the row being edited
    -- itself — without this, an edit that collides with another employee's email/msisdn
    -- would throw a raw, unhandled UQ_ constraint violation instead of a clean 400.
    IF @var_Email IS NOT NULL AND EXISTS (
        SELECT 1 FROM tbl_employees WHERE email = @var_Email AND PK_employee_guid <> @var_Guid
    )
    BEGIN
        SET @result = 400;
        SET @message = 'Email already exists.';

        SELECT @result AS result, @message AS message;
        RETURN;
    END

    IF @var_MSISDN IS NOT NULL AND EXISTS (
        SELECT 1 FROM tbl_employees WHERE msisdn = @var_MSISDN AND PK_employee_guid <> @var_Guid
    )
    BEGIN
        SET @result = 400;
        SET @message = 'MSISDN already exists.';

        SELECT @result AS result, @message AS message;
        RETURN;
    END

    -- Same friendly-400-before-FK reasoning as usp_createEmployee.
    IF @var_OfficeGuid IS NOT NULL AND NOT EXISTS (SELECT 1 FROM tbl_offices WHERE PK_office_guid = @var_OfficeGuid)
    BEGIN
        SET @result = 400;
        SET @message = 'Office not found.';

        SELECT @result AS result, @message AS message;
        RETURN;
    END

    IF @var_DepartmentGuid IS NOT NULL AND NOT EXISTS (SELECT 1 FROM tbl_departments WHERE PK_department_guid = @var_DepartmentGuid)
    BEGIN
        SET @result = 400;
        SET @message = 'Department not found.';

        SELECT @result AS result, @message AS message;
        RETURN;
    END

    IF @var_CostCenterGuid IS NOT NULL AND NOT EXISTS (SELECT 1 FROM tbl_cost_centers WHERE PK_cost_center_guid = @var_CostCenterGuid)
    BEGIN
        SET @result = 400;
        SET @message = 'Cost center not found.';

        SELECT @result AS result, @message AS message;
        RETURN;
    END

    -- Both updates must stay in sync, same reasoning as usp_createEmployee/usp_deleteEmployee's
    -- TRY/CATCH + transaction: usp_getEmployee/usp_getEmployees INNER JOIN the two tables, so a
    -- tbl_employees update that commits while the paired tbl_addresses update then fails would
    -- leave the two tables inconsistent.
    BEGIN TRY
        BEGIN TRANSACTION;

        UPDATE tbl_employees
        SET
            interaction_Date = @currDate,
            first_name = ISNULL(@var_FirstName, first_name),
            last_name = ISNULL(@var_LastName, last_name),
            email = ISNULL(@var_Email, email),
            msisdn = ISNULL(@var_MSISDN, msisdn),
            gender = ISNULL(@var_Gender, gender),
            birthdate = ISNULL(@var_Birthdate, birthdate),
            hire_Date = ISNULL(@var_HireDate, hire_Date),
            FK_office_guid = ISNULL(@var_OfficeGuid, FK_office_guid),
            FK_department_guid = ISNULL(@var_DepartmentGuid, FK_department_guid),
            FK_cost_center_guid = ISNULL(@var_CostCenterGuid, FK_cost_center_guid)
        WHERE PK_employee_guid = @var_Guid;

        -- Only touch tbl_addresses when the request actually supplied an address
        -- field; otherwise every ISNULL(@param, column) would resolve to the
        -- existing value and this would be a no-op write on every edit call.
        IF @var_Country IS NOT NULL OR @var_County IS NOT NULL OR @var_Town IS NOT NULL
            OR @var_ZIP IS NOT NULL OR @var_Street IS NOT NULL OR @var_Number IS NOT NULL
        BEGIN
            UPDATE tbl_addresses
            SET
                country = ISNULL(@var_Country, country),
                county = ISNULL(@var_County, county),
                town = ISNULL(@var_Town, town),
                zip_code = ISNULL(@var_ZIP, zip_code),
                street = ISNULL(@var_Street, street),
                number = ISNULL(@var_Number, number)
            WHERE FK_employee_guid = @var_Guid;
        END

        COMMIT TRANSACTION;

        SET @result = 0;
        SET @message = 'Employee details updated successfully.';
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        SET @result = 500;
        SET @message = CONCAT('Failed to update employee: ', ERROR_MESSAGE());
    END CATCH

    SELECT @result AS result, @message AS message;
END
