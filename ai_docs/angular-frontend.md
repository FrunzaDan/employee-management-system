# Angular Frontend

## What it is

The Angular 22 app under `UI/`. It is zoneless, uses standalone components and signals, and renders with SSR.

## Key files / paths

- `src/environments/environment.ts` — `apiUrl` (`https://localhost:7146`) plus the email, phone number and username regexes.
- `src/app/app.config.ts`, `app.routes.ts`, `app.ts` (the shell).
- `src/app/services/`:
  - `employee`, `salary-history`, `office`, `department`, `cost-center`, `audit-log`, `global-audit-log`;
  - the auth pieces (`auth.guard`, `verify-token`, `auth-token.interceptor`, `auth-error.interceptor`, `session-storage`);
  - the shared helpers (`notification`, `confirm-dialog`, `api-logger`, `health`, `unsaved-changes.guard`).
- `src/app/components/` — one folder per page or widget. `organization/` holds the admin and details pages for offices, departments and cost centers.
- `src/app/interfaces/` — mirrors of the API's JSON.
- `src/app/utils/extract-error-message.ts`, `audit-action-label.ts`, `employee-status-label.ts`, `chart-scale.ts`, `chart-stats.ts`, `chart-geometry.ts` (chart helpers, shared with the customer and Imalo apps).
- `src/app/pipes/ron.pipe.ts`, `src/styles.css`.

## How it works

### Config

- `app.config.ts` sets up:
  - `provideZonelessChangeDetection()`;
  - the router, with component input binding, view transitions and `canceledNavigationResolution: 'computed'`;
  - hydration with event replay;
  - `provideHttpClient(withFetch(), withInterceptors([apiLoggerInterceptor, authTokenInterceptor, authErrorInterceptor]))`.
- Every route is lazy (`loadComponent`) and has a `title`. `AppTitleStrategy` appends " · Employee Management System".

### Auth

- **Login:** `POST /access-token`. The token goes into `sessionStorage`, and the app navigates to `/employees`.
- **Adding the token:** `authTokenInterceptor` adds the bearer token only to requests whose URL starts with `apiUrl`.
- **Route guard:** `authGuardFn` calls `GET /verify-token` before every protected route. On failure it clears the session and goes to `/login?sessionExpired=true`.
- **Mid-page 401s:** `authErrorInterceptor` handles a `401` from any other call the same way. The guard and the interceptor are kept separate on purpose.
- **Logout:** clears the session explicitly before navigating.

### Routes

| Route | Component |
|---|---|
| `/login` | `user-login` |
| `/employees` (`/` redirects here) | `home` → `employee-list` |
| `/employees/:employeeId` | `employee-details` (record, job info, salary history, audit trail) |
| `/create-employee`, `/employees/update/:employeeId` | `create-employee`, `update-employee` |
| `/offices`, `/departments`, `/cost-centers` | admin pages: a table with one inline add/edit form and "Quickly view employees", which expands the shared `employee-list` filtered to that row |
| `/offices/:officeId`, `/departments/:departmentId`, `/cost-centers/:costCenterId` | details page with a full employees table |
| `/audit-log` | `global-audit-log` |
| `/charts` | `charts` |
| `/about` | `about` (API-logging toggle, test-employee generator) |
| `**` | `page-not-found` |

- Route params bind to signal inputs (`employeeId = input<string>()`).

### Data loading

- **Every read is a resource:** `httpResource` in services, and `rxResource` where a page combines several Observables. `subscribe()` is only for one-off actions such as create, update, delete and export.
- **`EmployeeService`:**
  - **List:** `loadEmployees(params)` sets a params signal, and `employeesResource` refetches on each change. A newer request cancels the older one.
  - **While loading:** a `linkedSignal` keeps the last page on screen during a load and after a failed one.
  - **One employee:** `getEmployee(id)` returns an Observable. `employee-details` and `update-employee` each key an `rxResource` on the route id; the details page reloads it after a deactivate, a reactivate or a new salary entry.
  - **After a change:** an update, status change or delete is written straight into the loaded list, with no refetch.
- **Office, department and cost-center services:**
  - the list is an `httpResource`, reloaded after each mutation;
  - `getEmployees(id)` and `fetchX()` are one-off Observables;
  - the details pages use two `rxResource`s keyed on the route id.
