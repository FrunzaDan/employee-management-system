import { HttpErrorResponse } from '@angular/common/http';
import { signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { of, throwError } from 'rxjs';
import { ActivateEmployeeService } from '../../services/activate-employee.service';
import { ConfirmDialogService } from '../../services/confirm-dialog.service';
import { DeleteEmployeeService } from '../../services/delete-employee.service';
import { ExportEmployeeService } from '../../services/export-employee.service';
import { GetEmployeeService } from '../../services/get-employee.service';
import { NotificationService } from '../../services/notification.service';
import {
  Employee,
  EmployeeStatus,
} from '../../interfaces/employee-response';
import { EmployeeListComponent } from './employee-list.component';

describe('EmployeeListComponent', () => {
  let component: EmployeeListComponent;
  let loadEmployees: ReturnType<typeof vi.fn>;
  let exportEmployees: ReturnType<typeof vi.fn>;
  let totalItems: ReturnType<typeof signal<number>>;
  let employeesSignal: ReturnType<typeof signal<Employee[]>>;
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
    ...overrides,
  });

  beforeEach(() => {
    loadEmployees = vi.fn();
    exportEmployees = vi.fn();
    totalItems = signal(0);
    employeesSignal = signal<Employee[]>([]);
    deleteEmployee = vi.fn().mockReturnValue(of({ status: 200, responseMessage: 'ok' }));
    deleteEmployeeSilently = vi
      .fn()
      .mockReturnValue(of({ status: 200, responseMessage: 'ok' }));
    deactivateEmployeeSilently = vi
      .fn()
      .mockReturnValue(of({ status: 200, responseMessage: 'ok' }));
    confirm = vi.fn().mockResolvedValue(true);
    notificationShow = vi.fn();

    const getEmployeeServiceStub = {
      employeesSignal,
      loadingSignal: signal(false),
      errorSignal: signal<string | null>(null),
      totalItemsSignal: totalItems,
      pageNumberSignal: signal(1),
      pageSizeSignal: signal(10),
      loadEmployees,
    };

    TestBed.configureTestingModule({
      providers: [
        {
          provide: GetEmployeeService,
          useValue: getEmployeeServiceStub,
        },
        {
          provide: ActivateEmployeeService,
          useValue: {
            loadingSignal: signal(false),
            errorSignal: signal<string | null>(null),
            deactivateEmployeeSilently,
          },
        },
        {
          provide: DeleteEmployeeService,
          useValue: { deleteEmployee, deleteEmployeeSilently },
        },
        {
          provide: ExportEmployeeService,
          useValue: {
            loadingSignal: signal(false),
            errorSignal: signal<string | null>(null),
            exportEmployees,
          },
        },
        { provide: ConfirmDialogService, useValue: { confirm } },
        { provide: NotificationService, useValue: { show: notificationShow } },
        { provide: Router, useValue: { navigate: vi.fn() } },
      ],
    });

    component = TestBed.runInInjectionContext(() => new EmployeeListComponent());
  });

  afterEach(() => {
    vi.useRealTimers();
  });

  describe('setSort', () => {
    it('toggles direction when clicking the already-active column, and resets to page 1', () => {
      totalItems.set(75);
      component.currentPage.set(3);
      loadEmployees.mockClear();

      component.setSort('name'); // 'name' is already the default sort column

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
      totalItems.set(120); // 120 items / 50 per page = 3 pages
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

      component.goToPage(1); // already on page 1, totalPages() is 1 with 0 items

      expect(loadEmployees).not.toHaveBeenCalled();
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

  describe('toggleSelection / toggleSelectAllOnPage', () => {
    it('adds a employeeId to selectedEmployeeIds when checked, and removes it when unchecked', () => {
      component.toggleSelection('employeeId-1', true);
      expect(component.isSelected('employeeId-1')).toBe(true);

      component.toggleSelection('employeeId-1', false);
      expect(component.isSelected('employeeId-1')).toBe(false);
    });

    it('allOnPageSelected is false when the page is empty', () => {
      employeesSignal.set([]);
      expect(component.allOnPageSelected()).toBe(false);
    });

    it('toggleSelectAllOnPage(true) selects every employee on the current page', () => {
      employeesSignal.set([buildEmployee({ employeeId: 'g1' }), buildEmployee({ employeeId: 'g2' })]);

      component.toggleSelectAllOnPage(true);

      expect(component.isSelected('g1')).toBe(true);
      expect(component.isSelected('g2')).toBe(true);
      expect(component.allOnPageSelected()).toBe(true);
    });

    it('toggleSelectAllOnPage(false) clears the selection for every employee on the current page', () => {
      employeesSignal.set([buildEmployee({ employeeId: 'g1' }), buildEmployee({ employeeId: 'g2' })]);
      component.toggleSelectAllOnPage(true);

      component.toggleSelectAllOnPage(false);

      expect(component.isSelected('g1')).toBe(false);
      expect(component.isSelected('g2')).toBe(false);
      expect(component.allOnPageSelected()).toBe(false);
    });
  });

  describe('duplicateGuids effect', () => {
    it('warns when the current page contains duplicate GUIDs', () => {
      const warnSpy = vi.spyOn(console, 'warn').mockImplementation(() => {});

      employeesSignal.set([
        buildEmployee({ employeeId: 'dup' }),
        buildEmployee({ employeeId: 'dup' }),
      ]);
      TestBed.flushEffects();

      expect(warnSpy).toHaveBeenCalledWith('Duplicate GUIDs found:', ['dup']);
      warnSpy.mockRestore();
    });

    it('does not warn when every GUID on the page is unique', () => {
      const warnSpy = vi.spyOn(console, 'warn').mockImplementation(() => {});

      employeesSignal.set([buildEmployee({ employeeId: 'g1' }), buildEmployee({ employeeId: 'g2' })]);
      TestBed.flushEffects();

      expect(warnSpy).not.toHaveBeenCalled();
      warnSpy.mockRestore();
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
      // removeEmployeeLocally only drops the row locally; the component still
      // re-fetches so totalItems/page count don't go stale.
      expect(loadEmployees).toHaveBeenCalledTimes(1);
    });

    it('surfaces the error and stops loading when the delete request fails', async () => {
      deleteEmployee.mockReturnValue(
        throwError(
          () =>
            new HttpErrorResponse({
              status: 409,
              error: { responseMessage: 'Employee must be deactivated first.' },
            }),
        ),
      );

      await component.deleteEmployee('employeeId-1');

      expect(component.deleting()).toBe(false);
      expect(component.deleteError()).toBe('Employee must be deactivated first.');
    });
  });

  describe('bulkDeleteSelected', () => {
    it('does nothing when no rows are selected', async () => {
      await component.bulkDeleteSelected();

      expect(confirm).not.toHaveBeenCalled();
    });

    it('does not call any API when the user cancels the confirmation', async () => {
      employeesSignal.set([buildEmployee({ employeeId: 'g1' })]);
      component.toggleSelection('g1', true);
      confirm.mockResolvedValue(false);

      await component.bulkDeleteSelected();

      expect(deactivateEmployeeSilently).not.toHaveBeenCalled();
      expect(deleteEmployeeSilently).not.toHaveBeenCalled();
    });

    it('deactivates Active employees and deletes non-Active ones, then shows a success summary and refetches', async () => {
      employeesSignal.set([
        buildEmployee({ employeeId: 'active-1', status: EmployeeStatus.Active }),
        buildEmployee({ employeeId: 'deactivated-1', status: EmployeeStatus.Deactivated }),
        buildEmployee({ employeeId: 'test-1', status: EmployeeStatus.Test }),
      ]);
      component.toggleSelectAllOnPage(true);
      loadEmployees.mockClear();

      await component.bulkDeleteSelected();

      expect(confirm).toHaveBeenCalledWith(expect.stringContaining('1 is active'));
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
      employeesSignal.set([
        buildEmployee({ employeeId: 'active-1', status: EmployeeStatus.Active }),
        buildEmployee({ employeeId: 'deactivated-1', status: EmployeeStatus.Deactivated }),
      ]);
      component.toggleSelectAllOnPage(true);
      deactivateEmployeeSilently.mockReturnValue(throwError(() => new Error('boom')));

      await component.bulkDeleteSelected();

      expect(deleteEmployeeSilently).toHaveBeenCalledWith('deactivated-1');
      expect(notificationShow).toHaveBeenCalledWith(
        'Bulk action completed with 1 failure(s) (1 succeeded).',
        'error',
      );
    });
  });
});
