CREATE PROCEDURE [dbo].[EmployeeSalary_Create]
    @EmployeeSalaryId UNIQUEIDENTIFIER,
    @EmployeeId UNIQUEIDENTIFIER,
    @GrossSalary DECIMAL(12, 2),
    @EffectiveDate DATE
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

    BEGIN TRY
        INSERT INTO dbo.EmployeeSalary
        (EmployeeSalaryId, EmployeeId, GrossSalary, EffectiveDate, CreatedAt)
        VALUES
        (@EmployeeSalaryId, @EmployeeId, @GrossSalary, @EffectiveDate, @Now);

        SET @Result = 0;
        SET @Message = 'Salary entry added successfully.';
    END TRY
    BEGIN CATCH
        SET @Result = 500;
        SET @Message = CONCAT('Failed to add salary entry: ', ERROR_MESSAGE());
    END CATCH

    SELECT @Result AS Result, @Message AS Message;
END
