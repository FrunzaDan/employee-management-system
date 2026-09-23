CREATE TABLE [dbo].[tbl_offices]
(
    [PK_office_guid] UNIQUEIDENTIFIER NOT NULL,
    [office_name] NVARCHAR (100) NOT NULL,
    [city] NVARCHAR (100) NULL,
    [country] NVARCHAR (100) NULL,
    CONSTRAINT [PK_tbl_offices] PRIMARY KEY CLUSTERED ([PK_office_guid])
);
