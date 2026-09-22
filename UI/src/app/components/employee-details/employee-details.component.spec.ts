import { HttpErrorResponse } from '@angular/common/http';
import { signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { Router, provideRouter } from '@angular/router';
import { of, throwError } from 'rxjs';
import {
  Employee,
  EmployeeActivationStatus,
} from '../../interfaces/employee-response';
import { ActivateEmployeeService } from '../../services/activate-employee.service';
import { AuditLogService } from '../../services/audit-log.service';
import { ConfirmDialogService } from '../../services/confirm-dialog.service';
import { DeleteEmployeeService } from '../../services/delete-employee.service';
import { GetEmployeeService } from '../../services/get-employee.service';
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
  let selectedEmployee: ReturnType<typeof signal<Employee | null>>;
  let activationLoading: ReturnType<typeof signal<boolean>>;
  let loadHistory: ReturnType<typeof vi.fn>;

  const buildEmployee = (overrides: Partial<Employee> = {}): Employee => ({
    guid: 'guid-1',
    firstName: 'Dan',
    lastName: 'Frunza',
    msisdn: '123456789',
    email: 'dan@example.com',
    gender: 1,
    employeeStatus: EmployeeActivationStatus.Active,
    creationDate: '2026-01-01',
    interactionDate: '2026-01-01',
    birthdate: '1990-01-01',
    address: {
      country: 'Romania',
      county: 'Cluj',
      town: 'Cluj-Napoca',
      zip: '400000',
      street: 'Main',
      number: '1',
    },
    hireDate: '2020-01-01',
    ...overrides,
  });

  // routeParamId is what withComponentInputBinding() would bind to the `id`
  // input from `?id=`; set it to null before createComponent() for the "no id" case.
  let routeParamId: string | null = 'guid-1';

  const createComponent = (): EmployeeDetailsComponent => {
    getEmployee = vi.fn();
    loadAuditLog = vi.fn();
    deactivateEmployee = vi.fn();
    reactivateEmployee = vi.fn();
    deleteEmployee = vi.fn().mockReturnValue(of({ status: 200, responseMessage: 'ok' }));
    confirm = vi.fn().mockResolvedValue(true);
    navigate = vi.fn().mockResolvedValue(true);
    selectedEmployee = signal<Employee | null>(null);
    activationLoading = signal(false);
    loadHistory = vi.fn();

    // EmployeeDetailsComponent resolves its dependencies via field-initializer
    // inject() calls, so it needs TestBed provider tokens (not positional
    // constructor args) plus an active injection context for the effect()
    // call in its constructor.
    TestBed.configureTestingModule({
      providers: [
        {
          provide: GetEmployeeService,
          useValue: {
            selectedEmployeeSignal: selectedEmployee,
            loadingSignal: signal(false),
            errorSignal: signal<string | null>(null),
            getEmployee,
          },
        },
        {
          provide: ActivateEmployeeService,
          useValue: {
            loadingSignal: activationLoading,
            errorSignal: signal<string | null>(null),
            deactivateEmployee,
            reactivateEmployee,
          },
        },
        { provide: ConfirmDialogService, useValue: { confirm } },
        { provide: DeleteEmployeeService, useValue: { deleteEmployee } },
        {
          provide: AuditLogService,
          useValue: {
            entriesSignal: signal([]),
            loadingSignal: signal(false),
            errorSignal: signal<string | null>(null),
            loadAuditLog,
          },
        },
        {
          provide: SalaryHistoryService,
          useValue: {
            entriesSignal: signal([]),
            loadingSignal: signal(false),
            errorSignal: signal<string | null>(null),
            loadHistory,
            addSalary: vi.fn(),
          },
        },
        provideRouter([]),
      ],
    });

    // RouterLink in the template needs the real Router; only stub navigate().
    TestBed.inject(Router).navigate = navigate as unknown as Router['navigate'];

    const fixture = TestBed.createComponent(EmployeeDetailsComponent);
    if (routeParamId) fixture.componentRef.setInput('id', routeParamId);
    fixture.detectChanges();
    return fixture.componentInstance;
  };

  beforeEach(() => {
    routeParamId = 'guid-1';
  });

  afterEach(() => {
    vi.restoreAllMocks();
  });

  describe('loading by id', () => {
    it('fetches the employee and its audit log using the id input', () => {
      createComponent();

      expect(getEmployee).toHaveBeenCalledWith('guid-1');
      expect(loadAuditLog).toHaveBeenCalledWith('guid-1');
    });

    it('navigates home instead of fetching when there is no id', () => {
      routeParamId = null;
      createComponent();

      expect(getEmployee).not.toHaveBeenCalled();
      expect(loadAuditLog).not.toHaveBeenCalled();
      expect(navigate).toHaveBeenCalledWith(['']);
    });
  });

  describe('computed labels', () => {
    it('employeeGender maps the numeric code to a label', () => {
      const component = createComponent();
      selectedEmployee.set(buildEmployee({ gender: 2 }));

      expect(component.employeeGender()).toBe('female');
    });

    it('employeeStatusLabel maps the status code to a label', () => {
      const component = createComponent();
      selectedEmployee.set(
        buildEmployee({ employeeStatus: EmployeeActivationStatus.Deactivated }),
      );

      expect(component.employeeStatusLabel()).toBe('Deactivated');
    });

    it('both are undefined when no employee is loaded', () => {
      const component = createComponent();

      expect(component.employeeGender()).toBeUndefined();
      expect(component.employeeStatusLabel()).toBeUndefined();
    });
  });

  describe('canDelete', () => {
    it('is false for an Active employee', () => {
      const component = createComponent();
      selectedEmployee.set(
        buildEmployee({ employeeStatus: EmployeeActivationStatus.Active }),
      );

      expect(component.canDelete()).toBe(false);
    });

    it('is true for a Deactivated employee', () => {
      const component = createComponent();
      selectedEmployee.set(
        buildEmployee({ employeeStatus: EmployeeActivationStatus.Deactivated }),
      );

      expect(component.canDelete()).toBe(true);
    });

    it('is true for a Test employee (exempt from the deactivate-first rule)', () => {
      const component = createComponent();
      selectedEmployee.set(
        buildEmployee({ employeeStatus: EmployeeActivationStatus.Test }),
      );

      expect(component.canDelete()).toBe(true);
    });
  });

  describe('deactivateEmployee / reactivateEmployee', () => {
    it('deactivateEmployee asks for confirmation before delegating to the service', async () => {
      const component = createComponent();
      selectedEmployee.set(buildEmployee({ guid: 'guid-1' }));

      await component.deactivateEmployee();

      expect(confirm).toHaveBeenCalled();
      expect(deactivateEmployee).toHaveBeenCalledWith('guid-1');
    });

    it('deactivateEmployee does nothing when the user cancels', async () => {
      const component = createComponent();
      selectedEmployee.set(buildEmployee({ guid: 'guid-1' }));
      confirm.mockResolvedValue(false);

      await component.deactivateEmployee();

      expect(deactivateEmployee).not.toHaveBeenCalled();
    });

    it('reactivateEmployee delegates directly, without a confirmation prompt', () => {
      const component = createComponent();
      selectedEmployee.set(buildEmployee({ guid: 'guid-1' }));

      component.reactivateEmployee();

      expect(confirm).not.toHaveBeenCalled();
      expect(reactivateEmployee).toHaveBeenCalledWith('guid-1');
    });
  });

  describe('deleteEmployee', () => {
    it('does nothing when the user cancels the confirmation', async () => {
      const component = createComponent();
      selectedEmployee.set(buildEmployee({ guid: 'guid-1' }));
      confirm.mockResolvedValue(false);

      await component.deleteEmployee();

      expect(deleteEmployee).not.toHaveBeenCalled();
    });

    it('deletes the employee and navigates back to the list on success', async () => {
      const component = createComponent();
      selectedEmployee.set(buildEmployee({ guid: 'guid-1' }));

      await component.deleteEmployee();

      expect(deleteEmployee).toHaveBeenCalledWith('guid-1');
      expect(navigate).toHaveBeenCalledWith(['/employees']);
    });

    it('surfaces the error and stops loading when the delete request fails', async () => {
      const component = createComponent();
      selectedEmployee.set(buildEmployee({ guid: 'guid-1' }));
      deleteEmployee.mockReturnValue(
        throwError(
          () =>
            new HttpErrorResponse({
              status: 409,
              error: { responseMessage: 'Employee must be deactivated first.' },
            }),
        ),
      );

      await component.deleteEmployee();

      expect(component.deleting()).toBe(false);
      expect(component.deleteError()).toBe('Employee must be deactivated first.');
    });
  });

  describe('audit log reload on activation-loading transition', () => {
    it('reloads the audit log once a deactivate/reactivate call resolves (true -> false)', () => {
      const component = createComponent();
      selectedEmployee.set(buildEmployee({ guid: 'guid-1' }));
      loadAuditLog.mockClear();

      activationLoading.set(true);
      TestBed.flushEffects();
      expect(loadAuditLog).not.toHaveBeenCalled();

      activationLoading.set(false);
      TestBed.flushEffects();

      expect(loadAuditLog).toHaveBeenCalledWith('guid-1');
    });

    it('does not reload on the initial false state (no prior true)', () => {
      const component = createComponent();
      selectedEmployee.set(buildEmployee({ guid: 'guid-1' }));
      loadAuditLog.mockClear();

      TestBed.flushEffects();

      expect(loadAuditLog).not.toHaveBeenCalled();
    });
  });
});
