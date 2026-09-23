CREATE PROCEDURE [dbo].[usp_addEmployeeSalary]
    @var_SalaryGuid UNIQUEIDENTIFIER,
    @var_EmployeeGuid UNIQUEIDENTIFIER,
    @var_BruttoSalary DECIMAL(12, 2),
    @var_EffectiveDate DATE
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @result INT;
    DECLARE @message NVARCHAR(255);
    DECLARE @currentDateTime DATETIME2(3) = SYSUTCDATETIME();

    IF NOT EXISTS (SELECT 1 FROM tbl_employees WHERE PK_employee_guid = @var_EmployeeGuid)
    BEGIN
        SET @result = 404;
        SET @message = 'Employee not found.';

        SELECT @result AS result, @message AS message;
        RETURN;
    END

    BEGIN TRY
        INSERT INTO dbo.tbl_employee_salary_history
        (PK_salary_guid, FK_employee_guid, brutto_salary, effective_Date, created_Date)
        VALUES
        (@var_SalaryGuid, @var_EmployeeGuid, @var_BruttoSalary, @var_EffectiveDate, @currentDateTime);

        SET @result = 0;
        SET @message = 'Salary entry added successfully.';
    END TRY
    BEGIN CATCH
        SET @result = 500;
        SET @message = CONCAT('Failed to add salary entry: ', ERROR_MESSAGE());
    END CATCH

    SELECT @result AS result, @message AS message;
END
