IF NOT EXISTS (
    SELECT 1
    FROM tbl_employers
    WHERE employer_id = 'TestEmployerID'
)
BEGIN
    -- Password: 'Employer123', hashed with PBKDF2-HMACSHA256 (100,000 iterations) and the salt below,
    -- to match EmployeeManagementSystem.DataAccess.DBConnection.PasswordHasher.
    DECLARE @employerPasswordSalt BINARY(16) = 0xAB5AB08F9BE5CFB295D3C525B9A9F776;
    DECLARE @hashedEmployerPassword BINARY(32) = 0x87C4A43EFD1C28A1B4EA65975094BE49E8115FDEF2C4237332FF64C23DC94AFE;
    DECLARE @currDate DATETIME = GETDATE();

    INSERT INTO dbo.tbl_employers (
        employer_id,
        employer_password,
        employer_password_salt,
        employer_role,
        last_interaction
    )
    VALUES (
        'TestEmployerID',
        @hashedEmployerPassword,
        @employerPasswordSalt,
        '1801',
        @currDate
    );
END
