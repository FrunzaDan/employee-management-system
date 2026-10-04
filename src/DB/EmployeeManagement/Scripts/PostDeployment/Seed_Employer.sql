IF NOT EXISTS (
    SELECT 1
    FROM dbo.Employer
    WHERE Username = 'TestEmployerID'
)
BEGIN
    DECLARE @PasswordSalt BINARY(16) = 0xAB5AB08F9BE5CFB295D3C525B9A9F776;
    DECLARE @PasswordHash BINARY(32) = 0x87C4A43EFD1C28A1B4EA65975094BE49E8115FDEF2C4237332FF64C23DC94AFE;
    DECLARE @Now DATETIME2(3) = SYSUTCDATETIME();

    INSERT INTO dbo.Employer (
        Username,
        PasswordHash,
        PasswordSalt,
        RoleCode,
        LastInteractionAt
    )
    VALUES (
        'TestEmployerID',
        @PasswordHash,
        @PasswordSalt,
        1801,
        @Now
    );
END
