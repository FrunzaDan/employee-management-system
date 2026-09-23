CREATE TABLE [dbo].[EmployeeAddress] (
    [EmployeeId]   UNIQUEIDENTIFIER NOT NULL,
    [Country]      NVARCHAR (100) NULL,
    [County]       NVARCHAR (100) NULL,
    -- Postal codes are ASCII letters/digits/spaces/hyphens worldwide (AddressValidation
    -- enforces that), so no Unicode needed.
    [PostalCode]   VARCHAR (20)   NULL,
    [City]         NVARCHAR (100) NULL,
    [Street]       NVARCHAR (100) NULL,
    [StreetNumber] NVARCHAR (50)  NULL,
    -- The employee's key *is* this table's key: the relationship is strictly 1:1, which every
    -- proc already assumes. As the clustered PK it replaces the old heap + UNIQUE index —
    -- every employee read/write (Employee_Get/Employee_List's INNER JOIN, Employee_Update,
    -- Employee_Delete) seeks it directly, and the tighter (seek, not scan) locking reduces
    -- the deadlock surface between Employee_Create and Employee_Delete's opposite
    -- table-access order.
    CONSTRAINT [PK_EmployeeAddress] PRIMARY KEY CLUSTERED ([EmployeeId]),
    CONSTRAINT [FK_EmployeeAddress_Employee] FOREIGN KEY ([EmployeeId])
        REFERENCES [dbo].[Employee] ([EmployeeId])
);
