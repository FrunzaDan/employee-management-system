CREATE PROCEDURE [dbo].[Department_Get]
    @DepartmentId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;

    SELECT DepartmentId, Name
    FROM dbo.Department
    WHERE DepartmentId = @DepartmentId;
END
