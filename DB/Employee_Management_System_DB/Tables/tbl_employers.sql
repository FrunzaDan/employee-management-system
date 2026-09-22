CREATE TABLE [dbo].[tbl_employers] (
    [employer_id]      NVARCHAR (50) NOT NULL,
    [employer_password]    BINARY(32) NULL,
    [employer_password_salt] BINARY(16) NULL,
    [employer_role]      INT           NULL,
    [last_interaction] DATETIME      NULL,
    PRIMARY KEY (employer_id)
);
