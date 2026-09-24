CREATE TABLE [dbo].[EmployeeAddress]
(
    [EmployeeId] UNIQUEIDENTIFIER NOT NULL,
    -- NOT NULL: every address field is required by both the API and the UI.
    [Country] NVARCHAR (100) NOT NULL,
    [County] NVARCHAR (100) NOT NULL,
    -- Postal codes are ASCII letters/digits/spaces/hyphens worldwide (AddressValidation
    -- enforces that), so no Unicode needed.
    [PostalCode] VARCHAR (20) NOT NULL,
    [City] NVARCHAR (100) NOT NULL,
    [Street] NVARCHAR (100) NOT NULL,
    [StreetNumber] NVARCHAR (50) NOT NULL,
    -- The employee's key is the natural primary key of a 1:1 child row. As a clustered PK
    -- it both enforces the 1:1 employee-to-address relationship every proc assumes and gives
    -- the FK column its index (a FK doesn't index its own referencing column) — without it
    -- every employee read/write (Employee_Get/Employee_List's INNER JOIN, Employee_Update,
    -- Employee_Delete) would scan this table. The tighter (seek, not scan) locking also
    -- reduces the deadlock surface between Employee_Create and Employee_Delete's opposite
    -- table-access order.
    CONSTRAINT [PK_EmployeeAddress] PRIMARY KEY CLUSTERED ([EmployeeId]),
    CONSTRAINT [FK_EmployeeAddress_Employee]
        FOREIGN KEY ([EmployeeId]) REFERENCES [dbo].[Employee] ([EmployeeId])
);