- **Retries:** deactivate and reactivate retry only on status 0 or ≥500, with backoff. After a reactivate, the list re-reads that row from the server, because the restored status may be Test rather than Active.
- **Employee list:**
  - search (debounced 300 ms), sort and paging all happen on the server;
  - page size is 50;
  - bulk actions deactivate the active employees and delete the rest, one call each;
  - reused, filtered, on the org admin and details pages through the `officeId`, `departmentId` and `costCenterId` inputs (plus `emptyMessage`). It lists `EmployeeService` in its own `providers`, so every list has its own page state.
- **CSV export:** uses the list's current search and sort, requests a `blob` and downloads it client-side.
- **Health banner:** `HealthService` polls `/health` every 15 s, in the browser only. `App` shows an "API is not running" card when it fails.
### Charts (`/charts`)

- Same building blocks as the customer app: hand-built inline SVG, no chart library, pure transforms in `charts/charts-data.ts` and the shared `utils/chart-stats.ts` (both with specs).
- **Data:** `EmployeeInsightsService` loads `GET /api/employee/insights`, one anonymous row per employee (status, gender, birth date, hire date, department, office, current salary, and `salaryHistory`: the salary entries in effect so far, oldest first). `loadInsights()` reloads on every visit.
- **Who counts:** status is charted for everyone. Hires over time and raises per year also include deactivated employees. Every other chart and KPI uses the **current workforce** (not deactivated). Missing departments and offices show as "Unassigned"; employees without a salary are left out of the pay charts, and the payroll KPI says how many.
- **Layout:** an "At a glance" band (headcount with a sparkline, monthly payroll, median salary with a year-by-year sparkline, median raise with the count in the past year, average tenure, average age), then "Workforce" (hires over time with a monthly/yearly toggle, "how today's team was built", status and gender donuts, age groups, years of service, headcount by department and office) and "Pay" (salary histogram, payroll share by department, average salary by department and by gender with a pay-gap sentence, pay vs. years of service) and "Salary growth" (median salary year by year, raises per year, yearly pay growth by department).
- **Salary growth:** a raise is any entry higher than the one before it; cuts are ignored. "Median salary, year by year" uses the salary in effect on 31 December of each year (today for this year). Yearly pay growth is compound growth from the first salary to today, averaged per department; employees never raised count as 0%, and anyone with under a year since their first salary is left out.
- **Pay vs. years of service** (`scatter-chart`, employee only): one dot per salaried employee with a hire date, coloured by department. The legend chips toggle departments, and the dashed least-squares trend line is re-fitted to what's visible and summarised in words. The axes run to the first round step above the data (`axisScale`).
- Components, colours, tooltips and motion are the same as in the customer app; see its `angular-frontend.md` "Charts" section.

- **Test-employee generator** (About page): creates 50 employees with random job info. For each one it takes the returned `employeeId` and silently adds a salary history of 2–6 entries (`randomSalaryHistory`): a starting salary of 3,000–9,000 RON on the hire date, then raises of 3–15% on random dates up to today, so all of them have taken effect. A failed salary entry is ignored.

### Forms (Signal Forms)

- `form()`, `[formField]` and `[formRoot]` with `submission: { action, onInvalid }`. There's no `FormGroup` or `ngModel`.
- `employee-form-fields/employee-form.ts` holds the model, the schema and the mappers.
- The "Job information" card (hire date, office, department, cost center) is required. `EmployeeFormFieldsComponent` loads its selects.
- `update-employee`'s model is a `linkedSignal` from the employee its `rxResource` loads.
- **Unsaved changes:** `unsavedChangesGuard` plus `beforeunload`. "Dirty" means the values differ from the baseline.

### Naming (same in all three apps)

- **Page state:** `loading` and `loadError` for the data the page itself loads. Actions get their own: `saveError`, `deleteError`, `loginError`.
- **Service verbs:** `loadX()` starts a resource the service holds and returns nothing. `getX()` and `fetchX()` return an Observable. Writes are `createX`, `updateX` and `deleteX`, plus `…Silently` variants.
- **Service fields:** private resources end in `Resource`, and the base URL field is `apiUrl`.
- **Lists:** `sortColumn` and `sortDirection` (`'asc' | 'desc'`), with `SORT_LABELS` for the table caption. Bulk selection uses `selectedXIds`, `isSelected`, `toggleSelection`, `allSelected`, `toggleSelectAll` and `bulkActionInProgress`.
- **Status labels:** `employeeStatusLabel()` in `utils/employee-status-label.ts` is the only place a status code becomes text.
- **Page titles and buttons:** "Add employee" and "Edit employee"; the edit form's button is "Save changes".

