CREATE TABLE [dbo].[tbl_employees]
(
    [PK_employee_guid] NVARCHAR (50) NOT NULL,
    [first_name] NVARCHAR (50) NULL,
    [last_name] NVARCHAR (50) NULL,
    -- NOT NULL (not just the UQ_ constraints below): SQL Server treats every NULL as
    -- distinct under UNIQUE, so a NULL email/msisdn would silently bypass both the unique
    -- constraint and usp_createEmployee's own duplicate pre-check (`= NULL` never matches).
    [email] NVARCHAR (50) NOT NULL,
    [msisdn] NVARCHAR (50) NOT NULL,
    [gender] INT NULL,
    [birthdate] NVARCHAR (50) NULL,
    [employee_Status] INT NULL,
    [creation_Date] NVARCHAR (50) NULL,
    [interaction_Date] NVARCHAR (50) NULL,
    -- Job info: independent of the address/status fields above and always optional at
    -- the schema level — usp_createEmployee/usp_editEmployee pre-check these guids exist
    -- (friendly 400) before they'd ever hit these FKs.
    [hire_Date] NVARCHAR (50) NULL,
    [FK_office_guid] NVARCHAR (50) NULL,
    [FK_department_guid] NVARCHAR (50) NULL,
    [FK_cost_center_guid] NVARCHAR (50) NULL,
    PRIMARY KEY (PK_employee_guid),
    CONSTRAINT [UQ_tbl_employees_email] UNIQUE ([email]),
    CONSTRAINT [UQ_tbl_employees_msisdn] UNIQUE ([msisdn]),
    -- No ON DELETE CASCADE: deleting an office/department/cost-center while an employee
    -- still references it should be blocked (see usp_delete*'s friendly pre-check), not
    -- silently null out the employee's assignment.
    FOREIGN KEY (FK_office_guid) REFERENCES tbl_offices(PK_office_guid),
    FOREIGN KEY (FK_department_guid) REFERENCES tbl_departments(PK_department_guid),
    FOREIGN KEY (FK_cost_center_guid) REFERENCES tbl_cost_centers(PK_cost_center_guid)
);
GO

-- Supports usp_getEmployees' default (no @SearchTerm) listing and its 'name' sort —
-- the common, unfiltered page-load path. Doesn't help the LIKE '%...%' search branches
-- (a leading wildcard can't seek any index), only the plain listing/sort case.
CREATE INDEX [IX_tbl_employees_last_first]
    ON [dbo].[tbl_employees] ([last_name], [first_name]);
