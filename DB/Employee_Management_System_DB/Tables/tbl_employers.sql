CREATE TABLE [dbo].[tbl_employers] (
    [employer_id]            NVARCHAR (50) NOT NULL,
    [employer_password]      BINARY (32)   NOT NULL,
    [employer_password_salt] BINARY (16)   NOT NULL,
    -- 1801 is the only role in use (see ai_docs/api.md); SMALLINT like employee_Status.
    [employer_role]          SMALLINT      NOT NULL,
    -- UTC, same convention as tbl_employees' timestamps.
    [last_interaction]       DATETIME2 (3) NULL,
    CONSTRAINT [PK_tbl_employers] PRIMARY KEY CLUSTERED ([employer_id])
);
