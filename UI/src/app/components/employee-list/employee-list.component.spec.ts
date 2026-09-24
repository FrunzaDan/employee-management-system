import { HttpErrorResponse } from '@angular/common/http';
import { signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { of, throwError } from 'rxjs';
import { EmployeeService } from '../../services/employee.service';
import { ConfirmDialogService } from '../../services/confirm-dialog.service';
import { NotificationService } from '../../services/notification.service';
import { Employee, EmployeeStatus } from '../../interfaces/employee';
import { EmployeeListComponent } from './employee-list.component';

describe('EmployeeListComponent', () => {
  let component: EmployeeListComponent;
  let loadEmployees: ReturnType<typeof vi.fn>;
  let exportEmployees: ReturnType<typeof vi.fn>;
  let totalItems: ReturnType<typeof signal<number>>;
  let employees: ReturnType<typeof signal<Employee[]>>;
  let deleteEmployee: ReturnType<typeof vi.fn>;
  let deleteEmployeeSilently: ReturnType<typeof vi.fn>;
  let deactivateEmployeeSilently: ReturnType<typeof vi.fn>;
  let confirm: ReturnType<typeof vi.fn>;
  let notificationShow: ReturnType<typeof vi.fn>;

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
    hireDate: null,
    officeId: null,
    officeName: null,
    departmentId: null,
    departmentName: null,
    costCenterId: null,
    costCenterName: null,
    currentGrossSalary: null,
    ...overrides,
  });

  beforeEach(() => {
    loadEmployees = vi.fn();
    exportEmployees = vi.fn();
    totalItems = signal(0);
    employees = signal<Employee[]>([]);
    deleteEmployee = vi
      .fn()
      .mockReturnValue(of({ status: 200, responseMessage: 'ok' }));
    deleteEmployeeSilently = vi
      .fn()
      .mockReturnValue(of({ status: 200, responseMessage: 'ok' }));
    deactivateEmployeeSilently = vi
      .fn()
      .mockReturnValue(of({ status: 200, responseMessage: 'ok' }));
    confirm = vi.fn().mockResolvedValue(true);
    notificationShow = vi.fn();

    const employeeServiceStub = {
      employees,
      loading: signal(false),
      error: signal<string | null>(null),
      totalItems: totalItems,
      pageNumber: signal(1),
      pageSize: signal(10),
      loadEmployees,
      activationLoading: signal(false),
      activationError: signal<string | null>(null),
      deactivateEmployeeSilently,
      deleteEmployee,
      deleteEmployeeSilently,
      exportLoading: signal(false),
      exportError: signal<string | null>(null),
      exportEmployees,
    };

    TestBed.configureTestingModule({
      providers: [
        { provide: EmployeeService, useValue: employeeServiceStub },
        { provide: ConfirmDialogService, useValue: { confirm } },
        { provide: NotificationService, useValue: { show: notificationShow } },
        { provide: Router, useValue: { navigate: vi.fn() } },
      ],
    });

    component = TestBed.runInInjectionContext(
      () => new EmployeeListComponent(),
    );
  });

  afterEach(() => {
    vi.useRealTimers();
  });

  describe('setSort', () => {
    it('toggles direction when clicking the already-active column, and resets to page 1', () => {
      totalItems.set(75);
      component.currentPage.set(3);
      loadEmployees.mockClear();

      component.setSort('name');

      expect(component.sortColumn()).toBe('name');
      expect(component.sortDirection()).toBe('desc');
      expect(component.currentPage()).toBe(1);
      expect(loadEmployees).toHaveBeenCalledTimes(1);
      expect(loadEmployees).toHaveBeenCalledWith({
        pageNumber: 1,
        pageSize: 50,
        searchTerm: undefined,
        sortColumn: 'name',
        sortDirection: 'desc',
      });
    });

    it('switches column and resets direction to asc when clicking a different column', () => {
      component.setSort('email');

      expect(component.sortColumn()).toBe('email');
      expect(component.sortDirection()).toBe('asc');
      expect(loadEmployees).toHaveBeenLastCalledWith({
        pageNumber: 1,
        pageSize: 50,
        searchTerm: undefined,
        sortColumn: 'email',
        sortDirection: 'asc',
      });
    });
  });

  describe('goToPage', () => {
    it('clamps above the last page down to totalPages', () => {
      totalItems.set(120);
      loadEmployees.mockClear();

      component.goToPage(10);

      expect(component.currentPage()).toBe(3);
      expect(loadEmployees).toHaveBeenLastCalledWith(
        expect.objectContaining({ pageNumber: 3 }),
      );
    });

    it('clamps below page 1 up to 1', () => {
      totalItems.set(75);
      component.currentPage.set(3);
      loadEmployees.mockClear();

      component.goToPage(0);

      expect(component.currentPage()).toBe(1);
      expect(loadEmployees).toHaveBeenLastCalledWith(
        expect.objectContaining({ pageNumber: 1 }),
      );
    });

    it('does nothing when the target page equals the current page', () => {
      loadEmployees.mockClear();

      component.goToPage(1);

      expect(loadEmployees).not.toHaveBeenCalled();
    });
  });

  describe('page clamping after a reload', () => {
    it('steps back to the last page when the current page no longer exists', () => {
      totalItems.set(120);
      component.currentPage.set(3);
      TestBed.tick();
      loadEmployees.mockClear();

      totalItems.set(100);
      TestBed.tick();

      expect(component.currentPage()).toBe(2);
      expect(loadEmployees).toHaveBeenCalledWith(
        expect.objectContaining({ pageNumber: 2 }),
      );
    });
  });

  describe('onSearchInput', () => {
    it('debounces so only the last call within the window triggers a fetch', () => {
      vi.useFakeTimers();

      component.onSearchInput('d');
      vi.advanceTimersByTime(100);
      component.onSearchInput('da');
      vi.advanceTimersByTime(100);
      component.onSearchInput('dan');

      expect(loadEmployees).not.toHaveBeenCalled();

      vi.advanceTimersByTime(299);
      expect(loadEmployees).not.toHaveBeenCalled();

      vi.advanceTimersByTime(1);
      expect(loadEmployees).toHaveBeenCalledTimes(1);
      expect(loadEmployees).toHaveBeenCalledWith({
        pageNumber: 1,
        pageSize: 50,
        searchTerm: 'dan',
        sortColumn: 'name',
        sortDirection: 'asc',
      });
    });

    it('resets to page 1 once the debounced fetch fires', () => {
      vi.useFakeTimers();
      totalItems.set(75);
      component.currentPage.set(2);
      loadEmployees.mockClear();

      component.onSearchInput('dan');
      vi.advanceTimersByTime(300);

      expect(component.currentPage()).toBe(1);
    });
  });

  describe('exportCsv', () => {
    it('exports with the current search term (trimmed) and sort state', () => {
      component.searchTerm.set('  dan  ');
      component.setSort('email');

      component.exportCsv();

      expect(exportEmployees).toHaveBeenCalledWith({
        searchTerm: 'dan',
        sortColumn: 'email',
        sortDirection: 'asc',
      });
    });

    it('omits searchTerm when the search box is blank', () => {
      component.exportCsv();

      expect(exportEmployees).toHaveBeenCalledWith({
        searchTerm: undefined,
        sortColumn: 'name',
        sortDirection: 'asc',
      });
    });
  });

  describe('toggleSelection / toggleSelectAll', () => {
    it('adds a employeeId to selectedEmployeeIds when checked, and removes it when unchecked', () => {
      component.toggleSelection('employeeId-1', true);
      expect(component.isSelected('employeeId-1')).toBe(true);

      component.toggleSelection('employeeId-1', false);
      expect(component.isSelected('employeeId-1')).toBe(false);
    });

    it('allSelected is false when the page is empty', () => {
      employees.set([]);
      expect(component.allSelected()).toBe(false);
    });

    it('toggleSelectAll(true) selects every employee on the current page', () => {
      employees.set([
        buildEmployee({ employeeId: 'g1' }),
        buildEmployee({ employeeId: 'g2' }),
      ]);

      component.toggleSelectAll(true);

      expect(component.isSelected('g1')).toBe(true);
      expect(component.isSelected('g2')).toBe(true);
      expect(component.allSelected()).toBe(true);
    });

    it('toggleSelectAll(false) clears the selection for every employee on the current page', () => {
      employees.set([
        buildEmployee({ employeeId: 'g1' }),
        buildEmployee({ employeeId: 'g2' }),
      ]);
      component.toggleSelectAll(true);

      component.toggleSelectAll(false);

      expect(component.isSelected('g1')).toBe(false);
      expect(component.isSelected('g2')).toBe(false);
      expect(component.allSelected()).toBe(false);
    });
  });

  describe('deleteEmployee', () => {
    it('does nothing when the user cancels the confirmation', async () => {
      confirm.mockResolvedValue(false);

      await component.deleteEmployee('employeeId-1');

      expect(deleteEmployee).not.toHaveBeenCalled();
    });

    it('deletes the employee and refetches the current page on success', async () => {
      loadEmployees.mockClear();

      await component.deleteEmployee('employeeId-1');

      expect(deleteEmployee).toHaveBeenCalledWith('employeeId-1');
      expect(component.deleting()).toBe(false);
      expect(component.deleteError()).toBeNull();
      expect(loadEmployees).toHaveBeenCalledTimes(1);
    });

    it('surfaces the error and stops loading when the delete request fails', async () => {
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

      await component.deleteEmployee('employeeId-1');

      expect(component.deleting()).toBe(false);
      expect(component.deleteError()).toBe(
        'Employee must be deactivated first.',
      );
    });
  });

  describe('bulkDeleteSelected', () => {
    it('does nothing when no rows are selected', async () => {
      await component.bulkDeleteSelected();

      expect(confirm).not.toHaveBeenCalled();
    });

    it('does not call any API when the user cancels the confirmation', async () => {
      employees.set([buildEmployee({ employeeId: 'g1' })]);
      component.toggleSelection('g1', true);
      confirm.mockResolvedValue(false);

      await component.bulkDeleteSelected();

      expect(deactivateEmployeeSilently).not.toHaveBeenCalled();
      expect(deleteEmployeeSilently).not.toHaveBeenCalled();
    });

    it('deactivates Active employees and deletes non-Active ones, then shows a success summary and refetches', async () => {
      employees.set([
        buildEmployee({
          employeeId: 'active-1',
          status: EmployeeStatus.Active,
        }),
        buildEmployee({
          employeeId: 'deactivated-1',
          status: EmployeeStatus.Deactivated,
        }),
        buildEmployee({ employeeId: 'test-1', status: EmployeeStatus.Test }),
      ]);
      component.toggleSelectAll(true);
      loadEmployees.mockClear();

      await component.bulkDeleteSelected();

      expect(confirm).toHaveBeenCalledWith(
        expect.stringContaining('1 is active'),
        expect.objectContaining({ confirmLabel: 'Apply', variant: 'danger' }),
      );
      expect(deactivateEmployeeSilently).toHaveBeenCalledWith('active-1');
      expect(deleteEmployeeSilently).toHaveBeenCalledWith('deactivated-1');
      expect(deleteEmployeeSilently).toHaveBeenCalledWith('test-1');
      expect(notificationShow).toHaveBeenCalledWith(
        'Bulk action completed: 1 deactivated, 2 deleted.',
        'success',
      );
      expect(component.bulkActionInProgress()).toBe(false);
      expect(loadEmployees).toHaveBeenCalledTimes(1);
    });

    it('reports a failure count and does not stop the batch when one operation fails', async () => {
      employees.set([
        buildEmployee({
          employeeId: 'active-1',
          status: EmployeeStatus.Active,
        }),
        buildEmployee({
          employeeId: 'deactivated-1',
          status: EmployeeStatus.Deactivated,
        }),
      ]);
      component.toggleSelectAll(true);
      deactivateEmployeeSilently.mockReturnValue(
        throwError(() => new Error('boom')),
      );

      await component.bulkDeleteSelected();

      expect(deleteEmployeeSilently).toHaveBeenCalledWith('deactivated-1');
      expect(notificationShow).toHaveBeenCalledWith(
        'Bulk action completed with 1 failure(s) (1 succeeded).',
        'error',
      );
    });
  });
});
