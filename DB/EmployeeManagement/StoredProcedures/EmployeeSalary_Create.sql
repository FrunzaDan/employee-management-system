CREATE PROCEDURE [dbo].[EmployeeSalary_Create]
    @EmployeeId UNIQUEIDENTIFIER,
    @GrossSalary DECIMAL(12, 2),
    @EffectiveDate DATE
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Result INT;
    DECLARE @Message NVARCHAR(255);

    IF NOT EXISTS (SELECT 1 FROM dbo.Employee WHERE EmployeeId = @EmployeeId)
    BEGIN
        SET @Result = 404;
        SET @Message = 'Employee not found.';

        SELECT @Result AS Result, @Message AS Message;
        RETURN;
    END

    BEGIN TRY
        -- EmployeeSalaryId (IDENTITY) and CreatedAt (SYSUTCDATETIME()) come from the table.
        INSERT INTO dbo.EmployeeSalary (EmployeeId, GrossSalary, EffectiveDate)
        VALUES (@EmployeeId, @GrossSalary, @EffectiveDate);

        SET @Result = 0;
        SET @Message = 'Salary entry added successfully.';
    END TRY
    BEGIN CATCH
        SET @Result = 500;
        SET @Message = CONCAT('Failed to add salary entry: ', ERROR_MESSAGE());
    END CATCH

    SELECT @Result AS Result, @Message AS Message;
END
