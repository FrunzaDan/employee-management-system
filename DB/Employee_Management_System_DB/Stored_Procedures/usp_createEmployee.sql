CREATE PROCEDURE [dbo].[usp_createEmployee]
    @var_Guid NVARCHAR(50),
    @var_FirstName NVARCHAR(50),
    @var_LastName NVARCHAR(50),
    @var_Email NVARCHAR(50),
    @var_MSISDN NVARCHAR(50),
    @var_Gender INT,
    @var_Birthdate NVARCHAR(50),
    @var_Country NVARCHAR(100),
    @var_County NVARCHAR(100),
    @var_Town NVARCHAR(50),
    @var_ZIP NVARCHAR(50),
    @var_Street NVARCHAR(100),
    @var_Number NVARCHAR(50),
    @var_EmployeeStatus INT = 1901,
    @var_HireDate NVARCHAR(50) = NULL,
    @var_OfficeGuid NVARCHAR(50) = NULL,
    @var_DepartmentGuid NVARCHAR(50) = NULL,
    @var_CostCenterGuid NVARCHAR(50) = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @result INT;
    DECLARE @message NVARCHAR(255);
    DECLARE @currentDateTime DATETIME = GETDATE();

    IF EXISTS (SELECT 1 FROM tbl_employees WHERE msisdn = @var_MSISDN)
    BEGIN
        SET @result = 400;  -- MSISDN already exists
        SET @message = 'MSISDN already exists.';
    END
    ELSE IF EXISTS (SELECT 1 FROM tbl_employees WHERE email = @var_Email)
    BEGIN
        SET @result = 400;  -- Email already exists
        SET @message = 'Email already exists.';
    END
    -- Friendly 400s instead of letting tbl_employees' FK_office/department/cost_center_guid
    -- throw a raw 500 on an unknown guid, same reasoning as the email/msisdn checks above.
    ELSE IF @var_OfficeGuid IS NOT NULL AND NOT EXISTS (SELECT 1 FROM tbl_offices WHERE PK_office_guid = @var_OfficeGuid)
    BEGIN
        SET @result = 400;
        SET @message = 'Office not found.';
    END
    ELSE IF @var_DepartmentGuid IS NOT NULL AND NOT EXISTS (SELECT 1 FROM tbl_departments WHERE PK_department_guid = @var_DepartmentGuid)
    BEGIN
        SET @result = 400;
        SET @message = 'Department not found.';
    END
    ELSE IF @var_CostCenterGuid IS NOT NULL AND NOT EXISTS (SELECT 1 FROM tbl_cost_centers WHERE PK_cost_center_guid = @var_CostCenterGuid)
    BEGIN
        SET @result = 400;
        SET @message = 'Cost center not found.';
    END
    ELSE
    BEGIN
        BEGIN TRY
            BEGIN TRANSACTION;

            -- Both inserts must succeed together: usp_getEmployee/usp_getEmployees INNER JOIN
            -- to tbl_addresses, so a employee row left without a matching address row would
            -- silently disappear from every read despite existing in tbl_employees.
            INSERT INTO dbo.tbl_employees
            (
                PK_employee_guid, first_name, last_name, email, msisdn,
                gender, birthdate, employee_Status, creation_Date, interaction_Date,
                hire_Date, FK_office_guid, FK_department_guid, FK_cost_center_guid
            )
            VALUES
            (
                @var_Guid, @var_FirstName, @var_LastName, @var_Email, @var_MSISDN,
                @var_Gender, @var_Birthdate, @var_EmployeeStatus, @currentDateTime, @currentDateTime,
                @var_HireDate, @var_OfficeGuid, @var_DepartmentGuid, @var_CostCenterGuid
            );

            INSERT INTO dbo.tbl_addresses
            (
                FK_employee_guid, country, county, town, zip_code, street, number
            )
            VALUES
            (
                @var_Guid, @var_Country, @var_County, @var_Town, @var_ZIP, @var_Street, @var_Number
            );

            COMMIT TRANSACTION;

            SET @result = 0;
            SET @message = CONCAT('Employee created successfully. GUID: ', @var_Guid);
        END TRY
        BEGIN CATCH
            IF @@TRANCOUNT > 0
                ROLLBACK TRANSACTION;

            SET @result = 500;
            SET @message = CONCAT('Failed to create employee: ', ERROR_MESSAGE());
        END CATCH
    END

    SELECT @result AS result, @message AS message;
END