### User feedback (same in all three apps)

- **Success:** a toast, fired by the service in `tap`. Bulk callers use the `…Silently` variants and show one summary toast.
- **Failed action:** an inline `role="alert"` next to the control, with text from `extractErrorMessage`, which reads Problem Details (`errors` → `detail` → `title`).
- **Failed page load:** an alert in place of the content.
- **Confirmations:** `ConfirmDialogService.confirm(...)`, never `window.confirm()`.
- **API logging:** `apiLoggerInterceptor` logs API calls to the console in the browser, with password and token redacted. It is on by default in dev mode and can be toggled on the About page.

### Styling and accessibility

- Bootstrap plus `styles.css` tokens (`--spectrumColor1..4`, `--dangerColor1`, and `--chartColor1..8` for charts), and the Jost font.
- Shared classes: `.page`, `.page-header`, `.app-card`, `.table-themed`, `.sort-button`, `.loading-state`, `.empty-state`.
- Table alignment (all three apps): text columns left, counts and money right (`text-end`), actions right; only checkbox and status-badge columns are centered. Names, phone numbers and dates get `text-nowrap`, so rows stay one line when the table scrolls sideways.
- Motion (same in all three apps; tokens `--duration-*` and `--ease-*`, rules in the Motion section of `styles.css`):
  - cards (`.app-card`) rise in on appearance; sibling cards follow a beat apart;
  - table body rows carry `animate.enter="row-enter"` and `[style.--row-index]="$index"`, so added rows fade in staggered and re-sorted rows keep still;
  - hovered rows, and rows marked `is-selected` for a bulk action, show an accent bar on their left edge;
  - table links grow slightly under the pointer and press in on click; sort buttons and checkboxes press in, and the sort arrow (one `▲`, turned by `.sort-indicator--desc`) pops in and flips;
  - loading placeholders fade in after 0.2 s, so a fast load never flashes one;
  - hover transforms sit in `@media (hover: hover)`, so a tap on a phone does not leave them stuck.
- Display formats: text is sentence case, money uses the `ron` pipe, dates use `longDate`, timestamps use `medium`.
- Accessibility (WCAG 2.2 AA):
  - one `<h1>` per page, focused after each navigation;
  - real links and buttons;
  - sort buttons inside `<th aria-sort>`;
  - `prefers-reduced-motion` is respected.

### Tests

- Vitest through `@angular/build:unit-test`, using the default `ng new` setup.
- **HTTP:** `HttpTestingController`. Resource specs call `TestBed.tick()` to send the request, then `await ApplicationRef.whenStable()` after `flush()`.
- **Components:**
  - use `fixture.componentRef.setInput(...)`;
  - specs that render `EmployeeFormFieldsComponent` must stub the office, department and cost-center services.

## Gotchas / conventions

- The app is zoneless, so any state the template reads must be a signal.
- `linkedSignal` is lazy: it only remembers a page that something has read.
- `value()` throws while a resource is in error. Guard reads with `hasValue()`.
- SSR runs HTTP through Node's `fetch`, which has its own TLS trust. See [build-and-run](build-and-run.md).
- The shared files (`notification`, `confirm-dialog`, `api-logger`, `extract-error-message`, `ron.pipe`, `audit-action-label`) are identical in all three apps. Change them together. `utils/chart-scale.ts` (axis math: `niceMax`, `formatTick`) is identical in all three apps. All three apps also share, file for file, `utils/chart-stats.ts` (banding, month/year series, histogram, ranking), `utils/chart-geometry.ts` (monotone smooth curves), the chart components `time-series-chart`, `donut-chart`, `kpi-tile`, `ranked-bar-chart` and the `.ranked-*` rules in `styles.css`; the customer and employee apps also share `charts.component.css` and the `--chartColor1..8` values. Change them together.
