CREATE PROCEDURE [dbo].[Department_Create]
    @Name NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @Result INT;
    DECLARE @Message NVARCHAR(255);
    DECLARE @DepartmentId UNIQUEIDENTIFIER = NULL;
    DECLARE @Inserted TABLE (DepartmentId UNIQUEIDENTIFIER);

    BEGIN TRY
        -- DepartmentId comes from the table's NEWSEQUENTIALID() default.
        INSERT INTO dbo.Department (Name)
        OUTPUT inserted.DepartmentId INTO @Inserted
        VALUES (@Name);

        SELECT @DepartmentId = DepartmentId FROM @Inserted;

        SET @Result = 0;
        SET @Message = 'Department created successfully.';
    END TRY
    BEGIN CATCH
        SET @Result = 500;
        SET @Message = CONCAT('Failed to create department: ', ERROR_MESSAGE());
    END CATCH

    -- DepartmentId: the new department's server-generated key; only meaningful when Result = 0.
    SELECT @Result AS Result, @Message AS Message, @DepartmentId AS DepartmentId;
END
