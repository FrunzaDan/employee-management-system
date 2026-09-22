CREATE TABLE [dbo].[tbl_cost_centers]
(
    [PK_cost_center_guid] NVARCHAR (50) NOT NULL,
    [cost_center_code] NVARCHAR (50) NOT NULL,
    [cost_center_name] NVARCHAR (100) NULL,
    PRIMARY KEY (PK_cost_center_guid),
    CONSTRAINT [UQ_tbl_cost_centers_code] UNIQUE ([cost_center_code])
);
