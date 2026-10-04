import { Component, inject, signal } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { ApiLoggerService } from '../../services/api-logger.service';
import { NotificationService } from '../../services/notification.service';
import { EmployeeService } from '../../services/employee.service';
import { OfficeService } from '../../services/office.service';
import { DepartmentService } from '../../services/department.service';
import { CostCenterService } from '../../services/cost-center.service';
import { SalaryHistoryService } from '../../services/salary-history.service';
import { Office } from '../../interfaces/office';
import { Department } from '../../interfaces/department';
import { CostCenter } from '../../interfaces/cost-center';
import {
  CreateEmployeeRequest,
  EmployeeStatus,
} from '../../interfaces/employee';
import {
  buildRandomEmployee,
  pick,
  randomInt,
  toIsoDate,
} from '../../utils/random-employee';

const TEST_EMPLOYEE_COUNT = 50;

const HIRE_DATE_RANGE_START = new Date(2005, 0, 1);
const HIRE_DATE_RANGE_END = new Date(2025, 11, 1);

const MIN_STARTING_SALARY = 3000;
const MAX_STARTING_SALARY = 9000;
const MIN_SALARY_ENTRIES = 2;
const MAX_SALARY_ENTRIES = 6;
const MIN_RAISE = 0.03;
const MAX_RAISE = 0.15;

function roundToTen(value: number): number {
  return Math.round(value / 10) * 10;
}

// A starting salary on the hire date, then raises on random later dates up to
// today, so every entry has already taken effect.
export function randomSalaryHistory(
  hireDate: string,
  today: Date,
): { grossSalary: number; effectiveDate: string }[] {
  const start = new Date(`${hireDate}T00:00:00`).getTime() + 86_400_000;
  const end = today.getTime();
  const raiseCount = randomInt(MIN_SALARY_ENTRIES, MAX_SALARY_ENTRIES) - 1;
  const raiseDates = [
    ...new Set(
      Array.from({ length: raiseCount }, () =>
        toIsoDate(new Date(start + Math.random() * Math.max(end - start, 0))),
      ),
    ),
  ]
    .filter((date) => date > hireDate)
    .sort();

  let grossSalary = roundToTen(
    randomInt(MIN_STARTING_SALARY, MAX_STARTING_SALARY),
  );
  const history = [{ grossSalary, effectiveDate: hireDate }];
  for (const effectiveDate of raiseDates) {
    const raise = MIN_RAISE + Math.random() * (MAX_RAISE - MIN_RAISE);
    grossSalary = roundToTen(grossSalary * (1 + raise));
    history.push({ grossSalary, effectiveDate });
  }
  return history;
}

function pickId<T>(
  values: readonly T[],
  idOf: (value: T) => string,
): string | undefined {
  return values.length > 0 ? idOf(pick(values)) : undefined;
}

@Component({
  selector: 'app-about',
  templateUrl: './about.component.html',
  styleUrl: './about.component.css',
  imports: [],
})
export class AboutComponent {
  private readonly apiLoggerService = inject(ApiLoggerService);
  private readonly notificationService = inject(NotificationService);
  private readonly employeeService = inject(EmployeeService);
  private readonly officeService = inject(OfficeService);
  private readonly departmentService = inject(DepartmentService);
  private readonly costCenterService = inject(CostCenterService);
  private readonly salaryHistoryService = inject(SalaryHistoryService);

  readonly apiLoggingEnabled = this.apiLoggerService.enabled;
  readonly addingTestEmployees = signal(false);

  toggleApiLogging(): void {
    this.apiLoggerService.toggle();
    this.notificationService.show(
      `API call logging turned ${this.apiLoggingEnabled() ? 'on' : 'off'}.`,
    );
  }

  async createTestEmployees(): Promise<void> {
    if (this.addingTestEmployees()) {
      return;
    }
    this.addingTestEmployees.set(true);

    try {
      const [offices, departments, costCenters] = await Promise.all([
        firstValueFrom(this.officeService.fetchOffices()),
        firstValueFrom(this.departmentService.fetchDepartments()),
        firstValueFrom(this.costCenterService.fetchCostCenters()),
      ]);

      const taken = new Set<string>();
      const employees = Array.from({ length: TEST_EMPLOYEE_COUNT }, () =>
        this.buildRandomEmployee(taken, offices, departments, costCenters),
      );

      let added = 0;
      let failed = 0;
      for (const employee of employees) {
        let employeeId: string | null;
        try {
          const response = await firstValueFrom(
            this.employeeService.createEmployeeSilently(employee),
          );
          employeeId = response.data;
        } catch {
          failed++;
          continue;
        }
        added++;
        if (employeeId)
          await this.createRandomSalaryHistory(employeeId, employee);
      }

      const problems = failed > 0 ? [`${failed} failed`] : [];
      this.notificationService.show(
        `Added ${added} test employees` +
          (problems.length > 0 ? ` (${problems.join('; ')}).` : '.'),
        problems.length > 0 ? 'error' : 'success',
      );
    } finally {
      this.addingTestEmployees.set(false);
    }
  }

  private async createRandomSalaryHistory(
    employeeId: string,
    employee: CreateEmployeeRequest,
  ): Promise<void> {
    const history = randomSalaryHistory(employee.hireDate!, new Date());
    await Promise.all(
      history.map(async (entry) => {
        try {
          await firstValueFrom(
            this.salaryHistoryService.createSalarySilently({
              employeeId,
              ...entry,
            }),
          );
        } catch {
          // Sample data: an entry that fails to save is just left out.
        }
      }),
    );
  }

  private buildRandomEmployee(
    taken: Set<string>,
    offices: Office[],
    departments: Department[],
    costCenters: CostCenter[],
  ): CreateEmployeeRequest {
    return {
      ...buildRandomEmployee(HIRE_DATE_RANGE_START, HIRE_DATE_RANGE_END, taken),
      status: EmployeeStatus.Test,
      officeId: pickId(offices, (office) => office.officeId),
      departmentId: pickId(
        departments,
        (department) => department.departmentId,
      ),
      costCenterId: pickId(
        costCenters,
        (costCenter) => costCenter.costCenterId,
      ),
    };
  }
}
