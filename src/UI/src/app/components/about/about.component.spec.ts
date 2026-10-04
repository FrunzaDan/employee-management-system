import { TestBed } from '@angular/core/testing';
import { signal } from '@angular/core';
import { of, throwError } from 'rxjs';
import { EmployeeService } from '../../services/employee.service';
import { ApiLoggerService } from '../../services/api-logger.service';
import { NotificationService } from '../../services/notification.service';
import { OfficeService } from '../../services/office.service';
import { DepartmentService } from '../../services/department.service';
import { CostCenterService } from '../../services/cost-center.service';
import { SalaryHistoryService } from '../../services/salary-history.service';
import { CreateSalaryRequest } from '../../interfaces/salary';
import { AboutComponent, randomSalaryHistory } from './about.component';

describe('AboutComponent', () => {
  let createEmployeeSilently: ReturnType<typeof vi.fn>;
  let toggle: ReturnType<typeof vi.fn>;
  let enabled: ReturnType<typeof signal<boolean>>;
  let show: ReturnType<typeof vi.fn>;
  let fetchOffices: ReturnType<typeof vi.fn>;
  let fetchDepartments: ReturnType<typeof vi.fn>;
  let fetchCostCenters: ReturnType<typeof vi.fn>;
  let createSalarySilently: ReturnType<typeof vi.fn>;

  const office = {
    officeId: 'office-1',
    name: 'HQ',
    city: 'Cluj',
    country: 'Romania',
  };
  const department = { departmentId: 'department-1', name: 'Engineering' };
  const costCenter = {
    costCenterId: 'cost-center-1',
    code: 'CC-1',
    name: 'Eng',
  };

  const createComponent = () => {
    createEmployeeSilently = vi
      .fn()
      .mockReturnValue(
        of({ status: 200, responseMessage: 'ok', data: 'employee-1' }),
      );
    toggle = vi.fn();
    enabled = signal(true);
    show = vi.fn();
    fetchOffices = vi.fn().mockReturnValue(of([office]));
    fetchDepartments = vi.fn().mockReturnValue(of([department]));
    fetchCostCenters = vi.fn().mockReturnValue(of([costCenter]));
    createSalarySilently = vi
      .fn()
      .mockReturnValue(of({ status: 200, responseMessage: 'ok' }));

    TestBed.configureTestingModule({
      providers: [
        { provide: EmployeeService, useValue: { createEmployeeSilently } },
        { provide: ApiLoggerService, useValue: { enabled, toggle } },
        { provide: NotificationService, useValue: { show } },
        { provide: OfficeService, useValue: { fetchOffices } },
        { provide: DepartmentService, useValue: { fetchDepartments } },
        { provide: CostCenterService, useValue: { fetchCostCenters } },
        { provide: SalaryHistoryService, useValue: { createSalarySilently } },
      ],
    });
    return TestBed.runInInjectionContext(() => new AboutComponent());
  };

  afterEach(() => vi.restoreAllMocks());

  describe('toggleApiLogging', () => {
    it('delegates to the service and shows the resulting state', () => {
      const component = createComponent();

      component.toggleApiLogging();

      expect(toggle).toHaveBeenCalled();
      expect(show).toHaveBeenCalledWith('API call logging turned on.');
    });

    it('reflects "off" when the service reports disabled', () => {
      const component = createComponent();
      enabled.set(false);

      component.toggleApiLogging();

      expect(show).toHaveBeenCalledWith('API call logging turned off.');
    });
  });

  describe('createTestEmployees', () => {
    it('registers 50 test employees and reports success', async () => {
      const component = createComponent();

      await component.createTestEmployees();

      expect(createEmployeeSilently).toHaveBeenCalledTimes(50);
      expect(show).toHaveBeenCalledWith('Added 50 test employees.', 'success');
    });

    it('gives each generated employee a unique email/phoneNumber suffix', async () => {
      const component = createComponent();

      await component.createTestEmployees();

      const emails = createEmployeeSilently.mock.calls.map((c) => c[0].email);
      expect(new Set(emails).size).toBe(50);
    });

    it('counts a failed registration as failed and still reports the rest as added', async () => {
      const component = createComponent();
      let n = 0;
      createEmployeeSilently.mockImplementation(() => {
        if (n++ === 0) return throwError(() => new Error('400'));
        return of({ status: 200, responseMessage: 'ok', data: 'employee-1' });
      });

      await component.createTestEmployees();

      expect(show).toHaveBeenCalledWith(
        'Added 49 test employees (1 failed).',
        'error',
      );
    });

    it('ignores a second click while a run is in progress', async () => {
      const component = createComponent();

      const first = component.createTestEmployees();
      await component.createTestEmployees();
      await first;

      expect(createEmployeeSilently).toHaveBeenCalledTimes(50);
    });

    it('clears the in-progress flag when done', async () => {
      const component = createComponent();

      await component.createTestEmployees();

      expect(component.addingTestEmployees()).toBe(false);
    });

    it('gives every generated employee a hire date between 1 Jan 2005 and 1 Dec 2025', async () => {
      const component = createComponent();

      await component.createTestEmployees();

      const hireDates = createEmployeeSilently.mock.calls.map(
        (c) => c[0].hireDate as string,
      );
      expect(hireDates).toHaveLength(50);
      for (const hireDate of hireDates) {
        expect(hireDate).toMatch(/^\d{4}-\d{2}-\d{2}$/);
        expect(hireDate >= '2005-01-01').toBe(true);
        expect(hireDate <= '2025-12-01').toBe(true);
      }
    });

    it('assigns a random office/department/cost center from the fetched lists', async () => {
      const component = createComponent();

      await component.createTestEmployees();

      const employee = createEmployeeSilently.mock.calls[0][0];
      expect(employee.officeId).toBe(office.officeId);
      expect(employee.departmentId).toBe(department.departmentId);
      expect(employee.costCenterId).toBe(costCenter.costCenterId);
    });

    it('adds a salary history of 2 to 6 entries for every created employee', async () => {
      const component = createComponent();
      let n = 0;
      createEmployeeSilently.mockImplementation(() =>
        of({ status: 200, responseMessage: 'ok', data: `employee-${n++}` }),
      );

      await component.createTestEmployees();

      const hireDates = createEmployeeSilently.mock.calls.map(
        ([employee]) => employee.hireDate,
      );
      const byEmployee = new Map<string, CreateSalaryRequest[]>();
      for (const [entry] of createSalarySilently.mock.calls as [
        CreateSalaryRequest,
      ][]) {
        byEmployee.set(entry.employeeId, [
          ...(byEmployee.get(entry.employeeId) ?? []),
          entry,
        ]);
      }
      expect(byEmployee.size).toBe(50);
      for (const [employeeId, entries] of byEmployee) {
        expect(entries.length).toBeGreaterThanOrEqual(2);
        expect(entries.length).toBeLessThanOrEqual(6);
        const index = Number(employeeId.split('-')[1]);
        expect(entries[0].effectiveDate).toBe(hireDates[index]);
        expect(entries[0].grossSalary).toBeGreaterThanOrEqual(3000);
        expect(entries[0].grossSalary).toBeLessThanOrEqual(9000);
      }
    });

    it('does not add a salary entry for an employee that failed to register', async () => {
      const component = createComponent();
      let n = 0;
      createEmployeeSilently.mockImplementation(() => {
        if (n++ === 0) return throwError(() => new Error('400'));
        return of({
          status: 200,
          responseMessage: 'ok',
          data: `employee-${n}`,
        });
      });

      await component.createTestEmployees();

      const employeeIds = new Set(
        createSalarySilently.mock.calls.map(([entry]) => entry.employeeId),
      );
      expect(employeeIds.size).toBe(49);
    });

    it('still reports success even if adding a salary entry fails', async () => {
      const component = createComponent();
      createSalarySilently.mockReturnValue(throwError(() => new Error('500')));

      await component.createTestEmployees();

      expect(show).toHaveBeenCalledWith('Added 50 test employees.', 'success');
    });
  });

  describe('randomSalaryHistory', () => {
    const today = new Date(2026, 8, 28);

    it('starts on the hire date and only raises pay, never past today', () => {
      for (let i = 0; i < 200; i++) {
        const history = randomSalaryHistory('2025-11-30', today);

        expect(history.length).toBeGreaterThanOrEqual(2);
        expect(history.length).toBeLessThanOrEqual(6);
        expect(history[0].effectiveDate).toBe('2025-11-30');
        for (let j = 1; j < history.length; j++) {
          expect(history[j].effectiveDate > history[j - 1].effectiveDate).toBe(
            true,
          );
          expect(history[j].effectiveDate <= '2026-09-28').toBe(true);
          const raise = history[j].grossSalary / history[j - 1].grossSalary;
          expect(raise).toBeGreaterThan(1);
          expect(raise).toBeLessThan(1.16);
        }
      }
    });
  });
});
