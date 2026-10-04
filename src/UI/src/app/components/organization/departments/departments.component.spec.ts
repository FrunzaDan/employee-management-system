import { HttpErrorResponse } from '@angular/common/http';
import { signal } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { submit } from '@angular/forms/signals';
import { provideRouter } from '@angular/router';
import { of, throwError } from 'rxjs';
import { Department } from '../../../interfaces/department';
import { DepartmentService } from '../../../services/department.service';
import { ConfirmDialogService } from '../../../services/confirm-dialog.service';
import { DepartmentsComponent } from './departments.component';

describe('DepartmentsComponent', () => {
  let loadDepartments: ReturnType<typeof vi.fn>;
  let createDepartment: ReturnType<typeof vi.fn>;
  let updateDepartment: ReturnType<typeof vi.fn>;
  let deleteDepartment: ReturnType<typeof vi.fn>;
  let confirm: ReturnType<typeof vi.fn>;

  const department: Department = {
    departmentId: 'department-1',
    name: 'Engineering',
    employeeCount: 2,
    totalGrossSalary: 12000,
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
    expect(component.departmentForm().value()).toEqual({
      departmentId: null,
      name: '',
    });

    component.startEdit(department);
    expect(component.departmentForm().value()).toEqual({
      departmentId: 'department-1',
      name: 'Engineering',
    });
  });

  describe('save', () => {
    it('refuses a blank name without calling the API', async () => {
      const component = createComponent();
      component.startAdd();
      component.departmentForm.name().value.set('  ');

      await submit(component.departmentForm);

      expect(component.departmentForm.name().errors()[0].message).toBe(
        'Department name is required.',
      );
      expect(component.saveError()).toBeNull();
      expect(createDepartment).not.toHaveBeenCalled();
    });

    it('creates a new department and closes the draft', async () => {
      const component = createComponent();
      component.startAdd();
      component.departmentForm.name().value.set('Sales');

      await submit(component.departmentForm);

      expect(createDepartment).toHaveBeenCalledWith({ name: 'Sales' });
      expect(component.editorOpen()).toBe(false);
      expect(component.departmentForm().submitting()).toBe(false);
    });

    it('updates an existing department by its id', async () => {
      const component = createComponent();
      component.startEdit(department);
      component.departmentForm.name().value.set('Platform');

      await submit(component.departmentForm);

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
      component.departmentForm.name().value.set('Sales');

      await submit(component.departmentForm);

      expect(component.saveError()).toBe('Department still has employees.');
      expect(component.editorOpen()).toBe(true);
      expect(component.departmentForm().submitting()).toBe(false);
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
    it('expands the department to show its employees', () => {
      const component = createComponent();

      component.toggleEmployees(department);

      expect(component.expandedDepartmentId()).toBe('department-1');
    });

    it('collapses on a second click', () => {
      const component = createComponent();
      component.toggleEmployees(department);

      component.toggleEmployees(department);

      expect(component.expandedDepartmentId()).toBeNull();
    });
  });

  describe('template', () => {
    let fixture: ComponentFixture<DepartmentsComponent>;

    const el = <T extends HTMLElement>(selector: string): T =>
      fixture.nativeElement.querySelector(selector);
    const buttonByText = (text: string): HTMLButtonElement =>
      Array.from<HTMLButtonElement>(
        fixture.nativeElement.querySelectorAll('button'),
      ).find((b) => b.textContent?.trim() === text)!;
    const type = (selector: string, value: string) => {
      const input = el<HTMLInputElement>(selector);
      input.value = value;
      input.dispatchEvent(new Event('input'));
    };
    const settle = () => new Promise((resolve) => setTimeout(resolve));

    beforeEach(async () => {
      TestBed.configureTestingModule({ providers: [provideRouter([])] });
      createComponent();
      fixture = TestBed.createComponent(DepartmentsComponent);
      await fixture.whenStable();
    });

    it('loads and renders each department', () => {
      const row = el('tbody tr');

      expect(loadDepartments).toHaveBeenCalledTimes(1);
      expect(row.textContent).toContain('Engineering');
      expect(row.textContent).toContain('12.000,00 RON');
    });

    it('creates a department from the Add department form', async () => {
      buttonByText('Add department').click();
      await fixture.whenStable();
      expect(el('form h2').textContent).toContain('New department');

      type('#name', 'Sales');
      buttonByText('Save').click();
      await settle();

      expect(createDepartment).toHaveBeenCalledWith({ name: 'Sales' });
    });

    it('shows the error under the name and does not save a blank department', async () => {
      buttonByText('Add department').click();
      await fixture.whenStable();

      buttonByText('Save').click();
      await settle();
      await fixture.whenStable();

      expect(el('#name-error').textContent).toContain(
        'Department name is required.',
      );
      expect(createDepartment).not.toHaveBeenCalled();
    });

    it("edits the row's department from its pre-filled Edit form", async () => {
      buttonByText('Edit').click();
      await fixture.whenStable();
      expect(el('form h2').textContent).toContain('Edit department');
      expect(el<HTMLInputElement>('#name').value).toBe('Engineering');

      type('#name', 'Renamed');
      buttonByText('Save').click();
      await settle();

      expect(updateDepartment).toHaveBeenCalledWith(
        expect.objectContaining({
          departmentId: 'department-1',
          name: 'Renamed',
        }),
      );
      expect(createDepartment).not.toHaveBeenCalled();
    });

    it("deletes the row's department from its Delete button", async () => {
      buttonByText('Delete').click();
      await settle();

      expect(deleteDepartment).toHaveBeenCalledWith('department-1');
    });
  });
});
