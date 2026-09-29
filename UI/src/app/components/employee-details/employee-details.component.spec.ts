import { HttpErrorResponse } from '@angular/common/http';
import { WritableSignal, signal } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Router, provideRouter } from '@angular/router';
import { submit } from '@angular/forms/signals';
import { ReplaySubject, of, throwError } from 'rxjs';
import { Employee, EmployeeStatus } from '../../interfaces/employee';
import { EmployeeService } from '../../services/employee.service';
import { AuditLogService } from '../../services/audit-log.service';
import { ConfirmDialogService } from '../../services/confirm-dialog.service';
import { SalaryHistoryService } from '../../services/salary-history.service';
import { AuditLogEntry } from '../../interfaces/audit-log-entry';
import { EmployeeDetailsComponent } from './employee-details.component';

describe('EmployeeDetailsComponent', () => {
  let getEmployee: ReturnType<typeof vi.fn>;
  let bindAuditLog: ReturnType<typeof vi.fn>;
  let reloadAuditLog: ReturnType<typeof vi.fn>;
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
  let bindSalaryHistory: ReturnType<typeof vi.fn>;
  let createSalary: ReturnType<typeof vi.fn>;

  const buildEmployee = (overrides: Partial<Employee> = {}): Employee => ({
    employeeId: 'employeeId-1',
    firstName: 'Dan',
    lastName: 'Frunza',
    phoneNumber: '123456789',
    email: 'dan@example.com',
    gender: 1,
    status: EmployeeStatus.Active,
    accountCreatedAt: '2026-01-01',
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
    bindAuditLog = vi.fn();
    reloadAuditLog = vi.fn();
    deactivateEmployee = vi.fn().mockResolvedValue(true);
    reactivateEmployee = vi.fn().mockResolvedValue(true);
    deleteEmployee = vi
      .fn()
      .mockReturnValue(of({ status: 200, responseMessage: 'ok' }));
    confirm = vi.fn().mockResolvedValue(true);
    navigate = vi.fn().mockResolvedValue(true);
    activationLoading = signal(false);
    bindSalaryHistory = vi.fn();
    createSalary = vi
      .fn()
      .mockReturnValue(of({ status: 200, responseMessage: 'ok' }));

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
            bindAuditLog,
            reloadAuditLog,
          },
        },
        {
          provide: SalaryHistoryService,
          useValue: {
            entries: signal([]),
            loading: signal(false),
            error: signal<string | null>(null),
            bindSalaryHistory,
            createSalary,
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
    it('fetches the employee, and binds its audit log and salary history to the id input', () => {
      const component = createComponent();

      expect(getEmployee).toHaveBeenCalledWith('employeeId-1');
      expect(bindAuditLog).toHaveBeenCalledWith(component.employeeId);
      expect(bindSalaryHistory).toHaveBeenCalledWith(component.employeeId);
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

      await component.reactivateEmployee();

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

  describe('refresh after a status change', () => {
    it('reloads the employee and its audit log once the status change succeeds', async () => {
      const component = createComponent();
      await loadEmployee(buildEmployee({ employeeId: 'employeeId-1' }));

      await component.deactivateEmployee();
      TestBed.tick();

      expect(reloadAuditLog).toHaveBeenCalledTimes(1);
      expect(getEmployee).toHaveBeenCalledTimes(2);
    });

    it('does not reload when the status change fails', async () => {
      const component = createComponent();
      await loadEmployee(buildEmployee({ employeeId: 'employeeId-1' }));
      reactivateEmployee.mockResolvedValue(false);

      await component.reactivateEmployee();
      TestBed.tick();

      expect(reloadAuditLog).not.toHaveBeenCalled();
      expect(getEmployee).toHaveBeenCalledTimes(1);
    });
  });

  describe('salary entry form', () => {
    it('does not submit, and marks the fields, while they are empty', async () => {
      const component = createComponent();
      await loadEmployee(buildEmployee());

      await submit(component.salaryForm);

      expect(createSalary).not.toHaveBeenCalled();
      expect(component.salaryForm.grossSalary().errors()[0].message).toBe(
        'Gross salary is required.',
      );
      expect(component.salaryForm.effectiveDate().touched()).toBe(true);
    });

    it('rejects a gross salary that is not above zero', async () => {
      const component = createComponent();
      await loadEmployee(buildEmployee());
      component.salaryForm.grossSalary().value.set(0);
      component.salaryForm.effectiveDate().value.set('2026-01-01');

      await submit(component.salaryForm);

      expect(createSalary).not.toHaveBeenCalled();
      expect(component.salaryForm.grossSalary().errors()[0].message).toBe(
        'Gross salary must be greater than zero.',
      );
    });

    it('adds the entry, clears the form, and refreshes the employee and audit trail', async () => {
      const component = createComponent();
      await loadEmployee(buildEmployee({ employeeId: 'employeeId-1' }));
      component.salaryForm.grossSalary().value.set(5000);
      component.salaryForm.effectiveDate().value.set('2026-01-01');

      await submit(component.salaryForm);
      TestBed.tick();

      expect(createSalary).toHaveBeenCalledWith({
        employeeId: 'employeeId-1',
        grossSalary: 5000,
        effectiveDate: '2026-01-01',
      });
      expect(component.salaryForm().value()).toEqual({
        grossSalary: null,
        effectiveDate: '',
      });
      expect(component.salaryForm().touched()).toBe(false);
      expect(getEmployee).toHaveBeenCalledTimes(2);
      expect(reloadAuditLog).toHaveBeenCalled();
    });

    it('keeps the values and shows the server message when adding fails', async () => {
      const component = createComponent();
      await loadEmployee(buildEmployee());
      createSalary.mockReturnValue(
        throwError(
          () =>
            new HttpErrorResponse({
              status: 409,
              error: { title: 'Error', detail: 'Duplicate effective date.' },
            }),
        ),
      );
      component.salaryForm.grossSalary().value.set(5000);
      component.salaryForm.effectiveDate().value.set('2026-01-01');

      await submit(component.salaryForm);

      expect(component.createSalaryError()).toBe('Duplicate effective date.');
      expect(component.salaryForm.grossSalary().value()).toBe(5000);
      expect(reloadAuditLog).not.toHaveBeenCalled();
    });
  });

  describe('audit trail preview', () => {
    const buildAuditLog = (count: number): AuditLogEntry[] =>
      Array.from({ length: count }, (_, i) => ({
        employeeAuditLogId: count - i,
        employeeId: 'employee-1',
        performedBy: 'admin',
        actionType: 'Edited',
        details: `change #${count - i}.`,
        occurredAt: '2026-01-01T00:00:00Z',
      }));

    const setAuditLog = (entries: AuditLogEntry[]) =>
      (
        TestBed.inject(AuditLogService).entries as WritableSignal<
          AuditLogEntry[]
        >
      ).set(entries);

    it('shows only the latest 10 actions followed by "…", which reveals the rest', async () => {
      createComponent();
      setAuditLog(buildAuditLog(12));
      await loadEmployee(buildEmployee());

      const items = () =>
        fixture.nativeElement.querySelectorAll('.audit-list li.audit-item');
      const more = () =>
        fixture.nativeElement.querySelector('.audit-more-button');

      expect(items().length).toBe(11);
      expect(more().textContent.trim()).toBe('…');
      expect(more().getAttribute('aria-label')).toBe('Show 2 older actions');
      expect(fixture.nativeElement.textContent).toContain('change #3.');
      expect(fixture.nativeElement.textContent).not.toContain('change #2.');

      more().click();
      await fixture.whenStable();

      expect(items().length).toBe(12);
      expect(more()).toBeNull();
      expect(fixture.nativeElement.textContent).toContain('change #1.');
    });

    it('shows no "…" when there are 10 actions or fewer', async () => {
      createComponent();
      setAuditLog(buildAuditLog(10));
      await loadEmployee(buildEmployee());

      expect(
        fixture.nativeElement.querySelectorAll('.audit-list li.audit-item')
          .length,
      ).toBe(10);
      expect(
        fixture.nativeElement.querySelector('.audit-more-button'),
      ).toBeNull();
    });
  });
});
