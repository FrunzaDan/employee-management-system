CREATE PROCEDURE [dbo].[usp_getEmployerAuthData]
    @var_EmployerID NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE tbl_employers
    SET last_interaction = SYSUTCDATETIME()
    WHERE employer_id = @var_EmployerID;

    SELECT
        employer_password AS password_hash,
        employer_password_salt AS password_salt,
        employer_role
    FROM tbl_employers
    WHERE employer_id = @var_EmployerID;
END;
