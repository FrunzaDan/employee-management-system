-- Demo offices so the job-info dropdowns aren't empty on a fresh DB (Seed_Department and
-- Seed_CostCenter do the same for theirs). The IF NOT EXISTS guard makes this safe to re-run.
IF NOT EXISTS (SELECT 1 FROM dbo.Office WHERE Name = 'Headquarters')
BEGIN
    INSERT INTO dbo.Office (OfficeId, Name, City, Country)
    VALUES
        (NEWID(), 'Headquarters', 'Bucharest', 'Romania'),
        (NEWID(), 'West Coast Office', 'San Francisco', 'United States'),
        (NEWID(), 'European Hub', 'Berlin', 'Germany');
END
