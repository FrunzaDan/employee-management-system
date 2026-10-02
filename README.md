# Employee Management System

A full-stack CRUD app for an employer to manage employee records, job details, salary history and the org structure (offices, departments, cost centers), behind a JWT login. It's the sibling of the Customer Management System: same stack and conventions, with an organization model on top.

---

## 🚀 Key Features

- **JWT login:** Employer sign-in issues a 15-minute HMAC-SHA256 token; passwords are hashed with salted PBKDF2 and the login endpoint is rate-limited per IP.
- **Employee lifecycle:** Create, view, edit, deactivate, reactivate and delete employees. An active employee must be deactivated before it can be deleted.
- **Job info and salary history:** Each employee has a hire date, office, department and cost center, and a dated salary history in which every change is audit-logged.
- **Org structure:** Offices, departments and cost centers each have a list page (headcount, total gross salary) and a details page with their employees.
- **Server-side list handling:** Search, sort and pagination happen in SQL. Bulk actions deactivate or delete a selection in one go.
- **Audit log, charts and CSV export:** Per-employee and global audit history, an insights/charts page, and CSV export of the current list.
- **Test data generator:** The About page adds 50 demo employees. Test employees skip the deactivate-before-delete rule.

---

## 🛠 Tech Stack

- **Frontend:** Angular 22.2 (standalone components, signals, zoneless), SSR via `@angular/ssr` + Express, Bootstrap 5, TypeScript
- **Backend:** ASP.NET Core Web API on .NET 10 (controllers), layered as WebAPI → BusinessLogic → DataAccess → Domain
- **Database / Storage:** SQL Server (Azure SQL Edge in Docker), ADO.NET with stored procedures only (no ORM), SSDT project deployed with `sqlpackage`
- **Tooling & Other:** OpenAPI + Swagger UI, xUnit v3 + Moq (Microsoft Testing Platform), Vitest + jsdom, Prettier, StyleCop/Roslynator analyzers, Postman collection

---

## 📋 Prerequisites

Before running this project, ensure you have the following installed:

- .NET 10 SDK (10.0.401 or newer, pinned in `global.json`)
- .NET 8 SDK (the database project's `DB/EmployeeManagement/global.json` pins it for the SQL build tooling)
- Node.js `^22.22.3`, `^24.15.0` or `>=26` with npm
- Docker Desktop (runs the SQL Server container)
- A trusted ASP.NET Core dev certificate: `dotnet dev-certs https --trust` (once per machine)

`sqlpackage` is installed automatically as a global dotnet tool by `run.sh` if it is missing.

---

## ⚙️ Local Setup & Running

### 1. Clone the repository

```bash
git clone https://github.com/FrunzaDan/employee-management-system.git
cd employee-management-system
```

### 2. Configuration

Everything works out of the box for local development. The relevant settings live in `API/EmployeeManagementSystemApi/EmployeeManagementSystem.WebAPI/appsettings.json`:

- `ConnectionStrings:Docker` points at the container on `localhost,1433`. On Windows, the API falls back to `ConnectionStrings:LocalSqlServer` (Windows auth) if Docker doesn't answer within 3 seconds.
- `Auth` holds the JWT key, issuer, audience and token lifetime. The key is a placeholder for local use only.
- `Cors:AllowedOrigins` allows the Angular dev server on port 4206.

`run.sh` reads these environment variables if you need to override the defaults: `SQL_SA_PASSWORD`, `SQL_PORT`, `SQL_CONTAINER_NAME`, `SQL_IMAGE` and `SQL_PLATFORM`.

### 3. Installation & Run

```bash
./run.sh
```

This starts Docker if needed, creates or starts the `sqlserver` container, builds and publishes the database schema, starts the API in the background on `https://localhost:7146`, and then runs the Angular dev server in the foreground on `http://localhost:4206`. `Ctrl+C` stops the API and Angular; the database container keeps running.

Log in with the seeded test account:

```
Employer ID: TestEmployerID
Password:    Employer123
```

To build and test everything without starting any services:

```bash
./build.sh
```

That restores and builds the .NET solution, runs the xUnit tests, builds the SQL project, then runs `npm ci`, the production build and the Vitest suite for the UI.

---

## 🗄 Database & Migrations

There are no EF migrations. The schema is an SSDT project in `DB/EmployeeManagement`:

- Tables: `Employee`, `EmployeeAddress`, `EmployeeSalary`, `EmployeeAuditLog`, `Employer`, `Office`, `Department` and `CostCenter`.
- All data access goes through stored procedures named `<Entity>_<Verb>`.
- Post-deployment scripts seed the test employer and demo offices, departments and cost centers.

`run.sh` builds the project into a `.dacpac` and publishes it with `sqlpackage`, which diffs the target database and applies only the changes.

The SQL container is shared with the customer and Imalo apps (same `sqlserver` container on port 1433, one database per app). To start it by hand:

```bash
docker run -e "ACCEPT_EULA=1" -e "MSSQL_SA_PASSWORD=MyStrongPassw0rd?" \
  -p 1433:1433 --name sqlserver -d mcr.microsoft.com/azure-sql-edge
```

---

## 🔌 API / App Usage

Swagger UI is available at `https://localhost:7146/swagger` in Development, with a bearer-token scheme for trying calls by hand. There is also a Postman collection in `API/Postman/`.

| Area | Routes |
|---|---|
| Auth | `POST api/authentication/access-token` (rate-limited), `GET api/authentication/verify-token` |
| Employees | `POST create`, `GET get`, `GET all`, `GET export`, `GET insights`, `PATCH update`, `PATCH deactivate`, `PATCH reactivate`, `DELETE delete`, all under `api/employee/` |
| Salary | `GET api/employee/salary-history`, `POST api/employee/salary-history` |
| Audit log | `GET api/employee/audit-log`, `GET api/employee/audit-log/all`, `DELETE api/employee/audit-log/all` (needs the Employer role, `1801`) |
| Org structure | `GET all`, `GET get`, `POST create`, `PATCH update`, `DELETE delete`, `GET employees`, under each of `api/office/`, `api/department/` and `api/cost-center/` |
| Health | `GET /health` |

Every route except login and health requires a bearer token. Errors come back as RFC 9457 Problem Details.

---

## 📝 License & Notes

Personal learning project with no license file. Ask before reusing any of it.

- Logging out only clears the token from browser session storage. There is no server-side token revocation.
- Bulk delete in the UI loops over the single-employee endpoints rather than calling a bulk API.
- More detailed technical notes per layer are in [`ai_docs/`](ai_docs/index.md).
