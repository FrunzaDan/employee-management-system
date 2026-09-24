CREATE PROCEDURE [dbo].[Employee_ListByOffice]
    @OfficeId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    SELECT EmployeeId, FirstName, LastName, Email, StatusCode
    FROM dbo.Employee
    WHERE OfficeId = @OfficeId
    ORDER BY LastName, FirstName;
END
