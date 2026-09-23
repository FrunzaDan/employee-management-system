CREATE TABLE [dbo].[Employer] (
    -- The employer's login name (the API calls it "Employer ID").
    [Username]          NVARCHAR (50) NOT NULL,
    -- PBKDF2 output + its per-user salt (see DataAccess's PasswordHasher); never the password.
    [PasswordHash]      BINARY (32)   NOT NULL,
    [PasswordSalt]      BINARY (16)   NOT NULL,
    -- 1801 is the only role in use (see ai_docs/api.md); SMALLINT like Employee.StatusCode.
    [RoleCode]          SMALLINT      NOT NULL,
    -- UTC, same convention as Employee's timestamps.
    [LastInteractionAt] DATETIME2 (3) NULL,
    CONSTRAINT [PK_Employer] PRIMARY KEY CLUSTERED ([Username])
);
