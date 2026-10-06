# API

## What it is

The ASP.NET Core Web API (.NET 10) under `src/API/EmployeeManagementSystemApi/`.

## Key files / paths

- `EmployeeManagementSystem.WebAPI/Program.cs` — options, pipeline, JwtBearer, CORS, rate limiter.
- `WebAPI/Controllers/` — `AuthenticationController`, `EmployeeController`, `OfficeController`, `DepartmentController`, `CostCenterController`, all deriving from `ApiControllerBase`.
- `WebAPI/ErrorHandling/GlobalExceptionHandler.cs` — the one place unhandled exceptions are logged.
- `BusinessLogic/Features/` — one `<Action>Handler` per endpoint, grouped by area:
  - `Employees/` — create, get, get list, export, update, deactivate, reactivate, delete, insights; plus `EmployeeListQuery` (sort/search checks shared by list and export) and `EmployeeCsvExporter`.
  - `Salaries/` — `CreateEmployeeSalaryHandler`, `GetEmployeeSalaryHistoryHandler`.
  - `AuditLog/` — `IEmployeeAuditLogger`/`EmployeeAuditLogger` and the get, get-all and delete-all handlers.
  - `Offices/`, `Departments/`, `CostCenters/` — six handlers each: get list, get, create, update, delete, get employees.
  - `Auth/` — `GetAccessTokenHandler`, `JwtCreation` (also the password and role check), `JwtSigningKey`, `PasswordHasher`.
- `BusinessLogic/Contracts/` — requests (`EmployeeRequests`, `SalaryRequests`, `OfficeRequests`, `DepartmentRequests`, `CostCenterRequests`, `GetEmployeesRequest`, `EmployerCredentials`), `ResponseModel<T>`, `PagedResponse<T>`, `AccessTokenResponse`.
- `BusinessLogic/Abstractions/` — the repository interfaces BusinessLogic needs: `IEmployeeRepository` (+ `EmployeeLookup`), `ISalaryRepository`, `IAuditLogRepository`, `IEmployerRepository` (+ `EmployerAuthData`), `IOfficeRepository`, `IDepartmentRepository`, `ICostCenterRepository`; `DataAccess` implements them.
- `BusinessLogic/Constants/` — `RegexConstants`, `PagingConstants` (max page size).
- `BusinessLogic/Configuration/` — `AuthOptions`.
- `BusinessLogic/Validations/` — email, phone number and address rules.
- `DataAccess/Repositories/` — one class per interface, each owning its parameter builders and row mappers; `EmployeeSummaryResults` holds the employee-summary reader the three org repositories share.
- `DataAccess/DBConnection/` — `SqlConnectionFactory`, `StoredProcedureExecutor` (runs one procedure per call), `StoredProcedureResults` (shared `(Result, Message)` readers), `SqlExtensions`.
- `DataAccess/Configuration/` — `DatabaseOptions`. `DataAccess/DataAccessDependencyInjection.cs` — `AddDataAccess()`.
- Project references (Clean Architecture): `Domain` ← `BusinessLogic` ← `DataAccess`; `WebAPI` → `BusinessLogic` + `DataAccess` (composition root only). BusinessLogic references no ASP.NET or SQL package.
- `Domain/Models/` (read models and enums only), `Domain/Constants/FieldLengthConstants.cs`.
- `Directory.Build.props`, `Directory.Packages.props` — shared settings and central package versions.
- `EmployeeManagementSystem.Tests/` — xUnit v3 tests.

## How it works

### Pipeline (`Program.cs`, in order)

1. `UseHttpLogging`
2. `UseExceptionHandler`
3. `UseStatusCodePages`
4. `Cache-Control: no-store`
5. CORS
6. HTTPS redirection
7. `UseRateLimiter`
8. authentication
9. authorization
10. `/health`
11. controllers

Other settings:
- **CORS:** only `Cors:AllowedOrigins`, which is the UI on port 4205.
- **OpenAPI and Swagger UI:** at `/openapi/v1.json` and `/swagger`, Development only.
- **Rate limit:** the `"login"` policy allows 5 requests per minute per IP, and is applied only to `POST /access-token`.
- **URLs:** kebab-case, through `KebabCaseParameterTransformer` (`CostCenterController` → `/api/cost-center`).

### Configuration (options pattern)

- Two sections are bound, each with `AddOptions<T>().BindConfiguration(...).ValidateDataAnnotations().ValidateOnStart()`:
  - `Auth` → `AuthOptions`, with keys `SecureJwtKey` (32+ characters), `JwtIssuer`, `JwtAudience` and `AccessTokenTimeoutMinutes` (1–1440).
  - `ConnectionStrings` → `DatabaseOptions`, with keys `Docker` (required) and `LocalSqlServer` (optional).
