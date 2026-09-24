# Employee Management System — Index

## What it is

A learning full-stack CRUD app: an employer logs in and manages employee records, job info, salary history and the org structure (offices, departments, cost centers).

## Key files / paths

| Layer | Folder | Tech |
|---|---|---|
| UI | `UI/` | Angular 22 (zoneless, signals, SSR) |
| API | `API/EmployeeManagementSystemApi/` | .NET 10 ASP.NET Core Web API |
| DB | `DB/EmployeeManagement/` | SQL Server, SSDT `.sqlproj` deployed with `sqlpackage` |

- `build.sh` — build and test everything; starts nothing.
- `run.sh` — start the Docker database, deploy the schema, then start the API and UI.
- `API/Postman/` — Postman collection for manual API calls.
- `Documentation/Diagrams/` — early sketches; the docs below win where they disagree.

## How it works

- **UI → API:** JSON over HTTPS; every call after login carries `Authorization: Bearer <jwt>`.
- **API → DB:** `WebAPI` (controllers) → `BusinessLogic` (validation, JWT) → `DataAccess` (ADO.NET) → stored procedures only. `Domain` holds the shared models and options.
- **Result convention:** every mutating proc returns a `(Result, Message)` row. `Result = 0` means success; anything else is the HTTP status.
- **Features:**
  - login;
  - employee CRUD with an active/deactivated/test lifecycle;
  - a server-side paged, searchable and sortable list;
  - bulk actions and CSV export;
  - per-employee and global audit logs;
  - job info and append-only salary history;
  - admin pages for offices, departments and cost centers;
  - a test-data generator.

## Documented Concepts

- [api](api.md) — pipeline, configuration, database connection, errors, logging, endpoints, JWT, tests.
- [database](database.md) — tables, procs, lifecycle, audit log, org structure and salary, naming and data types.
- [angular-frontend](angular-frontend.md) — config, auth, routes, data loading, forms, feedback, styling, tests.
- [build-and-run](build-and-run.md) — Docker SQL, `build.sh`/`run.sh`, test login, TLS trust.
- [learning_approach](learning_approach.md) — how these docs are written and grown.

## Glossary

- **Employer** — the logged-in user (table `Employer`). **Employee** — the managed record.
- **StatusCode** — `1901` active, `1903` deactivated, `1904` test.
- **RoleCode** — `1801`, the only role.
- **Current gross salary** — the latest `EmployeeSalary` row by `EffectiveDate`, then `CreatedAt`.

## Gotchas / conventions

- The sibling apps (customer-management-system, imalo-education-webapp) are kept aligned on purpose: names, data types, error handling, logging and the database connection.
- Rough edges noted under Gotchas are known and accepted, not a TODO list.
