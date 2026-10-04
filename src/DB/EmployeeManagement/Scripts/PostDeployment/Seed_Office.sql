IF NOT EXISTS (SELECT 1 FROM dbo.Office WHERE Name = 'Headquarters')
BEGIN
    INSERT INTO dbo.Office (Name, City, Country)
    VALUES
        ('Headquarters', 'Bucharest', 'Romania'),
        ('West Coast Office', 'San Francisco', 'United States'),
        ('European Hub', 'Berlin', 'Germany');
END
