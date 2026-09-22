CREATE TABLE [dbo].[tbl_addresses] (
    [FK_employee_guid] NVARCHAR (50)  NOT NULL,
    [country]          NVARCHAR (100) NULL,
    [county]           NVARCHAR (100) NULL,
    [zip_code]         NVARCHAR (50) NULL,
    [town]             NVARCHAR (50) NULL,
    [street]           NVARCHAR (100) NULL,
    [number]           NVARCHAR (50) NULL,
    -- A FK doesn't auto-index its own (referencing) column — without this, every
    -- employee read/write (usp_getEmployee/usp_getEmployees' INNER JOIN, usp_editEmployee,
    -- usp_deleteEmployee) does a full scan of this table. It also enforces the 1:1
    -- employee-to-address relationship every proc already assumes but nothing previously
    -- guaranteed, and the tighter (seek, not scan) locking here reduces the deadlock
    -- surface between usp_createEmployee and usp_deleteEmployee's opposite table-access order.
    CONSTRAINT [UQ_tbl_addresses_FK_employee_guid] UNIQUE ([FK_employee_guid]),
    FOREIGN KEY (FK_employee_guid) REFERENCES tbl_employees(PK_employee_guid)
    ON UPDATE CASCADE
);
