IF NOT EXISTS (SELECT 1 FROM dbo.Department WHERE Name = 'Engineering')
BEGIN
    INSERT INTO dbo.Department (Name)
    VALUES
        ('Engineering'),
        ('Human Resources'),
        ('Sales'),
        ('Finance');
END
