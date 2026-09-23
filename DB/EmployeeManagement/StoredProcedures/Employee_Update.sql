CREATE PROCEDURE [dbo].[Employee_Update]
    @EmployeeId UNIQUEIDENTIFIER,
    @FirstName NVARCHAR(100) = NULL,
    @LastName NVARCHAR(100) = NULL,
    @Email NVARCHAR(254) = NULL,
    @PhoneNumber VARCHAR(15) = NULL,
    @Gender TINYINT = NULL,
    @BirthDate DATE = NULL,
    @Country NVARCHAR(100) = NULL,
    @County NVARCHAR(100) = NULL,
    @City NVARCHAR(100) = NULL,
    @PostalCode VARCHAR(20) = NULL,
    @Street NVARCHAR(100) = NULL,
    @StreetNumber NVARCHAR(50) = NULL,
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

    IF NOT EXISTS (SELECT 1 FROM dbo.Employee WHERE EmployeeId = @EmployeeId)
    BEGIN
        SET @Result = 404;
        SET @Message = 'Employee not found.';

        SELECT @Result AS Result, @Message AS Message;
        RETURN;
    END

    -- Same duplicate pre-check Employee_Create does, excluding the row being edited
    -- itself — without this, an edit that collides with another employee's Email/PhoneNumber
    -- would throw a raw, unhandled UQ_ constraint violation instead of a clean 400.
    IF @Email IS NOT NULL AND EXISTS (
        SELECT 1 FROM dbo.Employee WHERE Email = @Email AND EmployeeId <> @EmployeeId
    )
    BEGIN
        SET @Result = 400;
        SET @Message = 'Email already exists.';

        SELECT @Result AS Result, @Message AS Message;
        RETURN;
    END

    IF @PhoneNumber IS NOT NULL AND EXISTS (
        SELECT 1 FROM dbo.Employee WHERE PhoneNumber = @PhoneNumber AND EmployeeId <> @EmployeeId
    )
    BEGIN
        SET @Result = 400;
        SET @Message = 'Phone number already exists.';

        SELECT @Result AS Result, @Message AS Message;
        RETURN;
    END

    -- Same friendly-400-before-FK reasoning as Employee_Create.
    IF @OfficeId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM dbo.Office WHERE OfficeId = @OfficeId)
    BEGIN
        SET @Result = 400;
        SET @Message = 'Office not found.';

        SELECT @Result AS Result, @Message AS Message;
        RETURN;
    END

    IF @DepartmentId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM dbo.Department WHERE DepartmentId = @DepartmentId)
    BEGIN
        SET @Result = 400;
        SET @Message = 'Department not found.';

        SELECT @Result AS Result, @Message AS Message;
        RETURN;
    END

    IF @CostCenterId IS NOT NULL AND NOT EXISTS (SELECT 1 FROM dbo.CostCenter WHERE CostCenterId = @CostCenterId)
    BEGIN
        SET @Result = 400;
        SET @Message = 'Cost center not found.';

        SELECT @Result AS Result, @Message AS Message;
        RETURN;
    END

    -- Both updates must stay in sync, same reasoning as Employee_Create/Employee_Delete's
    -- TRY/CATCH + transaction: Employee_Get/Employee_List INNER JOIN the two tables, so a
    -- Employee update that commits while the paired EmployeeAddress update then fails would
    -- leave the two tables inconsistent.
    BEGIN TRY
        BEGIN TRANSACTION;

        UPDATE dbo.Employee
        SET
            LastInteractionAt = @Now,
            FirstName = ISNULL(@FirstName, FirstName),
            LastName = ISNULL(@LastName, LastName),
            Email = ISNULL(@Email, Email),
            PhoneNumber = ISNULL(@PhoneNumber, PhoneNumber),
            Gender = ISNULL(@Gender, Gender),
            BirthDate = ISNULL(@BirthDate, BirthDate),
            HireDate = ISNULL(@HireDate, HireDate),
            OfficeId = ISNULL(@OfficeId, OfficeId),
            DepartmentId = ISNULL(@DepartmentId, DepartmentId),
            CostCenterId = ISNULL(@CostCenterId, CostCenterId)
        WHERE EmployeeId = @EmployeeId;

        -- Only touch EmployeeAddress when the request actually supplied an address
        -- field; otherwise every ISNULL(@param, column) would resolve to the
        -- existing value and this would be a no-op write on every edit call.
        IF @Country IS NOT NULL OR @County IS NOT NULL OR @City IS NOT NULL
            OR @PostalCode IS NOT NULL OR @Street IS NOT NULL OR @StreetNumber IS NOT NULL
        BEGIN
            UPDATE dbo.EmployeeAddress
            SET
                Country = ISNULL(@Country, Country),
                County = ISNULL(@County, County),
                City = ISNULL(@City, City),
                PostalCode = ISNULL(@PostalCode, PostalCode),
                Street = ISNULL(@Street, Street),
                StreetNumber = ISNULL(@StreetNumber, StreetNumber)
            WHERE EmployeeId = @EmployeeId;
        END

        COMMIT TRANSACTION;

        SET @Result = 0;
        SET @Message = 'Employee details updated successfully.';
    END TRY
    BEGIN CATCH
        IF @@TRANCOUNT > 0
            ROLLBACK TRANSACTION;

        SET @Result = 500;
        SET @Message = CONCAT('Failed to update employee: ', ERROR_MESSAGE());
    END CATCH

    SELECT @Result AS Result, @Message AS Message;
END
