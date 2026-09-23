CREATE TABLE [dbo].[tbl_addresses] (
    [FK_employee_guid] UNIQUEIDENTIFIER NOT NULL,
    [country]          NVARCHAR (100) NULL,
    [county]           NVARCHAR (100) NULL,
    -- Postal codes are ASCII letters/digits/spaces/hyphens worldwide (AddressValidation
    -- enforces that), so no Unicode needed.
    [zip_code]         VARCHAR (20)   NULL,
    [town]             NVARCHAR (100) NULL,
    [street]           NVARCHAR (100) NULL,
    [number]           NVARCHAR (50)  NULL,
    -- The employee GUID *is* this table's key: the relationship is strictly 1:1, which every
    -- proc already assumes. As the clustered PK it replaces the old heap + UNIQUE index —
    -- every employee read/write (usp_getEmployee/usp_getEmployees' INNER JOIN, usp_editEmployee,
    -- usp_deleteEmployee) seeks it directly, and the tighter (seek, not scan) locking reduces
    -- the deadlock surface between usp_createEmployee and usp_deleteEmployee's opposite
    -- table-access order.
    CONSTRAINT [PK_tbl_addresses] PRIMARY KEY CLUSTERED ([FK_employee_guid]),
    CONSTRAINT [FK_tbl_addresses_tbl_employees] FOREIGN KEY ([FK_employee_guid])
        REFERENCES [dbo].[tbl_employees] ([PK_employee_guid])
);
