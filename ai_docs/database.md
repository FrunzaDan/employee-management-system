# Database

## What it is

The `EmployeeManagement` SQL Server database, as an SSDT project under `DB/EmployeeManagement/`. All access goes through stored procedures.

## Key files / paths

- `Tables/`:
  - `Employee`, `EmployeeAddress`, `Employer`;
  - `EmployeeAuditLog`;
  - `Office`, `Department`, `CostCenter`;
  - `EmployeeSalary`.
- `StoredProcedures/` — procs named `<Entity>_<Verb>`, for example `Employee_List`, `EmployeeSalary_Create` and `Office_Delete`.
- `Scripts/PostDeployment/` — `PostDeployment.sql` `:r`-includes `Seed_Employer`, `Seed_Office`, `Seed_Department` and `Seed_CostCenter`.
- `global.json` — pins the .NET 8 SDK for this project. Keep it.

## How it works

### Tables

- **`Employee`:**
  - `EmployeeId` (`NEWSEQUENTIALID()`);
  - `FirstName`, `LastName`;
  - `Email` (unique) and `PhoneNumber` (`VARCHAR(15)`, unique);
  - `Gender` (`TINYINT`, 0/1/2);
  - `BirthDate` and `HireDate` (`DATE`);
  - `StatusCode` (`SMALLINT`, default 1901);
  - nullable FKs `OfficeId`, `DepartmentId`, `CostCenterId`, each indexed;
  - `CreatedAt`, `LastInteractionAt`.
- **`EmployeeAddress`:** one row per employee. `EmployeeId` is both the primary key and the foreign key. Every column is `NOT NULL`.
- **`Employer`:**
  - `Username` is the primary key;
  - `PasswordHash` `BINARY(32)` and `PasswordSalt` `BINARY(16)`;
  - `RoleCode`.
- **`Office`, `Department`, `CostCenter`:** small lookup tables with GUID keys. `CostCenter.Code` is unique.
- **`EmployeeSalary`:**
  - `EmployeeSalaryId` is an `INT IDENTITY`;
  - `EmployeeId`, `GrossSalary` `DECIMAL(12,2)`, `EffectiveDate`, `CreatedAt`;
  - it is **append-only**: no edit or delete.
- **`EmployeeAuditLog`:**
  - `EmployeeAuditLogId` is an `INT IDENTITY`;
  - `ActionType` is `Created`, `Edited`, `Deactivated`, `Reactivated`, `Deleted` or `SalaryChanged`;
  - it has **no FK** to `Employee`, so history survives a delete.

### Procedures

- **`Employee_List`:** paged with `OFFSET`/`FETCH`. It searches with `LIKE` (wildcards escaped) and sorts through a `CASE` `ORDER BY` (no dynamic SQL). The total comes from `COUNT(*) OVER()`. `/export` reuses it with page size 5000.
- **Current salary:** `Employee_Get` and `Employee_List` add `CurrentGrossSalary` via `OUTER APPLY (TOP 1 … ORDER BY EffectiveDate DESC, CreatedAt DESC)`.
- **`Employee_Get`:** one `IF` branch each for id, phone number and email, so each gets an index seek.
- **`Employee_Update`:** a partial update (`ISNULL(@x, column)`) that updates the employee and address rows in one transaction.
- **Employee create/update:** check that the office, department and cost center exist first, and return `400` if one doesn't.
- **`{Office,Department,CostCenter}_List`:** unpaginated. They add `EmployeeCount` and `TotalGrossSalary`.
- **`<Org>_Delete`:** returns `409` while any employee still references the row.
- **`Employee_ListByOffice`/`ByDepartment`/`ByCostCenter`:** the employees assigned to one office, department or cost center.
- **`EmployeeAuditLog_List`:** paged. The total is a separate first result set, so an empty page still reports the right total.

### Employee lifecycle (enforced in the procs)

- New employees are active (`1901`). The generator creates test employees (`1904`).
- `Deactivate` needs an employee that isn't already deactivated. `Reactivate` needs one that isn't already active. Otherwise the result is `409`.
- `Delete` needs status `1903` or `1904`. It deletes the address, then the salary rows, then the employee, in one transaction.
- Email and phone number duplicates are checked first (`409`). The unique constraints catch the race window, and the `CATCH` maps errors 2601/2627 to the same `409`.
- Creating an employee sets no salary. The first salary is added through `EmployeeSalary_Create`, like any later raise.

### Error handling (all procs)

- Every proc starts with `SET NOCOUNT ON; SET XACT_ABORT ON`.
- Expected outcomes come back as `(Result, Message)` rows:
  - `400`: a referenced row is missing;
  - `404`: not found;
  - `409`: a duplicate or a state conflict.
- Multi-statement writes use `TRY` / `BEGIN TRANSACTION` / `CATCH` → `ROLLBACK` + `THROW`. Nothing returns `ERROR_MESSAGE()`.

### Naming and data types (shared by all three apps)

- **Naming:**
  - PascalCase everywhere, with no `tbl_`/`usp_`/`sp_` prefixes.
  - Tables are singular.
  - The primary key is `<Table>Id`.
  - A column doesn't repeat its table's name (`Office.Name`).
  - Column suffixes: `…At` is a UTC `DATETIME2(3)`, `…Date` is a `DATE`, `…Code` is a checked code.
  - Constraints are always named: `PK_`, `FK_`, `UQ_`, `CK_`, `DF_`, `IX_`.
- **Types:**
  - Entity keys are `UNIQUEIDENTIFIER` + `NEWSEQUENTIALID()`; history keys are `INT IDENTITY`.
  - Names are `NVARCHAR(100)`, email is `NVARCHAR(254)`, phone number is `VARCHAR(15)` (digits only).
  - Money is `DECIMAL(12,2)`.
  - Timestamps are written with `SYSUTCDATETIME()`.
- **Parameters:** every parameter has exactly its column's type. A `VARCHAR` column compared with an `NVARCHAR` parameter loses its index seek.
- **API/UI names:** JSON and TypeScript names are the camelCase column names. A code column loses its `Code` suffix in C# and TypeScript: `StatusCode` → `status`.

## Gotchas / conventions

- Keep `run.sh`'s deploy (`sqlpackage` publish) as the only way the schema changes. There are no migration scripts.
- Org FKs are `NO ACTION`. Deleting an office, department or cost center is blocked while it's in use; there is no cascade.
- There are no unit tests for procs. They are exercised by running the real stack.
