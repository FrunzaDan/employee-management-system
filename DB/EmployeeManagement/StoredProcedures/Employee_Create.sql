CREATE PROCEDURE [dbo].[Employee_Create]
    @EmployeeId UNIQUEIDENTIFIER,
    @FirstName NVARCHAR(50),
    @LastName NVARCHAR(50),
    @Email NVARCHAR(254),
    @PhoneNumber VARCHAR(15),
    @Gender TINYINT,
    @BirthDate DATE,
    @Country NVARCHAR(100),
    @County NVARCHAR(100),
    @City NVARCHAR(100),
    @PostalCode VARCHAR(20),
    @Street NVARCHAR(100),
    @StreetNumber NVARCHAR(50),
    @StatusCode SMALLINT = 1901,
    @HireDate DATE = NULL,
    @OfficeId UNIQUEIDENTIFIER = NULL,
    @DepartmentId UNIQUEIDENTIFIER = NULL,
    @CostCenterId UNIQUEIDENTIFIER = NULL
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Result INT;
    DECLARE @Message NVARCHAR(255);
    DECLARE @Now DATETIME2(3) = SYSUTCDATETIME();

    IF EXISTS (SELECT 1 FROM dbo.Employee WHERE PhoneNumber = @PhoneNumber)
    BEGIN
        SET @Result = 400;  -- MSISDN already exists
        SET @Message = 'MSISDN already exists.';
    END
    ELSE IF EXISTS (SELECT 1 FROM dbo.Employee WHERE Email = @Email)
    BEGIN
        SET @Result = 400;  -- Email already exists
        SET @Message = 'Email already exists.';
    END
    -- Friendly 400s instead of letting Employee's FK_Employee_Office/Department/CostCenter
    -- throw a raw 500 on an unknown guid, same reasoning as the Email/PhoneNumber checks above.
    ELSE IF @OfficeId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM dbo.Office WHERE OfficeId = @OfficeId)
    BEGIN
        SET @Result = 400;
        SET @Message = 'Office not found.';
    END
    ELSE IF @DepartmentId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM dbo.Department WHERE DepartmentId = @DepartmentId)
    BEGIN
        SET @Result = 400;
        SET @Message = 'Department not found.';
    END
    ELSE IF @CostCenterId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM dbo.CostCenter WHERE CostCenterId = @CostCenterId)
    BEGIN
        SET @Result = 400;
        SET @Message = 'Cost center not found.';
    END
    ELSE
    BEGIN
        BEGIN TRY
            BEGIN TRANSACTION;

            -- Both inserts must succeed together: Employee_Get/Employee_List INNER JOIN
            -- to EmployeeAddress, so a employee row left without a matching address row would
            -- silently disappear from every read despite existing in Employee.
            INSERT INTO dbo.Employee
            (
                EmployeeId, FirstName, LastName, Email, PhoneNumber,
                Gender, BirthDate, StatusCode, CreatedAt, LastInteractionAt,
                HireDate, OfficeId, DepartmentId, CostCenterId
            )
            VALUES
            (
                @EmployeeId, @FirstName, @LastName, @Email, @PhoneNumber,
                @Gender, @BirthDate, @StatusCode, @Now, @Now,
                @HireDate, @OfficeId, @DepartmentId, @CostCenterId
            );

            INSERT INTO dbo.EmployeeAddress
            (
                EmployeeId, Country, County, City, PostalCode, Street, StreetNumber
            )
            VALUES
            (
                @EmployeeId, @Country, @County, @City, @PostalCode, @Street, @StreetNumber
            );

            COMMIT TRANSACTION;

            SET @Result = 0;
            SET @Message = CONCAT('Employee created successfully. GUID: ', @EmployeeId);
        END TRY
        BEGIN CATCH
            IF @@TRANCOUNT > 0
                ROLLBACK TRANSACTION;

            SET @Result = 500;
            SET @Message = CONCAT('Failed to create employee: ', ERROR_MESSAGE());
        END CATCH
    END

    SELECT @Result AS Result, @Message AS Message;
END
