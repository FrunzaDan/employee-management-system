-- An SSDT project allows exactly one post-deployment script, so this is the single
-- entry point; each seed lives in its own file and is pulled in with :r at build time.
:r ./Seed_Employer.sql
GO
:r ./Seed_Office.sql
GO
:r ./Seed_Department.sql
GO
:r ./Seed_CostCenter.sql
