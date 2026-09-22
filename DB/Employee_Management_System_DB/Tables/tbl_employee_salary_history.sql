CREATE TABLE [dbo].[tbl_employee_salary_history]
(
    [PK_salary_guid]   NVARCHAR (50)   NOT NULL,
    [FK_employee_guid] NVARCHAR (50)   NOT NULL,
    [brutto_salary]    DECIMAL (12, 2) NOT NULL,
    [effective_Date]   NVARCHAR (50)   NOT NULL,
    [created_Date]     NVARCHAR (50)   NULL,
    PRIMARY KEY (PK_salary_guid),
    FOREIGN KEY (FK_employee_guid) REFERENCES tbl_employees(PK_employee_guid)
    ON UPDATE CASCADE
);
GO

-- Backs both "current salary" (TOP 1 ... ORDER BY effective_Date DESC in
-- usp_getEmployee/usp_getEmployees) and usp_getEmployeeSalaryHistory's listing —
-- without this every employee read would scan the whole history table.
CREATE INDEX [IX_tbl_employee_salary_history_employee_effective]
    ON [dbo].[tbl_employee_salary_history] ([FK_employee_guid], [effective_Date] DESC);
