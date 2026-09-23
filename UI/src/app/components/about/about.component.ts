import { Component, inject, signal } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { ApiLoggerService } from '../../services/api-logger.service';
import { NotificationService } from '../../services/notification.service';
import { AddEmployeeService } from '../../services/add-employee.service';
import { OfficeService } from '../../services/office.service';
import { DepartmentService } from '../../services/department.service';
import { CostCenterService } from '../../services/cost-center.service';
import { SalaryHistoryService } from '../../services/salary-history.service';
import { Office } from '../../interfaces/office-response';
import { Department } from '../../interfaces/department-response';
import { CostCenter } from '../../interfaces/cost-center-response';
import {
  CreateEmployeeRequest,
  EmployeeStatus,
  Gender,
} from '../../interfaces/employee-response';

const TEST_EMPLOYEE_COUNT = 50;

// "Latest test data": hire dates for generated test employees are spread
// across this range, per the About page's bulk generator.
const HIRE_DATE_RANGE_START_YEAR = 2018;
const HIRE_DATE_RANGE_END_YEAR = 2025;

const MIN_GROSS_SALARY = 3000;
const MAX_GROSS_SALARY = 12000;

const FIRST_NAMES = [
  'Andrei',
  'Maria',
  'Ion',
  'Elena',
  'Mihai',
  'Ioana',
  'Cristian',
  'Ana',
  'Alexandru',
  'Gabriela',
  'Florin',
  'Andreea',
  'Radu',
  'Simona',
  'George',
  'Cristina',
  'Dan',
  'Diana',
  'Vasile',
  'Larisa',
  'Adrian',
  'Mihaela',
  'Bogdan',
  'Roxana',
  'Cătălin',
  'Monica',
  'Ștefan',
  'Alina',
  'Vlad',
  'Nicoleta',
  'Gabriel',
  'Laura',
  'Razvan',
  'Aurelia',
  'Dorin',
  'Camelia',
  'Eugen',
  'Loredana',
  'Sorin',
  'Rodica',
];

const LAST_NAMES = [
  'Popescu',
  'Ionescu',
  'Popa',
  'Radu',
  'Dumitru',
  'Stan',
  'Gheorghe',
  'Constantin',
  'Marin',
  'Stoica',
  'Matei',
  'Ciobanu',
  'Munteanu',
  'Rusu',
  'Barbu',
  'Florea',
  'Nistor',
  'Toma',
  'Oprea',
  'Cristea',
  'Preda',
  'Dobre',
  'Dima',
  'Sârbu',
  'Neagu',
  'Enache',
  'Bălan',
  'Diaconu',
  'Ilie',
  'Lupu',
  'Moldovan',
  'Dragomir',
  'Micu',
  'Nica',
  'Suciu',
  'Voinea',
  'Burlacu',
  'Manole',
  'Pavel',
  'Ungureanu',
];

const COUNTIES_CITIES: ReadonlyArray<{ county: string; city: string }> = [
  { county: 'Cluj', city: 'Cluj-Napoca' },
  { county: 'Iasi', city: 'Iasi' },
  { county: 'Timis', city: 'Timisoara' },
  { county: 'Brasov', city: 'Brasov' },
  { county: 'Constanta', city: 'Constanta' },
  { county: 'Bihor', city: 'Oradea' },
  { county: 'Sibiu', city: 'Sibiu' },
  { county: 'Dolj', city: 'Craiova' },
  { county: 'Ilfov', city: 'Otopeni' },
  { county: 'Bucuresti', city: 'Bucuresti' },
  { county: 'Arad', city: 'Arad' },
  { county: 'Arges', city: 'Pitesti' },
  { county: 'Bacău', city: 'Bacău' },
  { county: 'Bistrița-Năsăud', city: 'Bistrița' },
  { county: 'Botoșani', city: 'Botoșani' },
  { county: 'Brăila', city: 'Brăila' },
  { county: 'Buzău', city: 'Buzău' },
  { county: 'Caraș-Severin', city: 'Reșița' },
  { county: 'Călărași', city: 'Călărași' },
  { county: 'Covasna', city: 'Sfântu Gheorghe' },
  { county: 'Dâmbovița', city: 'Târgoviște' },
  { county: 'Galați', city: 'Galați' },
  { county: 'Gorj', city: 'Târgu Jiu' },
  { county: 'Hunedoara', city: 'Deva' },
  { county: 'Maramureș', city: 'Baia Mare' },
  { county: 'Mureș', city: 'Târgu Mureș' },
  { county: 'Neamț', city: 'Piatra Neamț' },
  { county: 'Prahova', city: 'Ploiești' },
  { county: 'Suceava', city: 'Suceava' },
  { county: 'Vâlcea', city: 'Râmnicu Vâlcea' },
];

const STREETS = [
  'Strada Avram Iancu',
  'Strada Nicolae Bălcescu',
  'Strada 1 Decembrie 1918',
  'Strada Stefan cel Mare',
  'Strada George Coșbuc',
  'Strada Tudor Vladimirescu',
  'Strada Ion Creangă',
  'Strada George Enescu',
  'Strada Horea',
  'Strada Primăverii',
  'Strada Castanilor',
  'Strada Teilor',
  'Strada Stejarului',
  'Strada Livezii',
  'Strada Şcolii',
  'Strada Bisericii',
  'Strada Păcii',
  'Strada Field',
  'Strada Carpați',
  'Strada Crișan',
];

