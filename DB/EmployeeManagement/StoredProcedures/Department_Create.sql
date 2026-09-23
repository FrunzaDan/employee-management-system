CREATE PROCEDURE [dbo].[Department_Create]
    @DepartmentId UNIQUEIDENTIFIER,
    @Name NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Result INT;
    DECLARE @Message NVARCHAR(255);

    BEGIN TRY
        INSERT INTO dbo.Department (DepartmentId, Name)
        VALUES (@DepartmentId, @Name);

        SET @Result = 0;
        SET @Message = CONCAT('Department created successfully. GUID: ', @DepartmentId);
    END TRY
    BEGIN CATCH
        SET @Result = 500;
        SET @Message = CONCAT('Failed to create department: ', ERROR_MESSAGE());
    END CATCH

    SELECT @Result AS Result, @Message AS Message;
END
