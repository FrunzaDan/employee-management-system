import { TestBed } from '@angular/core/testing';
import { signal } from '@angular/core';
import { of, throwError } from 'rxjs';
import { AddEmployeeService } from '../../services/add-employee.service';
import { ApiLoggerService } from '../../services/api-logger.service';
import { NotificationService } from '../../services/notification.service';
import { GetEmployeeService } from '../../services/get-employee.service';
import { OfficeService } from '../../services/office.service';
import { DepartmentService } from '../../services/department.service';
import { CostCenterService } from '../../services/cost-center.service';
import { SalaryHistoryService } from '../../services/salary-history.service';
import { AboutComponent } from './about.component';

describe('AboutComponent', () => {
  let addEmployeeSilently: ReturnType<typeof vi.fn>;
  let toggle: ReturnType<typeof vi.fn>;
  let enabled: ReturnType<typeof signal<boolean>>;
  let show: ReturnType<typeof vi.fn>;
  let findEmployeeGuid: ReturnType<typeof vi.fn>;
  let fetchOfficesOnce: ReturnType<typeof vi.fn>;
  let fetchDepartmentsOnce: ReturnType<typeof vi.fn>;
  let fetchCostCentersOnce: ReturnType<typeof vi.fn>;
  let addSalarySilently: ReturnType<typeof vi.fn>;

  const office = { guid: 'office-1', officeName: 'HQ', city: 'Cluj', country: 'Romania' };
  const department = { guid: 'department-1', departmentName: 'Engineering' };
  const costCenter = { guid: 'cost-center-1', costCenterCode: 'CC-1', costCenterName: 'Eng' };

  const createComponent = () => {
    addEmployeeSilently = vi.fn().mockReturnValue(of({ status: 200, responseMessage: 'ok' }));
    toggle = vi.fn();
    enabled = signal(true);
    show = vi.fn();
    findEmployeeGuid = vi.fn().mockReturnValue(of('employee-guid'));
    fetchOfficesOnce = vi.fn().mockReturnValue(of([office]));
    fetchDepartmentsOnce = vi.fn().mockReturnValue(of([department]));
    fetchCostCentersOnce = vi.fn().mockReturnValue(of([costCenter]));
    addSalarySilently = vi.fn().mockReturnValue(of({ status: 200, responseMessage: 'ok' }));

    TestBed.configureTestingModule({
      providers: [
        { provide: AddEmployeeService, useValue: { addEmployeeSilently } },
        { provide: ApiLoggerService, useValue: { enabled, toggle } },
        { provide: NotificationService, useValue: { show } },
        { provide: GetEmployeeService, useValue: { findEmployeeGuid } },
        { provide: OfficeService, useValue: { fetchOfficesOnce } },
        { provide: DepartmentService, useValue: { fetchDepartmentsOnce } },
        { provide: CostCenterService, useValue: { fetchCostCentersOnce } },
        { provide: SalaryHistoryService, useValue: { addSalarySilently } },
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

  describe('addTestEmployees', () => {
    it('registers 50 test employees and reports success', async () => {
      const component = createComponent();

      await component.addTestEmployees();

      expect(addEmployeeSilently).toHaveBeenCalledTimes(50);
      expect(show).toHaveBeenCalledWith('Added 50 test employees.', 'success');
    });

    it('gives each generated employee a unique email/msisdn suffix', async () => {
      const component = createComponent();

      await component.addTestEmployees();

      const emails = addEmployeeSilently.mock.calls.map((c) => c[0].email);
      expect(new Set(emails).size).toBe(50);
    });

    it('counts a failed registration as failed and still reports the rest as added', async () => {
      const component = createComponent();
      let n = 0;
      addEmployeeSilently.mockImplementation(() => {
        if (n++ === 0) return throwError(() => new Error('400'));
        return of({ status: 200, responseMessage: 'ok' });
      });

      await component.addTestEmployees();

      expect(show).toHaveBeenCalledWith('Added 49 test employees (1 failed).', 'error');
    });

    it('ignores a second click while a run is in progress', async () => {
      const component = createComponent();

      const first = component.addTestEmployees();
      await component.addTestEmployees(); // re-entrant call
      await first;

      expect(addEmployeeSilently).toHaveBeenCalledTimes(50);
    });

    it('clears the in-progress flag when done', async () => {
      const component = createComponent();

      await component.addTestEmployees();

      expect(component.addingTestEmployees()).toBe(false);
    });

    it('gives every generated employee a hire date between 2018 and 2025', async () => {
      const component = createComponent();

      await component.addTestEmployees();

      const hireDates = addEmployeeSilently.mock.calls.map((c) => c[0].hireDate as string);
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

      await component.addTestEmployees();

      const employee = addEmployeeSilently.mock.calls[0][0];
      expect(employee.officeGuid).toBe(office.guid);
      expect(employee.departmentGuid).toBe(department.guid);
      expect(employee.costCenterGuid).toBe(costCenter.guid);
    });

    it('adds an initial salary entry for every successfully created employee', async () => {
      const component = createComponent();

      await component.addTestEmployees();

      expect(findEmployeeGuid).toHaveBeenCalledTimes(50);
      expect(addSalarySilently).toHaveBeenCalledTimes(50);
      const entry = addSalarySilently.mock.calls[0][0];
      expect(entry.employeeGuid).toBe('employee-guid');
      expect(entry.bruttoSalary).toBeGreaterThanOrEqual(3000);
      expect(entry.bruttoSalary).toBeLessThanOrEqual(12000);
      expect(entry.effectiveDate).toMatch(/^\d{4}-\d{2}-\d{2}$/);
    });

    it('does not add a salary entry for an employee that failed to register', async () => {
      const component = createComponent();
      let n = 0;
      addEmployeeSilently.mockImplementation(() => {
        if (n++ === 0) return throwError(() => new Error('400'));
        return of({ status: 200, responseMessage: 'ok' });
      });

      await component.addTestEmployees();

      expect(addSalarySilently).toHaveBeenCalledTimes(49);
    });

    it('still reports success even if adding the initial salary fails', async () => {
      const component = createComponent();
      addSalarySilently.mockReturnValue(throwError(() => new Error('500')));

      await component.addTestEmployees();

      expect(show).toHaveBeenCalledWith('Added 50 test employees.', 'success');
    });
  });
});
