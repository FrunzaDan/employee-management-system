CREATE PROCEDURE [dbo].[Employee_Create]
    @FirstName NVARCHAR(100),
    @LastName NVARCHAR(100),
    @Email NVARCHAR(254),
    @PhoneNumber VARCHAR(15),
    @Gender TINYINT = 0,
    @BirthDate DATE = NULL,
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
    SET XACT_ABORT ON;

    DECLARE @Result INT;
    DECLARE @Message NVARCHAR(255);
    DECLARE @EmployeeId UNIQUEIDENTIFIER = NULL;
    DECLARE @Inserted TABLE (EmployeeId UNIQUEIDENTIFIER);

    IF EXISTS (SELECT 1 FROM dbo.Employee WHERE PhoneNumber = @PhoneNumber)
    BEGIN
        SET @Result = 409;
        SET @Message = 'Phone number already exists.';
    END
    ELSE IF EXISTS (SELECT 1 FROM dbo.Employee WHERE Email = @Email)
    BEGIN
        SET @Result = 409;
        SET @Message = 'Email already exists.';
    END
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

            INSERT INTO dbo.Employee
            (
                FirstName, LastName, Email, PhoneNumber, Gender, BirthDate, StatusCode,
                HireDate, OfficeId, DepartmentId, CostCenterId
            )
            OUTPUT inserted.EmployeeId INTO @Inserted
            VALUES
            (
                @FirstName, @LastName, @Email, @PhoneNumber, @Gender, @BirthDate, @StatusCode,
                @HireDate, @OfficeId, @DepartmentId, @CostCenterId
            );

            SELECT @EmployeeId = EmployeeId FROM @Inserted;

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
            SET @Message = 'Employee created successfully.';
        END TRY
        BEGIN CATCH
            IF @@TRANCOUNT > 0
                ROLLBACK TRANSACTION;

            IF ERROR_NUMBER() NOT IN (2601, 2627)
                THROW;

            SET @Result = 409;
            SET @Message = 'Email or phone number already exists.';
        END CATCH
    END

    SELECT @Result AS Result, @Message AS Message, @EmployeeId AS EmployeeId;
END
