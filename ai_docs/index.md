# Employee Management System — Index

## What it is

A learning full-stack CRUD app: an employer logs in and manages employee records, job info, salary history and the org structure (offices, departments, cost centers).

## Key files / paths

| Layer | Folder | Tech |
|---|---|---|
| UI | `src/UI/` | Angular 22 (zoneless, signals, Signal Forms, SSR) |
| API | `src/API/EmployeeManagementSystemApi/` | .NET 10 ASP.NET Core Web API, 4 projects + tests |
| DB | `src/DB/EmployeeManagement/` | SQL Server, SSDT `.sqlproj` deployed with `sqlpackage` |

- `build.sh` — build and test everything; starts nothing.
- `run.sh` — start the Docker database, deploy the schema, then start the API and UI.
- `src/API/Postman/` — Postman collection for manual API calls.

## How it works

### Architecture

```
Browser ──► Angular dev server :4205 (SSR via Express in Node)
              │  JSON over HTTPS, Authorization: Bearer <jwt>
              ▼
           ASP.NET Core API :7146
             WebAPI (controllers, ApiControllerBase.Reply, GlobalExceptionHandler)
               → BusinessLogic (services → EmployeeFunctions / OrgFunctions, validation, JWT)
               → IDbUtils (declared in BusinessLogic, implemented by DataAccess: DbUtils/DbHelper, ADO.NET, typed SqlParameters)
             Domain (models, options, constants) is shared by all three
              │  stored procedures only
              ▼
           SQL Server (Azure SQL Edge container "sqlserver" :1433, database EmployeeManagement)
```

- **Result convention:** every mutating proc returns a `(Result, Message)` row. `Result = 0` means success; anything else is the HTTP status. The API wraps successes in `ResponseModel<T>` and turns every failure into RFC 9457 Problem Details.
- **Auth:** login returns a 15-minute JWT, kept in `sessionStorage`. The UI's `authGuard` verifies it before each protected route; an interceptor attaches it to every API call.
- **Org structure:** offices, departments and cost centers are small lookup tables that employees reference by nullable FK. Their list procs add head count and total gross salary.

### A request end to end (adding a raise)

1. `employee-details` → `SalaryHistoryService` → `POST api/employee/salary-history`.
2. `EmployeeController` → `IEmployeeService` → `EmployeeSalary` validates the amount → `IDbUtils` calls `EmployeeSalary_Create` (append-only).
3. The proc's `(Result, Message)` row becomes a `ResponseModel`; `Reply()` returns it, or Problem Details for a non-success.
4. `EmployeeAuditLogger` writes a `SalaryChanged` row (best-effort). The details page reloads the employee, whose current salary is the latest entry already in effect.

### Features

- login;
- employee CRUD with an active/deactivated/test lifecycle;
- a server-side paged, searchable and sortable list;
- bulk actions and CSV export;
- per-employee and global audit logs;
- job info and append-only salary history;
- admin pages for offices, departments and cost centers;
- a charts dashboard (KPIs, workforce, pay, salary growth);
- a test-data generator.

## Documented Concepts

- [api](api.md) — pipeline, configuration, database connection, errors, logging, endpoints, naming, JWT, tests.
- [database](database.md) — tables, procs, current-salary rule, lifecycle, org structure, naming and data types.
- [angular-frontend](angular-frontend.md) — config, render modes, auth, routes, data loading, charts, forms, feedback, styling, tests.
- [build-and-run](build-and-run.md) — Docker SQL, `build.sh`/`run.sh`, test login, TLS trust.
- [learning_approach](learning_approach.md) — how these docs are written and grown.

### Where to look

| Question | Doc → section |
|---|---|
| Add or change an endpoint | api → Endpoints, Naming, Validation and data types |
| Add a column or proc | database → Naming and data types; api → Gotchas (the four places a column lives) |
| How "current salary" is decided | database → Procedures (current salary) |
| Why a request returned 4xx/5xx | api → Errors; database → Error handling |
| Add a page or chart | angular-frontend → Routes, Data loading, Charts |
| Something won't start | build-and-run → Gotchas |
| Code shared with the sibling apps | angular-frontend → Gotchas (shared files) |

## Glossary

- **Employer** — the logged-in user (table `Employer`). **Employee** — the managed record.
- **StatusCode** — `1901` active, `1903` deactivated, `1904` test.
- **RoleCode** — `1801`, the only role.
- **Current gross salary** — the latest `EmployeeSalary` row already in effect (`EffectiveDate <= today`), by `EffectiveDate`, then `CreatedAt`.
- **Org unit** — an office, department or cost center.

## Gotchas / conventions

- The sibling apps (customer-management-system, imalo-education-webapp) are kept aligned on purpose: names, data types, error handling, logging, the database connection, and a set of identical UI files. Ports: customer 4204/7145, employee 4205/7146, Imalo 4203/7244; all three share the `sqlserver` container.
- Office, department and cost-center changes are not audit-logged; salary changes are.
- Rough edges noted under Gotchas are known and accepted, not a TODO list.
