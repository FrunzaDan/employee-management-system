CREATE PROCEDURE [dbo].[Employer_GetAuthData]
    @Username NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    UPDATE dbo.Employer
    SET LastInteractionAt = SYSUTCDATETIME()
    WHERE Username = @Username;

    SELECT
        PasswordHash,
        PasswordSalt,
        RoleCode
    FROM dbo.Employer
    WHERE Username = @Username;
END;
