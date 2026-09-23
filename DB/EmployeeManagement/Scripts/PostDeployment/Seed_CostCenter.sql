-- Demo cost centers so the job-info dropdowns aren't empty on a fresh DB. The IF NOT EXISTS
-- guard makes this safe to re-run.
IF NOT EXISTS (SELECT 1 FROM dbo.CostCenter WHERE Code = 'CC-100')
BEGIN
    INSERT INTO dbo.CostCenter (Code, Name)
    VALUES
        ('CC-100', 'Product & Engineering'),
        ('CC-200', 'General & Administrative'),
        ('CC-300', 'Sales & Marketing');
END
