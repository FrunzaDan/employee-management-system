import { HttpErrorResponse } from '@angular/common/http';
import { signal } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Router, provideRouter } from '@angular/router';
import { ReplaySubject, of, throwError } from 'rxjs';
import { Employee, EmployeeStatus } from '../../interfaces/employee';
import { EmployeeService } from '../../services/employee.service';
import { AuditLogService } from '../../services/audit-log.service';
import { ConfirmDialogService } from '../../services/confirm-dialog.service';
import { SalaryHistoryService } from '../../services/salary-history.service';
import { EmployeeDetailsComponent } from './employee-details.component';

describe('EmployeeDetailsComponent', () => {
  let getEmployee: ReturnType<typeof vi.fn>;
  let loadAuditLog: ReturnType<typeof vi.fn>;
  let deactivateEmployee: ReturnType<typeof vi.fn>;
  let reactivateEmployee: ReturnType<typeof vi.fn>;
  let deleteEmployee: ReturnType<typeof vi.fn>;
  let confirm: ReturnType<typeof vi.fn>;
  let navigate: ReturnType<typeof vi.fn>;
  let employee$: ReplaySubject<Employee>;
  let fixture: ComponentFixture<EmployeeDetailsComponent>;

  const loadEmployee = async (employee: Employee) => {
    employee$.next(employee);
    await fixture.whenStable();
  };
  let activationLoading: ReturnType<typeof signal<boolean>>;
  let loadSalaryHistory: ReturnType<typeof vi.fn>;

  const buildEmployee = (overrides: Partial<Employee> = {}): Employee => ({
    employeeId: 'employeeId-1',
    firstName: 'Dan',
    lastName: 'Frunza',
    phoneNumber: '123456789',
    email: 'dan@example.com',
    gender: 1,
    status: EmployeeStatus.Active,
    createdAt: '2026-01-01',
    lastInteractionAt: '2026-01-01',
    birthDate: '1990-01-01',
    address: {
      country: 'Romania',
      county: 'Cluj',
      city: 'Cluj-Napoca',
      postalCode: '400000',
      street: 'Main',
      streetNumber: '1',
    },
    hireDate: '2020-01-01',
    officeId: null,
    officeName: null,
    departmentId: null,
    departmentName: null,
    costCenterId: null,
    costCenterName: null,
    currentGrossSalary: null,
    ...overrides,
  });

  let routeParamId: string | null = 'employeeId-1';

  const createComponent = (): EmployeeDetailsComponent => {
    employee$ = new ReplaySubject<Employee>(1);
    getEmployee = vi.fn(() => employee$);
    loadAuditLog = vi.fn();
    deactivateEmployee = vi.fn();
    reactivateEmployee = vi.fn();
    deleteEmployee = vi
      .fn()
      .mockReturnValue(of({ status: 200, responseMessage: 'ok' }));
    confirm = vi.fn().mockResolvedValue(true);
    navigate = vi.fn().mockResolvedValue(true);
    activationLoading = signal(false);
    loadSalaryHistory = vi.fn();

    TestBed.configureTestingModule({
      providers: [
        {
          provide: EmployeeService,
          useValue: {
            getEmployee,
            activationLoading,
            activationError: signal<string | null>(null),
            deactivateEmployee,
            reactivateEmployee,
            deleteEmployee,
          },
        },
        { provide: ConfirmDialogService, useValue: { confirm } },
        {
          provide: AuditLogService,
          useValue: {
            entries: signal([]),
            loading: signal(false),
            error: signal<string | null>(null),
            loadAuditLog,
          },
        },
        {
          provide: SalaryHistoryService,
          useValue: {
            entries: signal([]),
            loading: signal(false),
            error: signal<string | null>(null),
            loadSalaryHistory,
            createSalary: vi.fn(),
          },
        },
        provideRouter([]),
      ],
    });

    TestBed.inject(Router).navigate = navigate as unknown as Router['navigate'];

    fixture = TestBed.createComponent(EmployeeDetailsComponent);
    if (routeParamId) fixture.componentRef.setInput('employeeId', routeParamId);
    fixture.detectChanges();
    return fixture.componentInstance;
  };

  beforeEach(() => {
    routeParamId = 'employeeId-1';
  });

  afterEach(() => {
    vi.restoreAllMocks();
  });

  describe('loading by id', () => {
    it('fetches the employee and its audit log using the id input', () => {
      createComponent();

      expect(getEmployee).toHaveBeenCalledWith('employeeId-1');
      expect(loadAuditLog).toHaveBeenCalledWith('employeeId-1');
    });

    it('navigates to the employee list instead of fetching when there is no id', () => {
      routeParamId = null;
      createComponent();

      expect(getEmployee).not.toHaveBeenCalled();
      expect(loadAuditLog).not.toHaveBeenCalled();
      expect(navigate).toHaveBeenCalledWith(['/employees']);
    });
  });

  describe('computed labels', () => {
    it('genderLabel maps the numeric code to a label', async () => {
      const component = createComponent();
      await loadEmployee(buildEmployee({ gender: 2 }));

      expect(component.genderLabel()).toBe('female');
    });

    it('statusLabel maps the status code to a label', async () => {
      const component = createComponent();
      await loadEmployee(buildEmployee({ status: EmployeeStatus.Deactivated }));

      expect(component.statusLabel()).toBe('Deactivated');
    });

    it('both are undefined when no employee is loaded', () => {
      const component = createComponent();

      expect(component.genderLabel()).toBeUndefined();
      expect(component.statusLabel()).toBeUndefined();
    });
  });

  describe('canDelete', () => {
    it('is false for an Active employee', async () => {
      const component = createComponent();
      await loadEmployee(buildEmployee({ status: EmployeeStatus.Active }));

      expect(component.canDelete()).toBe(false);
    });

    it('is true for a Deactivated employee', async () => {
      const component = createComponent();
      await loadEmployee(buildEmployee({ status: EmployeeStatus.Deactivated }));

      expect(component.canDelete()).toBe(true);
    });

    it('is true for a Test employee (exempt from the deactivate-first rule)', async () => {
      const component = createComponent();
      await loadEmployee(buildEmployee({ status: EmployeeStatus.Test }));

      expect(component.canDelete()).toBe(true);
    });
  });

  describe('deactivateEmployee / reactivateEmployee', () => {
    it('deactivateEmployee asks for confirmation before delegating to the service', async () => {
      const component = createComponent();
      await loadEmployee(buildEmployee({ employeeId: 'employeeId-1' }));

      await component.deactivateEmployee();

      expect(confirm).toHaveBeenCalled();
      expect(deactivateEmployee).toHaveBeenCalledWith('employeeId-1');
    });

    it('deactivateEmployee does nothing when the user cancels', async () => {
      const component = createComponent();
      await loadEmployee(buildEmployee({ employeeId: 'employeeId-1' }));
      confirm.mockResolvedValue(false);

      await component.deactivateEmployee();

      expect(deactivateEmployee).not.toHaveBeenCalled();
    });

    it('reactivateEmployee delegates directly, without a confirmation prompt', async () => {
      const component = createComponent();
      await loadEmployee(buildEmployee({ employeeId: 'employeeId-1' }));

      component.reactivateEmployee();

      expect(confirm).not.toHaveBeenCalled();
      expect(reactivateEmployee).toHaveBeenCalledWith('employeeId-1');
    });
  });

  describe('deleteEmployee', () => {
    it('does nothing when the user cancels the confirmation', async () => {
      const component = createComponent();
      await loadEmployee(buildEmployee({ employeeId: 'employeeId-1' }));
      confirm.mockResolvedValue(false);

      await component.deleteEmployee();

      expect(deleteEmployee).not.toHaveBeenCalled();
    });

    it('deletes the employee and navigates back to the list on success', async () => {
      const component = createComponent();
      await loadEmployee(buildEmployee({ employeeId: 'employeeId-1' }));

      await component.deleteEmployee();

      expect(deleteEmployee).toHaveBeenCalledWith('employeeId-1');
      expect(navigate).toHaveBeenCalledWith(['/employees']);
    });

    it('surfaces the error and stops loading when the delete request fails', async () => {
      const component = createComponent();
      await loadEmployee(buildEmployee({ employeeId: 'employeeId-1' }));
      deleteEmployee.mockReturnValue(
        throwError(
          () =>
            new HttpErrorResponse({
              status: 409,
              error: {
                title: 'Error',
                detail: 'Employee must be deactivated first.',
              },
            }),
        ),
      );

      await component.deleteEmployee();

      expect(component.deleting()).toBe(false);
      expect(component.deleteError()).toBe(
        'Employee must be deactivated first.',
      );
    });
  });

  describe('audit log reload on activation-loading transition', () => {
    it('reloads the employee and its audit log once a deactivate/reactivate call resolves (true -> false)', async () => {
      const component = createComponent();
      await loadEmployee(buildEmployee({ employeeId: 'employeeId-1' }));
      loadAuditLog.mockClear();

      activationLoading.set(true);
      TestBed.flushEffects();
      expect(loadAuditLog).not.toHaveBeenCalled();

      activationLoading.set(false);
      TestBed.flushEffects();

      expect(loadAuditLog).toHaveBeenCalledWith('employeeId-1');
      expect(getEmployee).toHaveBeenCalledTimes(2);
    });

    it('does not reload on the initial false state (no prior true)', async () => {
      const component = createComponent();
      await loadEmployee(buildEmployee({ employeeId: 'employeeId-1' }));
      loadAuditLog.mockClear();

      TestBed.flushEffects();

      expect(loadAuditLog).not.toHaveBeenCalled();
    });
  });
});
