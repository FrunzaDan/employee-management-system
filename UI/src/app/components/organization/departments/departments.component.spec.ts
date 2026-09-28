import { HttpErrorResponse } from '@angular/common/http';
import { signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { of, throwError } from 'rxjs';
import { Department } from '../../../interfaces/department';
import { EmployeeSummary } from '../../../interfaces/employee-summary';
import { EmployeeStatus } from '../../../interfaces/employee';
import { DepartmentService } from '../../../services/department.service';
import { ConfirmDialogService } from '../../../services/confirm-dialog.service';
import { DepartmentsComponent } from './departments.component';

describe('DepartmentsComponent', () => {
  let loadDepartments: ReturnType<typeof vi.fn>;
  let createDepartment: ReturnType<typeof vi.fn>;
  let updateDepartment: ReturnType<typeof vi.fn>;
  let deleteDepartment: ReturnType<typeof vi.fn>;
  let getEmployees: ReturnType<typeof vi.fn>;
  let confirm: ReturnType<typeof vi.fn>;

  const department: Department = {
    departmentId: 'department-1',
    name: 'Engineering',
    employeeCount: 2,
    totalGrossSalary: 12000,
  };

  const employee: EmployeeSummary = {
    employeeId: 'employee-1',
    firstName: 'Ana',
    lastName: 'Pop',
    email: 'ana@example.com',
    status: EmployeeStatus.Active,
  };

  const conflict = new HttpErrorResponse({
    status: 409,
    error: { status: 409, detail: 'Department still has employees.' },
  });

  const createComponent = () => {
    loadDepartments = vi.fn();
    createDepartment = vi
      .fn()
      .mockReturnValue(of({ status: 200, responseMessage: 'ok' }));
    updateDepartment = vi
      .fn()
      .mockReturnValue(of({ status: 200, responseMessage: 'ok' }));
    deleteDepartment = vi
      .fn()
      .mockReturnValue(of({ status: 200, responseMessage: 'ok' }));
    getEmployees = vi.fn().mockReturnValue(of([employee]));
    confirm = vi.fn().mockResolvedValue(true);

    TestBed.configureTestingModule({
      providers: [
        {
          provide: DepartmentService,
          useValue: {
            departments: signal([department]),
            loading: signal(false),
            error: signal(null),
            loadDepartments,
            createDepartment,
            updateDepartment,
            deleteDepartment,
            getEmployees,
          },
        },
        { provide: ConfirmDialogService, useValue: { confirm } },
      ],
    });
    return TestBed.runInInjectionContext(() => new DepartmentsComponent());
  };

  it('loads the departments on init', () => {
    const component = createComponent();

    component.ngOnInit();

    expect(loadDepartments).toHaveBeenCalledTimes(1);
  });

  it('starts an add with an empty draft and an edit from the department', () => {
    const component = createComponent();

    component.startAdd();
    expect(component.draft()).toEqual({ departmentId: null, name: '' });

    component.startEdit(department);
    expect(component.draft()).toEqual({
      departmentId: 'department-1',
      name: 'Engineering',
    });
  });

  describe('save', () => {
    it('refuses a blank name without calling the API', async () => {
      const component = createComponent();
      component.startAdd();
      component.updateDraft('  ');

      await component.save();

      expect(component.saveError()).toBe('Department name is required.');
      expect(createDepartment).not.toHaveBeenCalled();
    });

    it('creates a new department and closes the draft', async () => {
      const component = createComponent();
      component.startAdd();
      component.updateDraft('Sales');

      await component.save();

      expect(createDepartment).toHaveBeenCalledWith({ name: 'Sales' });
      expect(component.draft()).toBeNull();
      expect(component.saving()).toBe(false);
    });

    it('updates an existing department by its id', async () => {
      const component = createComponent();
      component.startEdit(department);
      component.updateDraft('Platform');

      await component.save();

      expect(updateDepartment).toHaveBeenCalledWith({
        departmentId: 'department-1',
        name: 'Platform',
      });
      expect(createDepartment).not.toHaveBeenCalled();
    });

    it('keeps the draft open and shows the error when the save fails', async () => {
      const component = createComponent();
      createDepartment.mockReturnValue(throwError(() => conflict));
      component.startAdd();
      component.updateDraft('Sales');

      await component.save();

      expect(component.saveError()).toBe('Department still has employees.');
      expect(component.draft()).not.toBeNull();
      expect(component.saving()).toBe(false);
    });
  });

  describe('delete', () => {
    it('asks first, naming the department, then deletes it', async () => {
      const component = createComponent();

      await component.deleteDepartment(department);

      expect(confirm).toHaveBeenCalledWith(
        'Delete department "Engineering"? This cannot be undone.',
        expect.objectContaining({ variant: 'danger' }),
      );
      expect(deleteDepartment).toHaveBeenCalledWith('department-1');
    });

    it('does nothing when the user cancels', async () => {
      const component = createComponent();
      confirm.mockResolvedValue(false);

      await component.deleteDepartment(department);

      expect(deleteDepartment).not.toHaveBeenCalled();
    });

    it('shows why the delete failed', async () => {
      const component = createComponent();
      deleteDepartment.mockReturnValue(throwError(() => conflict));

      await component.deleteDepartment(department);

      expect(component.deleteError()).toBe('Department still has employees.');
    });
  });

  describe('toggleEmployees', () => {
    it('expands the department and lists its employees', () => {
      const component = createComponent();

      component.toggleEmployees(department);

      expect(getEmployees).toHaveBeenCalledWith('department-1');
      expect(component.expandedDepartmentId()).toBe('department-1');
      expect(component.expandedEmployees()).toEqual([employee]);
    });

    it('collapses on a second click without asking the API again', () => {
      const component = createComponent();
      component.toggleEmployees(department);

      component.toggleEmployees(department);

      expect(component.expandedDepartmentId()).toBeNull();
      expect(getEmployees).toHaveBeenCalledTimes(1);
    });

    it('shows an error when the employees cannot be loaded', () => {
      const component = createComponent();
      getEmployees.mockReturnValue(
        throwError(() => new HttpErrorResponse({ status: 503 })),
      );

      component.toggleEmployees(department);

      expect(component.expandedEmployeesError()).toBe(
        'Failed to load employees (503). Please try again.',
      );
      expect(component.expandedEmployeesLoading()).toBe(false);
    });
  });
});
