# API

## What it is

The ASP.NET Core Web API: request pipeline, controllers, request validation, JWT auth, password hashing, and the test setup — everything under `API/`.

## Key files / paths

- `API/.../EmployeeManagementSystem.WebAPI/Program.cs` — host setup, middleware pipeline, `AddJwtBearer` config.
- `API/.../EmployeeManagementSystem.WebAPI/Controllers/AuthenticationController.cs`, `EmployeeController.cs`, `OfficeController.cs`, `DepartmentController.cs`, `CostCenterController.cs`
- `API/.../EmployeeManagementSystem.WebAPI/Routing/KebabCaseParameterTransformer.cs` — turns `[controller]` into the kebab-case URL segment (`CostCenterController` → `/api/cost-center`).
- `API/.../EmployeeManagementSystem.DataAccess/DBConnection/DbHelper.cs` — builds typed `SqlParameter`s and maps stored-proc result sets to typed `ResponseModel<T>`s; `SqlExtensions.cs` holds the typed parameter/reader helpers it uses.
- `API/.../EmployeeManagementSystem.BusinessLogic/EmployeeFunctions/EmployeeCreation.cs`, `EmployeeUpdating.cs`, `EmployeeSalary.cs`; `OrgFunctions/OfficeFunctions.cs`, `DepartmentFunctions.cs`, `CostCenterFunctions.cs`
- `API/.../EmployeeManagementSystem.BusinessLogic/Validations/EmailValidation.cs`, `PhoneNumberValidation.cs`, `AddressValidation.cs`
- `API/.../EmployeeManagementSystem.BusinessLogic/Constants/RegexConstants.cs`, `API/.../EmployeeManagementSystem.Domain/Constants/FieldLengthConstants.cs` — column lengths, shared by BusinessLogic validation and DataAccess parameter sizing
- `API/.../EmployeeManagementSystem.Domain/Models/Enums.cs` — `Gender`, `EmployeeStatus`, `EmployerRole`, `AuditAction`, `EmployeeSortColumn`, `SortDirection`
- `API/.../EmployeeManagementSystem.BusinessLogic/AuthFunctions/JwtCreation.cs`, `JwtSigningKey.cs`
- `API/.../EmployeeManagementSystem.BusinessLogic/Services/Implementation/AuthService.cs`
- `API/.../EmployeeManagementSystem.DataAccess/DBConnection/DbUtils.cs` — `CheckEmployerCredentialsFromDb`
- `API/.../EmployeeManagementSystem.DataAccess/DBConnection/PasswordHasher.cs`
- `global.json` (repo root) — `{ "test": { "runner": "Microsoft.Testing.Platform" } }`
- `API/.../EmployeeManagementSystem.Tests/EmployeeManagementSystem.Tests.csproj`
- `API/.../Directory.Build.props` (shared `net10.0`/nullable/implicit-usings settings, nullable warnings are errors) and `Directory.Packages.props` (Central Package Management — every package version lives there, `.csproj` files have none)
- `API/.../EmployeeManagementSystem.WebAPI/OpenApi/BearerSecuritySchemeTransformer.cs`

## How it works

### Layering & request pipeline

`WebAPI` (controllers/host) → `BusinessLogic` (services, validation, JWT) → `DataAccess` (ADO.NET + stored procs) → `Domain` (models/config). All employee/employer DB access goes through **stored procedures** — no inline SQL, no ORM. See [database](database.md) for the schema/proc side.

**Middleware order in `Program.cs`** (order matters): `UseExceptionHandler` → `Cache-Control: no-store` middleware → `UseCors` → `UseHttpsRedirection` → `UseRateLimiter` → `UseAuthentication` → `UseAuthorization` → `MapHealthChecks("/health")` → `MapControllers`. In non-Development environments, `UseHsts()` runs alongside `UseHttpsRedirection`; in Development, `MapOpenApi()` serves the built-in OpenAPI document at `/openapi/v1.json` and Swagger UI (`Swashbuckle.AspNetCore.SwaggerUI` only — Swashbuckle no longer generates the document) shows it at `/swagger`, with the JWT bearer scheme added by `BearerSecuritySchemeTransformer`.

