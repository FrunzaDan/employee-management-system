CREATE TABLE [dbo].[Employer]
(
    [Username] NVARCHAR (50) NOT NULL,
    [PasswordHash] BINARY (32) NOT NULL,
    [PasswordSalt] BINARY (16) NOT NULL,
    [RoleCode] SMALLINT NOT NULL,
    [LastInteractionAt] DATETIME2 (3) NULL,
    CONSTRAINT [PK_Employer] PRIMARY KEY CLUSTERED ([Username])
);
