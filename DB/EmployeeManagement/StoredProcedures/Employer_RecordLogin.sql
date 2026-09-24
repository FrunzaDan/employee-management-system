CREATE PROCEDURE [dbo].[Employer_RecordLogin]
    @Username NVARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    UPDATE dbo.Employer
    SET LastInteractionAt = SYSUTCDATETIME()
    WHERE Username = @Username;
END;