- A bad value stops startup with an `OptionsValidationException` naming the key.
- Consumers take `IOptions<T>`.
- Per-machine overrides go in user-secrets or environment variables (`Auth__…`, `ConnectionStrings__…`).

### Database connection

- `SqlConnectionFactory` picks a connection string once per process, on first use:
  - **macOS/Linux:** always `ConnectionStrings:Docker`, the Azure SQL Edge container. SQL Server has no macOS build.
  - **Windows:** it tries Docker with a 3-second probe, then falls back to `ConnectionStrings:LocalSqlServer` (Windows auth).
- The choice is logged as event 3 (Docker) or event 4 (the local fallback, a warning).
- `run.sh` passes `ConnectionStrings__Docker` for the container it started.
- Each call opens its own connection and disposes it. SqlClient pools the physical connections.

### Errors (RFC 9457 Problem Details)

- **Successes** use the `ResponseModel<T>` envelope. **Every error** is Problem Details.
- **`400`:**
  - binding failures come from `[ApiController]`;
  - business-rule failures come from the handlers, and `Reply()` turns them into Problem Details.
- **`401`/`403`:** bad credentials or role, or a missing or expired token.
- **`404`/`409`:** from the proc's `(Result, Message)` row, for example a duplicate email or cost-center code, or an office still assigned to employees.
- **`429`:** the login rate limit.
- **`500`:** anything thrown, handled by `GlobalExceptionHandler`. `detail` is included in Development only. If the client has already aborted the request (a cancelled navigation or a superseded search), the handler logs at Debug and ends with `499` instead.
- There's no try/catch in controllers, services or data access. The one exception is the best-effort audit write.

### Logging

- Built-in `Microsoft.Extensions.Logging`.
- Console format: JSON in `appsettings.json`; `simple` in Development.
- Access log: one line per request with method, path, status and duration. No headers or bodies, and `/health` is excluded.
- App logs are `[LoggerMessage]` methods with shared event ids across the three APIs:
  - 1 = unhandled exception;
  - 2 = audit write failed;
  - 3/4 = which database was chosen;
  - 5 = the client aborted the request (Debug).

### Endpoints

- **`/api/authentication`:**
  - `POST /access-token`, which is rate-limited;
  - `GET /verify-token`, which requires `[Authorize]`.
- **`/api/employee`** (every endpoint requires `[Authorize]`):
  - Employees: `POST /create`, `GET /get?searchTerm=`, `GET /all` (paged, search, sort, optional `officeId`/`departmentId`/`costCenterId` filters), `GET /export` (CSV, same filters), `PATCH /update`, `PATCH /deactivate`, `PATCH /reactivate`, `DELETE /delete`.
  - Audit log: `GET /audit-log?employeeId=`, `GET /audit-log/all`, `DELETE /audit-log/all` (role `1801`).
  - Salary history: `GET /salary-history?employeeId=`, `POST /salary-history`.
  - Charts: `GET /insights` (one anonymous row per employee: status, gender, birth date, hire date, department, office, current salary, salary history).
- **`/api/office`, `/api/department`, `/api/cost-center`:** `GET /all`, `GET /get`, `POST /create`, `PATCH /update`, `DELETE /delete`, `GET /employees?<entity>Id=`.
- **`GET /health`:** liveness only, no auth.
- **Creates:** return the new GUID as `data`. Other mutations return no `data`.

### Naming

- Every async method ends in `Async`, in the handlers and the data layer. Controller actions are the exception, since their routes are explicit.
- **Handlers:** one class per endpoint, named `<Action>Handler` (`CreateEmployeeHandler`, `GetOfficesHandler`, `CreateEmployeeSalaryHandler`, …), with a single `HandleAsync`; the org units use the same granularity as employees. Its constructor takes only the repository it uses (and `IEmployeeAuditLogger` for audited writes). Controllers take the handler as an action parameter with `[FromServices]`; register new handlers in `BusinessLogicDependencyInjection`.
- **Repositories:** one interface per area in `BusinessLogic/Abstractions`, one method per stored procedure, implemented in `DataAccess/Repositories`.
- Collections are returned as `IReadOnlyList<T>`.

### Validation and data types

- **Requests and responses are separate models.**
  - Requests are all-nullable, and the handlers return a `400` per field.
  - Responses are `sealed record`s with `required` members.
