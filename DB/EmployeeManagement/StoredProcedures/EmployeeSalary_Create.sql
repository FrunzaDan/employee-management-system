CREATE PROCEDURE [dbo].[EmployeeSalary_Create]
    @EmployeeId UNIQUEIDENTIFIER,
    @GrossSalary DECIMAL(12, 2),
    @EffectiveDate DATE
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @Result INT;
    DECLARE @Message NVARCHAR(255);

    IF NOT EXISTS (SELECT 1 FROM dbo.Employee WHERE EmployeeId = @EmployeeId)
    BEGIN
        SET @Result = 404;
        SET @Message = 'Employee not found.';

        SELECT @Result AS Result, @Message AS Message;
        RETURN;
    END

    -- EmployeeSalaryId (IDENTITY) and CreatedAt (SYSUTCDATETIME()) come from the table.
    INSERT INTO dbo.EmployeeSalary (EmployeeId, GrossSalary, EffectiveDate)
    VALUES (@EmployeeId, @GrossSalary, @EffectiveDate);

    SET @Result = 0;
    SET @Message = 'Salary entry added successfully.';

    SELECT @Result AS Result, @Message AS Message;
END