- `GET /health` is ASP.NET Core's health-check endpoint (`AddHealthChecks()`/`MapHealthChecks`, no DB probe — liveness only; not on a controller, no `[Authorize]`, answers `200` with the plain-text body `Healthy`, not the `ResponseModel` shape), the same mechanism as the sibling apps. It exists so the Angular UI can poll for API liveness and show an "API is not running" banner instead of the app looking broken (see [angular-frontend](angular-frontend.md)); the UI reads it with `responseType: 'text'`. Being unauthenticated is intentional: it needs to answer even when nobody has a token yet.
- Every response is sent with `Cache-Control: no-store` (live, per-user data behind a bearer token — no browser or proxy should keep a copy), the same as the sibling apps.
- **Unhandled exceptions** go to `WebAPI/ErrorHandling/GlobalExceptionHandler.cs` — an `IExceptionHandler` registered with `AddExceptionHandler<GlobalExceptionHandler>()` and run by `UseExceptionHandler()`. It logs the exception once (method + path) and returns the usual `ResponseModel` envelope with a `500`; the exception message is only appended in Development. Controllers have no try/catch blocks. A request the client aborted is answered `499` by `UseExceptionHandler` itself, without reaching the handler. The same mechanism is used by both sibling apps (Imalo writes RFC 9457 Problem Details instead of the envelope, since it's plain REST). Covered by `Tests/ErrorHandling/GlobalExceptionHandlerTests.cs`.
- CORS is locked to `Cors:AllowedOrigins` in `appsettings.json` (`http`/`https` on `localhost:4206` and `localhost:4206` — 4206 is the port `UI/angular.json` actually serves on; an origin missing from this list shows up in the browser as a CORS block on `/health` and the "API is not running" banner even though the API is up), methods limited to `GET/POST/PATCH/DELETE`, headers limited to `Content-Type`/`Authorization`. No `AllowCredentials()` — consistent with bearer-token (not cookie) auth.
- Swagger UI is only wired up in Development, with a Bearer-JWT security scheme so tokens can be pasted in for manual testing.
- `AddRateLimiter` registers one named policy, `"login"`: a per-client-IP fixed-window limiter (5 requests/minute, in-memory), applied via `[EnableRateLimiting("login")]` on just `AuthenticationController.GetAccessToken` — not global, so it never throttles `verify-token` or any `EmployeeController` endpoint. Exceeding it short-circuits with `429` and a `ResponseModel` body written by `options.OnRejected` (`WriteAsJsonAsync`, since this runs before MVC's formatters). In-memory/per-instance, resets on app restart — a deliberate choice for this app's single-instance local/demo scope, not a persistent/distributed solution.

### Controllers

- `AuthenticationController` (`/api/authentication`):
  - `POST /access-token` — body `{ username, password }` → JWT if credentials check out. Rate-limited (see above).
  - `GET /verify-token` — `[Authorize]`-gated; if the request gets past the JWT middleware, the token is valid — the endpoint has nothing left to do but return 200.
- `EmployeeController` (`/api/employee`) — class-level `[Authorize]`, every endpoint requires a bearer token:
  - `POST /create`, `GET /get?searchTerm=...`, `GET /all`, `GET /export`, `GET /audit-log?employeeId=...`, `GET /audit-log/all`, `PATCH /update`, `PATCH /deactivate?employeeId=...`, `PATCH /reactivate?employeeId=...`, `DELETE /delete?employeeId=...`, `DELETE /audit-log/all`, `GET /salary-history?employeeId=...`, `POST /salary-history`.
  - `OfficeController` (`/api/office`), `DepartmentController` (`/api/department`), `CostCenterController` (`/api/cost-center`): `GET /all`, `GET /get?<entity>Id=...`, `POST /create`, `PATCH /update`, `DELETE /delete?<entity>Id=...`, `GET /employees?<entity>Id=...` (see [job-info-and-org-structure](job-info-and-org-structure.md)).
  - `GET /export` is the one endpoint that doesn't return the uniform `ResponseModel` JSON shape below — on success it returns a raw `text/csv` `File` result instead (see [database](database.md)); on failure it still returns `StatusCode(response.Status, response)` like everything else.
  - `DELETE /audit-log/all` permanently wipes **every** row in `EmployeeAuditLog` for **every** employee (`EmployeeAuditLog_DeleteAll` — a plain unconditional `DELETE`, no soft-delete). Wired to a "Clear audit log" button on the global audit log page (`global-audit-log.component.ts`'s `clearAuditLog()`), guarded client-side by a `confirm()` prompt. Carries `[Authorize(Roles = "1801")]` in addition to the class-level `[Authorize]` — a no-op today since `1801` is the only role that exists (see Known gaps below), but it stops a future second role from silently inheriting access to this destructive, untargeted action. The deletion itself is deliberately not audit-logged (no `EmployeeId` to attach it to once the table is wiped).

**Uniform response shape:** every mutating stored proc returns a `(Result INT, Message NVARCHAR)` result set (`Result = 0` means success; nonzero mirrors an HTTP status). `DbHelper.HandleResponseWithMessage` reads that into a `ResponseModel<object>`, and every controller action passes it straight through via `Reply(response)` (`StatusCode(response.Status, response)`) — controllers never branch on status themselves. `ResponseModel<T>.Data` is typed per endpoint (`ResponseModel<EmployeeModel>`, `ResponseModel<PagedResponse<EmployeeModel>>`, ...) and each action is declared `ActionResult<ResponseModel<T>>`, so Swagger shows the real schema. Mutations carry no `Data` (`ResponseModel<object>`, always null) — **except the creates** (`POST /create` and each org entity's `POST /create`), whose `Data` is the new row's ID (`ResponseModel<Guid?>`), since the key is generated by the DB and the client has no other way to learn it. Errors from middleware (the global exception handler, the login rate limiter) and from model binding use the same envelope.

### Data types (DB ↔ C# ↔ JSON)

Same design as the customer app (they were aligned on purpose — see [database](database.md)'s conventions):

- **Serializer: System.Text.Json** (the ASP.NET Core default; camelCase, case-insensitive reads, null properties omitted via `WhenWritingNull`). Newtonsoft.Json was removed.
- **Request vs response models are separate.** Responses (`EmployeeModel`, `SalaryModel`, `OfficeModel`, `AuditLogEntry`, ...) are `sealed record`s with `required` members that mirror the `NOT NULL`-ness of their columns. Requests (`CreateEmployeeRequest`, `UpdateEmployeeRequest`, `CreateSalaryRequest`, `Create/Update<Org>Request`, `AddressRequest`) are all-nullable with no `[Required]`: the business-logic classes validate every field and reply with a field-specific `400`. Create requests have no ID (the DB generates it) and `UpdateEmployeeRequest` has no `Status` (edit is not a lifecycle transition) — the shape can't express the forbidden input. Unknown JSON properties are ignored.
- **The org models' `EmployeeCount`/`TotalGrossSalary` are nullable**: only the list procs compute them, so a single office/department/cost center comes back without them.
- **IDs are `Guid`**, both in models and as `[FromQuery] Guid` parameters (`employeeId`, `officeId`, ...). A malformed value fails model binding; a missing one binds to `Guid.Empty`, which each business-logic method rejects with its own `400`. `EmployeeGetting.DetermineLookup` uses `Guid.TryParse`, which accepts every GUID spelling. `EmployeeSalaryId` is an `int` (an `INT IDENTITY` history row).
- **Dates:** `BirthDate`/`HireDate`/`EffectiveDate` are `DateOnly` (JSON `"1990-01-02"`); timestamps are `DateTime` with `Kind = Utc` (JSON `"2026-09-22T05:47:00Z"`, via `SqlExtensions.GetUtcDateTime`).
- **Codes are enums:** `Gender : byte` and `EmployeeStatus : short` / `EmployerRole : short` serialize as numbers; `AuditAction` serializes and is stored by name (`SalaryChanged`, ...); `EmployeeSortColumn`/`SortDirection` bind from the query string case-insensitively. Numbers that aren't enum members still bind, so each is backstopped with `Enum.IsDefined`.
- **SQL parameters are typed** (`SqlParameterExtensions`: `AddGuid`, `AddNVarChar(size)`, `AddVarChar(size)`, `AddTinyInt`, `AddSmallInt`, `AddDate`, `AddDecimal(12, 2)`), never `AddWithValue`; reads use typed getters, so a schema/mapping mismatch throws instead of being silently converted.

### Validation (before a stored procedure is ever called)

- **`EmployeeCreation`**: rejects missing names, missing/invalid `Email` or `PhoneNumber`, and a missing `Address` or any missing address field (`400` — every `EmployeeAddress` column is `NOT NULL`, so a missing one would otherwise surface as an opaque `500`); the new ID comes back from the DB (see [database](database.md)).
- **`EmployeeUpdating`**: `EmployeeId` is required (non-`Guid.Empty`); `Email`/`PhoneNumber`, if present in the request, are format-validated before the edit is attempted — but unlike registration, they're optional here (partial update). There's no `Status` on `UpdateEmployeeRequest` at all — edit is a partial-update endpoint, not a lifecycle-transition one, so a client can't use it to bypass `DeactivateEmployee`/`ReactivateEmployee`/`DeleteEmployee`'s dedicated audit trail and stored-procedure guardrails.
- **Field length caps**: `FieldLengthConstants` (in `Domain/Constants`, so `DbHelper` sizes each `SqlParameter` from it too) mirrors the `(N)VARCHAR` lengths declared in the DB schema. Both `EmployeeCreation` and `EmployeeUpdating` reject an over-length `FirstName`/`LastName`/`Email`, and `AddressValidation.ValidateLengths` (shared by both) checks every address field — without this, an over-length value would either be silently truncated by SQL Server or surface as an opaque error instead of a clean `400`.
- **Typed request models**: GUIDs are `Guid`, dates `DateOnly`, codes enums (`Gender`, `EmployeeStatus`) — so a malformed value (`"not-a-guid"`, `"2026-02-30"`) is rejected by **model binding** before any controller code runs. `Program.cs` sets `ApiBehaviorOptions.InvalidModelStateResponseFactory` so that rejection still comes back in the uniform `ResponseModel` envelope (`400`, `"Invalid value for '<field>'."`) rather than ASP.NET's `ValidationProblemDetails`. What's left for the business layer: a missing/`Guid.Empty` ID (a missing `?employeeId=` query param binds to `Guid.Empty`), and `Gender` via `Enum.IsDefined` (JSON-to-enum binding accepts *any* integer).
- **Salary**: `EmployeeSalary` rejects amounts `<= 0`, `> 9,999,999,999.99` (the `DECIMAL(12,2)` max — would otherwise be an overflow `500`), or with more than 2 decimals (SQL Server would silently round them).
- **Postal code**: `AddressValidation` allows only ASCII letters/digits/spaces/hyphens, because `PostalCode` is `VARCHAR` (a non-ASCII character would be stored as `?`).
- Email/phone number **uniqueness** is enforced in `Employee_Create` (create) and `Employee_Update` (edit, excluding the row being edited itself) — `400` if either already exists on another employee. This is a DB-layer check, not duplicated in C#.
- The same regexes exist client-side too, in Angular's `environment.ts` (`emailRegex`, `phoneNumberRegex`, `usernameRegex`) — kept in sync **manually**, not shared/generated from one source.
- Validation failures return `400` with a plain message, distinct from the `404`s used for "not found"/"no valid search key" and `409`s used for lifecycle conflicts (see [database](database.md)) — status code choice is meaningful here, not arbitrary.

### JWT auth flow

**Issuing** (`POST /api/authentication/access-token`):

1. `AuthService.GetAccessToken` rejects an empty username/password (`403`), otherwise delegates straight to `JwtCreation.GenerateBearerJwt`.
2. `JwtCreation.GenerateBearerJwt` calls `DbUtils.CheckEmployerCredentialsFromDb`, which fetches the employer's password hash/salt/role via `Employer_GetAuthData` and verifies the password (see Password security below). The success response's `Data` carries the employer's role (an `EmployerRole?`) back up.
3. On success, a JWT is minted: symmetric **HMAC-SHA256** signing (`SymmetricSecurityKey` + `HmacSha256Signature`), key from `Auth:SecureJWTKey` in `appsettings.json` via `JwtSigningKey.Create` (committed value is a demo placeholder — see Known gaps below).
4. **Claims on the token**: `ClaimTypes.Sid` = username, `JwtRegisteredClaimNames.Sub` = username, `ClaimTypes.Name` = username (no separate display name exists yet), `ClaimTypes.Role` = the employer's role fetched in step 2 (empty string if somehow null), `"amr"` = `"pwd"` (authentication-method-reference, OIDC-style — documents _how_ the subject authenticated), `Jti` = random GUID, `Iat` = issue time.
5. Issuer/Audience both `https://localhost:7146/` (demo value), expiry from `Auth:AccessTokenTimeout` (currently `15` minutes).

**Validating** — there is now **one** place tokens are checked, deliberately: `Program.cs`'s `AddJwtBearer` middleware, which runs on every `[Authorize]`-attributed endpoint (all of `EmployeeController`, plus `AuthenticationController.VerifyToken`). It checks signature, issuer, audience, and expiry (`ClockSkew = TimeSpan.Zero`, no grace period), deriving its `IssuerSigningKey` from the same `JwtSigningKey.Create` used at issuing time. `ClaimTypes.Role` is ASP.NET's default role-claim type, so `[Authorize(Roles = "1801")]` works on any endpoint today without touching the JWT pipeline further (used on `DELETE /audit-log/all`, above).

- There used to be a **second**, hand-rolled validation path — a `JwtValidation` class used internally by `AuthService` as a self-check right after minting a token, and an unused `HttpClient` built from an `IHttpClientFactory` that was never actually called with. Both were dead code (request-time auth was always enforced solely by the `AddJwtBearer` middleware) and were deleted; `AuthService` now just checks credentials and returns whatever `JwtCreation` produces (constructor-injected, not manually `new`'d). If you see references to `JwtValidation.cs` elsewhere (older docs, comments), they're stale.
- `JwtCreation.cs` and `Program.cs` used to each independently call `Encoding.ASCII.GetBytes(key)` to derive the signing key — two unsynchronized copies of the same logic. Both now go through the single `JwtSigningKey.Create`. The key is still raw ASCII bytes of the configured string, not base64-decoded, despite `SecureJwtKey` looking like base64 — that behavior itself is unchanged, only the duplication was fixed.
- The role claim only reflects the employer's role **at login time** — the token itself isn't re-checked against the DB later, so a role change mid-session only takes effect on the next login (inherent to stateless JWTs, not a bug).

### Password security

- `PasswordHasher.cs`: **PBKDF2-HMACSHA256**, 100,000 iterations, 16-byte random salt, 32-byte hash (`Rfc2898DeriveBytes.Pbkdf2`). Verification uses `CryptographicOperations.FixedTimeEquals` — a constant-time comparison, not `==`, so timing doesn't leak how many leading bytes matched.
- `Employer` stores `PasswordHash` (BINARY 32, the hash) and `PasswordSalt` (BINARY 16) as **separate columns** — the plaintext password is never stored or logged anywhere.
- `Employer_GetAuthData` fetches the hash+salt+role for a username and bumps `LastInteractionAt`; the actual comparison (`PasswordHasher.VerifyPassword`) happens in **C#, not SQL**.
- The seeded demo login (`TestEmployerID` / `Employer123`, see [build-and-run](build-and-run.md)) has its hash+salt precomputed and hardcoded in the post-deployment script's `BINARY` literals.
- This replaced an earlier **unsalted SHA-256** scheme — if unsalted-hash code or a single hash column reappears, that's a regression, not an alternate valid approach.

### Testing (xUnit v3 on .NET 10 SDK)

- The test project covers `BusinessLogic` (validations including `AddressValidation`, `JwtCreation`, `AuthService`, `EmployeeCreation`/`Updating`/`Getting`/`Activation`/`Deletion`/`Salary`, and the org functions) and `DataAccess`'s `PasswordHasher` — all pure logic, no live DB or Docker needed. This is why `build.sh` runs `dotnet test` _before_ the DB/Docker steps.
- `DbHelper` (stored-proc-result → `ResponseModel` mapping, including the `SqlDataReader`-based row mappers) is **not** unit tested — it takes a concrete `SqlDataReader`, not an interface, so exercising it would need a live connection or a structural change to introduce a mockable seam. Covered indirectly today only by manual testing (Postman/Swagger) and the app actually running.
- Packages: `xunit.v3.mtp-v2` (xUnit v3 on **Microsoft Testing Platform v2**), `Moq` for mocking `IDbUtils`/`IAppSettingsConfig`, and `Microsoft.Testing.Extensions.CodeCoverage` (`dotnet test --coverage`). The VSTest-era packages (`Microsoft.NET.Test.Sdk`, `xunit.runner.visualstudio`, `coverlet.collector`) were removed — under MTP they did nothing. The same setup is used by both sibling apps.
- Tests take the test's cancellation token from `TestContext.Current.CancellationToken` (xUnit v3), never `CancellationToken.None`.
- The **.NET 10 SDK dropped VSTest support for xUnit v3** — `dotnet test` runs MTP only because the repo-root `global.json` says `{ "test": { "runner": "Microsoft.Testing.Platform" } }`.
- That `global.json` is discovered by walking **up from the current working directory** `dotnet` is invoked from — not from the project or `.sln` path — so `dotnet test` has to run from inside the repo (`build.sh` `cd`s to the repo root for its test step).
- `DB/EmployeeManagement/global.json` is a separate one that deliberately pins the SQL project's build to the .NET 8 SDK (kept alongside the Docker SQL Server setup); `Microsoft.Build.Sql` 2.3.0 builds under it. Discovery stops at the nearest `global.json`, so the two don't conflict.
- xUnit v3 test projects compile to a **console executable** (`<OutputType>Exe</OutputType>` in the `.csproj`), not a library — the test assembly is its own runner now, a fundamental v3 architecture change from v2.

## Known gaps / deliberately deferred

Read before assuming something below is an oversight rather than a known, confirmed-with-the-user limitation.

- **`Auth:SecureJWTKey` is a placeholder value** committed in `appsettings.json` — fine for local dev, must be replaced (and pulled out of source control) for any real deployment. Not a live secret to protect; this repo's whole security posture assumes local-only use.
- **No JWT revocation mechanism.** Tokens carry a `Jti` but nothing persists or checks it — a stolen token stays valid until it naturally expires. There's a working client-side logout (see [angular-frontend](angular-frontend.md)), but that's just the client discarding its own copy; a token that already leaked elsewhere (e.g. captured before logout) is still valid until its natural expiry. Deliberate choice, confirmed with the user, over adding server-side revocation (a `Jti` denylist checked per-request) or a refresh-token scheme — both would give up the current fully-stateless design for a bigger architectural change than this app's scope currently justifies.
- **`CheckEmployerCredentialsFromDb`'s role-mismatch branch leaks the employer's numeric role** in its `403` message (`"The provided employer role ({role}) is not valid."`) instead of the generic "Invalid username or password." used for a bad password. Low severity (needs already-valid credentials to trigger). Deliberately left as-is — demo-only messaging, not worth the churn of touching it.
- **Single role in practice**: `1801` is the only `RoleCode` value currently seeded or checked against (`DbUtils.CheckEmployerCredentialsFromDb` checks `== EmployerRole.Employer`). The JWT does carry a `ClaimTypes.Role` claim, so per-endpoint `[Authorize(Roles = ...)]` would work if a second role is ever introduced — nothing needs to change in the token pipeline for that, only the DB check and any new `[Authorize]` attributes.
- Don't treat this list as a TODO to clear autonomously — several of these (placeholder JWT key, single role, role-message leak, no server-side revocation) are appropriate for a local-learning-project scope and were explicitly confirmed as "leave alone."

**Resolved** (kept for history — don't rediscover these as "new" findings):

- `IDbUtils.CheckEmployerCredentialsFromDb` returning `ResponseModel<object>` cast back to `int?` via `credentialsCheck.Data as int?` — fixed: now `ResponseModel<EmployerRole?>`.
- Most of `IDbUtils`/`IEmployeeService` returned `ResponseModel<object>`, `EmployeeModel` doubled as create request, edit request and response (every member nullable), the API used Newtonsoft.Json, and IDs were generated in C# (`SequentialGuid`) — previously left as-is deliberately; fixed at the user's explicit request (2026-09-23) in the data-types pass that aligned this API with the customer app's (see "Data types" above).
- No rate limiting or account lockout on `POST /api/authentication/access-token` — fixed: the `"login"` rate-limiter policy described above.
- `DELETE /api/employee/audit-log/all` was gated only by the controller's class-level `[Authorize]` — fixed: now also carries `[Authorize(Roles = "1801")]`, see Controllers above.
- The JWT signing key was derived independently in two places — fixed via `JwtSigningKey.Create`, see JWT auth flow above.
- `AuthService`/`JwtCreation` used to be manually `new`'d instead of DI-registered — fixed: both are now properly registered in `BusinessLogicDependencyInjection.cs` (`JwtCreation` as a singleton) and constructor-injected.

## Gotchas / conventions

- Don't add try/catch in controllers — the global exception handler is the intended single place for that.
- `DbHelper` reads every column by its exact PascalCase name (`"BirthDate"`, `"StatusCode"`, ...). `SqlDataReader`'s name lookup is case-insensitive, so a casing slip would still work here — keep the names exact anyway, so the code stays correct on a case-sensitive reader/provider.
- If you change a regex, remember there are two copies (C# `Validations/`, Angular `environment.ts`) — nothing enforces they match.
- If you change a DB column's type or length, update three things together: the column, every proc parameter compared with it, and `FieldLengthConstants`/the `SqlParameterExtensions` call — nothing enforces they match, and a type mismatch between parameter and column silently costs index seeks.
- `DbHelper`'s row mappers read nullable columns via `GetNullableString`/`GetNullableGuid`/`GetNullableDateOnly`/`GetNullableDecimal`, and `NOT NULL` ones via the non-nullable getters — keep new mappings consistent with the column's nullability (a real `DBNull` must surface as `null`, never `""`).
- Any new timestamp read must go through `GetUtcDateTime`, or it'll serialize without the `Z` and display in the wrong time zone.