function pick<T>(values: ReadonlyArray<T>): T {
  return values[Math.floor(Math.random() * values.length)];
}

function randomDigits(length: number): string {
  let digits = '';
  for (let i = 0; i < length; i++) {
    digits += Math.floor(Math.random() * 10).toString();
  }
  return digits;
}

function randomBirthDate(): string {
  const start = new Date(1950, 0, 1).getTime();
  const end = new Date(2005, 11, 31).getTime();
  const date = new Date(start + Math.random() * (end - start));
  const month = (date.getMonth() + 1).toString().padStart(2, '0');
  const day = date.getDate().toString().padStart(2, '0');
  return `${date.getFullYear()}-${month}-${day}`;
}

function randomHireDate(): string {
  const start = new Date(HIRE_DATE_RANGE_START_YEAR, 0, 1).getTime();
  const end = new Date(HIRE_DATE_RANGE_END_YEAR, 11, 31).getTime();
  const date = new Date(start + Math.random() * (end - start));
  const month = (date.getMonth() + 1).toString().padStart(2, '0');
  const day = date.getDate().toString().padStart(2, '0');
  return `${date.getFullYear()}-${month}-${day}`;
}

function randomGrossSalary(): number {
  return Math.floor(Math.random() * (MAX_GROSS_SALARY - MIN_GROSS_SALARY + 1)) + MIN_GROSS_SALARY;
}

// undefined (not '') when the list hasn't loaded/is empty, so the field is
// simply omitted from the create request rather than sent as a bad ID.
function pickId<T>(values: readonly T[], idOf: (value: T) => string): string | undefined {
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
  private readonly addEmployeeService = inject(AddEmployeeService);
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

  async addTestEmployees(): Promise<void> {
    if (this.addingTestEmployees()) {
      return;
    }
    this.addingTestEmployees.set(true);

    try {
      // Fetched once up front (not via the services' loadX()/signal state,
      // which is for the admin CRUD pages) so every generated employee can
      // pick a random, real office/department/cost-center employeeId — see
      // OfficeService.fetchOfficesOnce.
      const [offices, departments, costCenters] = await Promise.all([
        firstValueFrom(this.officeService.fetchOfficesOnce()),
        firstValueFrom(this.departmentService.fetchDepartmentsOnce()),
        firstValueFrom(this.costCenterService.fetchCostCentersOnce()),
      ]);

      // An index-based suffix (rather than pure randomness) guarantees no
      // email/phoneNumber collisions within the batch itself, since both columns
      // carry a unique constraint at the database level.
      const employees = Array.from({ length: TEST_EMPLOYEE_COUNT }, (_, index) =>
        this.buildRandomEmployee(index, offices, departments, costCenters),
      );

      let added = 0;
      let failed = 0;
      for (const employee of employees) {
        let employeeId: string | undefined;
        try {
          const response = await firstValueFrom(
            this.addEmployeeService.addEmployeeSilently(employee),
          );
          employeeId = response.data;
        } catch {
          failed++;
          continue;
        }
        added++;
        if (employeeId) await this.addRandomInitialSalary(employeeId, employee);
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

  // Best-effort: the employee was already created successfully (employeeId is the ID its
  // registration returned), so a failure to add its salary shouldn't be reported as a failed
  // employee — same "already succeeded, don't turn a follow-up failure into an error"
  // reasoning as EmployeeAuditLogger.
  private async addRandomInitialSalary(
    employeeId: string,
    employee: CreateEmployeeRequest,
  ): Promise<void> {
    try {
      await firstValueFrom(
        this.salaryHistoryService.addSalarySilently({
          employeeId,
          grossSalary: randomGrossSalary(),
          effectiveDate: employee.hireDate!,
        }),
      );
    } catch {
      // Swallowed — see comment above.
    }
  }

  private buildRandomEmployee(
    index: number,
    offices: Office[],
    departments: Department[],
    costCenters: CostCenter[],
  ): CreateEmployeeRequest {
    const firstName = pick(FIRST_NAMES);
    const lastName = pick(LAST_NAMES);
    const { county, city } = pick(COUNTIES_CITIES);
    const suffix = index.toString().padStart(2, '0');

    return {
      firstName,
      lastName,
      email: `${firstName.toLowerCase()}.${lastName.toLowerCase()}${suffix}@example.com`,
      phoneNumber: `07${randomDigits(6)}${suffix}`,
      gender: pick([Gender.NotDeclared, Gender.Male, Gender.Female]),
      birthDate: randomBirthDate(),
      status: EmployeeStatus.Test,
      address: {
        country: 'Romania',
        county,
        city,
        street: pick(STREETS),
        streetNumber: (Math.floor(Math.random() * 150) + 1).toString(),
        postalCode: randomDigits(6),
      },
      hireDate: randomHireDate(),
      officeId: pickId(offices, (office) => office.officeId),
      departmentId: pickId(departments, (department) => department.departmentId),
      costCenterId: pickId(costCenters, (costCenter) => costCenter.costCenterId),
    };
  }
}
