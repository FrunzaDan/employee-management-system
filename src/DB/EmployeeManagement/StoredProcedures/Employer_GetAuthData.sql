CREATE PROCEDURE [dbo].[Employer_GetAuthData]
    @Username NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    SELECT
        PasswordHash,
        PasswordSalt,
        RoleCode
    FROM dbo.Employer
    WHERE Username = @Username;
END;
