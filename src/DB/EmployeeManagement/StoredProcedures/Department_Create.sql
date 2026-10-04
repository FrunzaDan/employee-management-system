CREATE PROCEDURE [dbo].[Department_Create]
    @Name NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @Result INT;
    DECLARE @Message NVARCHAR(255);
    DECLARE @DepartmentId UNIQUEIDENTIFIER = NULL;
    DECLARE @Inserted TABLE (DepartmentId UNIQUEIDENTIFIER);

    INSERT INTO dbo.Department (Name)
    OUTPUT inserted.DepartmentId INTO @Inserted
    VALUES (@Name);

    SELECT @DepartmentId = DepartmentId FROM @Inserted;

    SET @Result = 0;
    SET @Message = 'Department created successfully.';

    SELECT @Result AS Result, @Message AS Message, @DepartmentId AS DepartmentId;
END
