import { TestBed } from '@angular/core/testing';
import { signal } from '@angular/core';
import { of, throwError } from 'rxjs';
import { CreateEmployeeService } from '../../services/create-employee.service';
import { ApiLoggerService } from '../../services/api-logger.service';
import { NotificationService } from '../../services/notification.service';
import { OfficeService } from '../../services/office.service';
import { DepartmentService } from '../../services/department.service';
import { CostCenterService } from '../../services/cost-center.service';
import { SalaryHistoryService } from '../../services/salary-history.service';
import { AboutComponent } from './about.component';

describe('AboutComponent', () => {
  let createEmployeeSilently: ReturnType<typeof vi.fn>;
  let toggle: ReturnType<typeof vi.fn>;
  let enabled: ReturnType<typeof signal<boolean>>;
  let show: ReturnType<typeof vi.fn>;
  let fetchOfficesOnce: ReturnType<typeof vi.fn>;
  let fetchDepartmentsOnce: ReturnType<typeof vi.fn>;
  let fetchCostCentersOnce: ReturnType<typeof vi.fn>;
  let createSalarySilently: ReturnType<typeof vi.fn>;

  const office = { officeId: 'office-1', name: 'HQ', city: 'Cluj', country: 'Romania' };
  const department = { departmentId: 'department-1', name: 'Engineering' };
  const costCenter = { costCenterId: 'cost-center-1', code: 'CC-1', name: 'Eng' };

  const createComponent = () => {
    createEmployeeSilently = vi.fn().mockReturnValue(of({ status: 200, responseMessage: 'ok', data: 'employee-1' }));
    toggle = vi.fn();
    enabled = signal(true);
    show = vi.fn();
    fetchOfficesOnce = vi.fn().mockReturnValue(of([office]));
    fetchDepartmentsOnce = vi.fn().mockReturnValue(of([department]));
    fetchCostCentersOnce = vi.fn().mockReturnValue(of([costCenter]));
    createSalarySilently = vi.fn().mockReturnValue(of({ status: 200, responseMessage: 'ok' }));

    TestBed.configureTestingModule({
      providers: [
        { provide: CreateEmployeeService, useValue: { createEmployeeSilently } },
        { provide: ApiLoggerService, useValue: { enabled, toggle } },
        { provide: NotificationService, useValue: { show } },
        { provide: OfficeService, useValue: { fetchOfficesOnce } },
        { provide: DepartmentService, useValue: { fetchDepartmentsOnce } },
        { provide: CostCenterService, useValue: { fetchCostCentersOnce } },
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

      expect(show).toHaveBeenCalledWith('Added 49 test employees (1 failed).', 'error');
    });

    it('ignores a second click while a run is in progress', async () => {
      const component = createComponent();

      const first = component.createTestEmployees();
      await component.createTestEmployees(); // re-entrant call
      await first;

      expect(createEmployeeSilently).toHaveBeenCalledTimes(50);
    });

    it('clears the in-progress flag when done', async () => {
      const component = createComponent();

      await component.createTestEmployees();

      expect(component.addingTestEmployees()).toBe(false);
    });

    it('gives every generated employee a hire date between 2018 and 2025', async () => {
      const component = createComponent();

      await component.createTestEmployees();

      const hireDates = createEmployeeSilently.mock.calls.map((c) => c[0].hireDate as string);
      expect(hireDates).toHaveLength(50);
      for (const hireDate of hireDates) {
        expect(hireDate).toMatch(/^\d{4}-\d{2}-\d{2}$/);
        const year = Number(hireDate.slice(0, 4));
        expect(year).toBeGreaterThanOrEqual(2018);
        expect(year).toBeLessThanOrEqual(2025);
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

    it('adds an initial salary entry for every successfully created employee', async () => {
      const component = createComponent();

      await component.createTestEmployees();

      expect(createSalarySilently).toHaveBeenCalledTimes(50);
      const entry = createSalarySilently.mock.calls[0][0];
      expect(entry.employeeId).toBe('employee-1');
      expect(entry.grossSalary).toBeGreaterThanOrEqual(3000);
      expect(entry.grossSalary).toBeLessThanOrEqual(12000);
      expect(entry.effectiveDate).toMatch(/^\d{4}-\d{2}-\d{2}$/);
    });

    it('does not add a salary entry for an employee that failed to register', async () => {
      const component = createComponent();
      let n = 0;
      createEmployeeSilently.mockImplementation(() => {
        if (n++ === 0) return throwError(() => new Error('400'));
        return of({ status: 200, responseMessage: 'ok', data: 'employee-1' });
      });

      await component.createTestEmployees();

      expect(createSalarySilently).toHaveBeenCalledTimes(49);
    });

    it('still reports success even if adding the initial salary fails', async () => {
      const component = createComponent();
      createSalarySilently.mockReturnValue(throwError(() => new Error('500')));

      await component.createTestEmployees();

      expect(show).toHaveBeenCalledWith('Added 50 test employees.', 'success');
    });
  });
});
