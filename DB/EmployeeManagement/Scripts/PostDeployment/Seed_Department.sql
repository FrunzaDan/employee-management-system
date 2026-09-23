-- Demo departments so the job-info dropdowns aren't empty on a fresh DB. The IF NOT EXISTS
-- guard makes this safe to re-run.
IF NOT EXISTS (SELECT 1 FROM dbo.Department WHERE Name = 'Engineering')
BEGIN
    INSERT INTO dbo.Department (Name)
    VALUES
        ('Engineering'),
        ('Human Resources'),
        ('Sales'),
        ('Finance');
END
