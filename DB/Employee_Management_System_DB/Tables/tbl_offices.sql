CREATE TABLE [dbo].[tbl_offices]
(
    [PK_office_guid] NVARCHAR (50) NOT NULL,
    [office_name] NVARCHAR (100) NOT NULL,
    [city] NVARCHAR (50) NULL,
    [country] NVARCHAR (100) NULL,
    PRIMARY KEY (PK_office_guid)
);