- **Types:**
  - IDs are `Guid`, except `EmployeeSalaryId`, which is an `int`.
  - `BirthDate`, `HireDate` and `EffectiveDate` are `DateOnly`.
  - Timestamps are UTC `DateTime` (serialized with a trailing `Z`).
- **Codes are enums:**
  - `Gender`, `EmployeeStatus` and `EmployerRole` serialize as numbers.
  - `AuditAction` serializes by name (including `SalaryChanged`).
- **Salary:** must be greater than 0, at most `DECIMAL(12,2)`, with 2 decimals at most.
- **Postal code:** ASCII only, because the column is `VARCHAR`.
- **Org lists:** `EmployeeCount` and `TotalGrossSalary` are nullable, because only the list procs compute them.
- **Lengths:** `FieldLengthConstants` mirrors the column lengths.
- **SQL parameters are typed.** `AddWithValue` is never used.
- **JSON:** System.Text.Json, camelCase, nulls written as `null`.

### Auth

- **Login:** `IEmployerRepository.GetEmployerAuthDataAsync` reads the hash, salt and role (`Employer_GetAuthData`); `JwtCreation` verifies PBKDF2-SHA256 with `PasswordHasher`: 100k iterations, a 16-byte salt, and `FixedTimeEquals`. An unknown username is hashed against a dummy salt, so it takes as long as a wrong password. Only a successful login updates `LastInteractionAt` (`Employer_RecordLogin`).
- **Token:** `JwtCreation` signs an HMAC-SHA256 JWT with these claims: `sub` and `unique_name` (both the username), `role`, `amr` (`pwd`), `jti` and `iat`. The expiry comes from `AccessTokenTimeoutMinutes`.
- **Validation:** only `AddJwtBearer`, which checks the signature, issuer, audience and lifetime with `ClockSkew = 0`.

### Tests

- **Setup:**
  - `xunit.v3.mtp-v2` on Microsoft Testing Platform, selected by the repo-root `global.json`.
  - `Moq` for mocks and `FakeLogger` for log assertions.
  - `dotnet test --coverage` for coverage.
- **Covered:** validations, every employee, salary and audit-log handler (`Tests/Features/<Area>/<Handler>Tests.cs`), the 18 org handlers in one table-driven `Features/OrgHandlersTests`, `JwtCreation`, `PasswordHasher` and `SqlConnectionFactory` (with a faked probe).
- **In-memory pipeline tests** (`WebApplicationFactory`):
  - `ErrorResponseTests` and `GlobalExceptionHandlerTests`;
  - `StartupValidationTests`;
  - `Security/EndpointAuthorizationTests`: every `api/` route in the live route table must answer 401 without a token (only the login is allow-listed), and `DELETE audit-log/all` needs role `1801`. Tokens are minted in the test with the same signing key. Use an `https://localhost` client, because following the HTTPS redirect drops the `Authorization` header.
  - `Endpoints/EmployeeEndpointTests`, `OrgEndpointTests` (office, department, cost center) and `AuthenticationEndpointTests`: every endpoint called the way the UI calls it (URL, method, query/body), through the real controller and handler, with only the seven repositories replaced by Moqs (`Endpoints/ApiHost`: `Employees`, `Salaries`, `AuditLog`, `Employers`, `Offices`, `Departments`, `CostCenters`). Each test checks the call that reaches the repository, the response (the `ResponseModel` envelope or a Problem Details error) and the audit entry with the signed-in user.
- **Architecture:** `Architecture/LayerDependencyTests` (NetArchTest): Domain references no other layer, ASP.NET Core or SqlClient; BusinessLogic references neither DataAccess, WebAPI, ASP.NET Core nor SqlClient; DataAccess references neither WebAPI nor ASP.NET Core; controllers reference neither DataAccess nor SqlClient.
- No test needs a database, so the stored procedures and the repositories (parameters and reader mapping) are not covered by any test.

## Gotchas / conventions

- Run `dotnet test` from inside the repo, so the root `global.json` is found.
- `src/DB/EmployeeManagement/global.json` pins the SQL project to the .NET 8 SDK. Don't remove it.
- A regex exists twice, in C# `Validations/` and in Angular `environment.ts`. Change both.
- When changing a column, also change the proc parameter, `FieldLengthConstants` and the typed `SqlParameter`.
- Every new timestamp read goes through `GetUtcDateTime`.
- Office, department and cost-center changes are not audit-logged. Salary changes are.
- Known and accepted limitations:
  - the committed JWT key is a placeholder;
  - there is no token revocation;
  - there is a single role.
