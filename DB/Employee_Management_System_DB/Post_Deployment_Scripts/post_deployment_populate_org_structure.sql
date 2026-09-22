-- Demo offices/departments/cost centers so the job-info dropdowns aren't empty
-- on a fresh DB. IF NOT EXISTS guards make this script safe to re-run.
IF NOT EXISTS (SELECT 1 FROM tbl_offices WHERE office_name = 'Headquarters')
BEGIN
    INSERT INTO dbo.tbl_offices (PK_office_guid, office_name, city, country)
    VALUES
        (NEWID(), 'Headquarters', 'Bucharest', 'Romania'),
        (NEWID(), 'West Coast Office', 'San Francisco', 'United States'),
        (NEWID(), 'European Hub', 'Berlin', 'Germany');
END
GO

IF NOT EXISTS (SELECT 1 FROM tbl_departments WHERE department_name = 'Engineering')
BEGIN
    INSERT INTO dbo.tbl_departments (PK_department_guid, department_name)
    VALUES
        (NEWID(), 'Engineering'),
        (NEWID(), 'Human Resources'),
        (NEWID(), 'Sales'),
        (NEWID(), 'Finance');
END
GO

IF NOT EXISTS (SELECT 1 FROM tbl_cost_centers WHERE cost_center_code = 'CC-100')
BEGIN
    INSERT INTO dbo.tbl_cost_centers (PK_cost_center_guid, cost_center_code, cost_center_name)
    VALUES
        (NEWID(), 'CC-100', 'Product & Engineering'),
        (NEWID(), 'CC-200', 'General & Administrative'),
        (NEWID(), 'CC-300', 'Sales & Marketing');
END
