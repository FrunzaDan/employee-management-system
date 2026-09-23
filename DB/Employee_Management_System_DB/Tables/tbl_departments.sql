CREATE TABLE [dbo].[tbl_departments]
(
    [PK_department_guid] UNIQUEIDENTIFIER NOT NULL,
    [department_name] NVARCHAR (100) NOT NULL,
    CONSTRAINT [PK_tbl_departments] PRIMARY KEY CLUSTERED ([PK_department_guid])
);
