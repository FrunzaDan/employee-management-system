import { Component, inject, signal } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { ApiLoggerService } from '../../services/api-logger.service';
import { NotificationService } from '../../services/notification.service';
import { AddEmployeeService } from '../../services/add-employee.service';
import { GetEmployeeService } from '../../services/get-employee.service';
import { OfficeService } from '../../services/office.service';
import { DepartmentService } from '../../services/department.service';
import { CostCenterService } from '../../services/cost-center.service';
import { SalaryHistoryService } from '../../services/salary-history.service';
import { Office } from '../../interfaces/office-response';
import { Department } from '../../interfaces/department-response';
import { CostCenter } from '../../interfaces/cost-center-response';
import {
  Employee,
  EmployeeActivationStatus,
} from '../../interfaces/employee-response';

const TEST_EMPLOYEE_COUNT = 50;

// "Latest test data": hire dates for generated test employees are spread
// across this range, per the About page's bulk generator.
const HIRE_DATE_RANGE_START_YEAR = 2018;
const HIRE_DATE_RANGE_END_YEAR = 2025;

const MIN_BRUTTO_SALARY = 3000;
const MAX_BRUTTO_SALARY = 12000;

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

const COUNTIES_TOWNS: ReadonlyArray<{ county: string; town: string }> = [
  { county: 'Cluj', town: 'Cluj-Napoca' },
  { county: 'Iasi', town: 'Iasi' },
  { county: 'Timis', town: 'Timisoara' },
  { county: 'Brasov', town: 'Brasov' },
  { county: 'Constanta', town: 'Constanta' },
  { county: 'Bihor', town: 'Oradea' },
  { county: 'Sibiu', town: 'Sibiu' },
  { county: 'Dolj', town: 'Craiova' },
  { county: 'Ilfov', town: 'Otopeni' },
  { county: 'Bucuresti', town: 'Bucuresti' },
  { county: 'Arad', town: 'Arad' },
  { county: 'Arges', town: 'Pitesti' },
  { county: 'Bacău', town: 'Bacău' },
  { county: 'Bistrița-Năsăud', town: 'Bistrița' },
  { county: 'Botoșani', town: 'Botoșani' },
  { county: 'Brăila', town: 'Brăila' },
  { county: 'Buzău', town: 'Buzău' },
  { county: 'Caraș-Severin', town: 'Reșița' },
  { county: 'Călărași', town: 'Călărași' },
  { county: 'Covasna', town: 'Sfântu Gheorghe' },
  { county: 'Dâmbovița', town: 'Târgoviște' },
  { county: 'Galați', town: 'Galați' },
  { county: 'Gorj', town: 'Târgu Jiu' },
  { county: 'Hunedoara', town: 'Deva' },
  { county: 'Maramureș', town: 'Baia Mare' },
  { county: 'Mureș', town: 'Târgu Mureș' },
  { county: 'Neamț', town: 'Piatra Neamț' },
  { county: 'Prahova', town: 'Ploiești' },
  { county: 'Suceava', town: 'Suceava' },
  { county: 'Vâlcea', town: 'Râmnicu Vâlcea' },
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

function randomBirthdate(): string {
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

function randomBruttoSalary(): number {
  return Math.floor(Math.random() * (MAX_BRUTTO_SALARY - MIN_BRUTTO_SALARY + 1)) + MIN_BRUTTO_SALARY;
}

// undefined (not '') when the list hasn't loaded/is empty, so the field is
// simply omitted from the create request rather than sent as a bad guid.
function pickGuid(values: ReadonlyArray<{ guid: string }>): string | undefined {
  return values.length > 0 ? pick(values).guid : undefined;
}

@Component({
  selector: 'app-about',
  templateUrl: './about.component.html',
  styleUrls: ['./about.component.css'],
  imports: [],
})
export class AboutComponent {
  private readonly apiLoggerService = inject(ApiLoggerService);
  private readonly notificationService = inject(NotificationService);
  private readonly addEmployeeService = inject(AddEmployeeService);
  private readonly getEmployeeService = inject(GetEmployeeService);
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
      // pick a random, real office/department/cost-center guid — see
      // OfficeService.fetchOfficesOnce.
      const [offices, departments, costCenters] = await Promise.all([
        firstValueFrom(this.officeService.fetchOfficesOnce()),
        firstValueFrom(this.departmentService.fetchDepartmentsOnce()),
        firstValueFrom(this.costCenterService.fetchCostCentersOnce()),
      ]);

      // An index-based suffix (rather than pure randomness) guarantees no
      // email/msisdn collisions within the batch itself, since both columns
      // carry a unique constraint at the database level.
      const employees = Array.from({ length: TEST_EMPLOYEE_COUNT }, (_, index) =>
        this.buildRandomEmployee(index, offices, departments, costCenters),
      );

      let added = 0;
      let failed = 0;
      for (const employee of employees) {
        try {
          await firstValueFrom(
            this.addEmployeeService.addEmployeeSilently(employee),
          );
        } catch {
          failed++;
          continue;
        }
        added++;
        await this.addRandomInitialSalary(employee);
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

  // Best-effort: the employee was already created successfully, so a failure
  // to look up its guid or add its salary shouldn't be reported as a failed
  // employee — same "already succeeded, don't turn a follow-up failure into
  // an error" reasoning as EmployeeAuditLogger.
  private async addRandomInitialSalary(employee: Employee): Promise<void> {
    try {
      const employeeGuid = await firstValueFrom(
        this.getEmployeeService.findEmployeeGuid(employee.email),
      );
      await firstValueFrom(
        this.salaryHistoryService.addSalarySilently({
          employeeGuid,
          bruttoSalary: randomBruttoSalary(),
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
  ): Employee {
    const firstName = pick(FIRST_NAMES);
    const lastName = pick(LAST_NAMES);
    const { county, town } = pick(COUNTIES_TOWNS);
    const suffix = index.toString().padStart(2, '0');

    return {
      firstName,
      lastName,
      email: `${firstName.toLowerCase()}.${lastName.toLowerCase()}${suffix}@example.com`,
      msisdn: `07${randomDigits(6)}${suffix}`,
      gender: Math.floor(Math.random() * 3),
      birthdate: randomBirthdate(),
      employeeStatus: EmployeeActivationStatus.Test,
      address: {
        country: 'Romania',
        county,
        town,
        street: pick(STREETS),
        number: (Math.floor(Math.random() * 150) + 1).toString(),
        zip: randomDigits(6),
      },
      hireDate: randomHireDate(),
      officeGuid: pickGuid(offices),
      departmentGuid: pickGuid(departments),
      costCenterGuid: pickGuid(costCenters),
    } as Employee;
  }
}
