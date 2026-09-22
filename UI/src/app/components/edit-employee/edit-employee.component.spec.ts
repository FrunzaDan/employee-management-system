import { HttpErrorResponse } from '@angular/common/http';
import { signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { submit } from '@angular/forms/signals';
import { Router, provideRouter } from '@angular/router';
import { of, throwError } from 'rxjs';
import { Employee } from '../../interfaces/employee-response';
import { GetEmployeeService } from '../../services/get-employee.service';
import { EditEmployeeService } from '../../services/edit-employee.service';
import { OfficeService } from '../../services/office.service';
import { DepartmentService } from '../../services/department.service';
import { CostCenterService } from '../../services/cost-center.service';
import { EditEmployeeComponent } from './edit-employee.component';

describe('EditEmployeeComponent', () => {
  let getEmployee: ReturnType<typeof vi.fn>;
  let editEmployee: ReturnType<typeof vi.fn>;
  let navigate: ReturnType<typeof vi.fn>;
  let selectedEmployee: ReturnType<typeof signal<Employee | null>>;

  const buildEmployee = (overrides: Partial<Employee> = {}): Employee => ({
    guid: 'guid-1',
    firstName: 'Dan',
    lastName: 'Frunza',
    msisdn: '123456789',
    email: 'dan@example.com',
    gender: 1,
    employeeStatus: 1901,
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
    officeGuid: '11111111-1111-1111-1111-111111111111',
    departmentGuid: '22222222-2222-2222-2222-222222222222',
    costCenterGuid: '33333333-3333-3333-3333-333333333333',
    ...overrides,
  });

  // `id` is what withComponentInputBinding() binds from `?id=`.
  const createComponent = (id: string | null = 'guid-1') => {
    const fixture = TestBed.createComponent(EditEmployeeComponent);
    if (id) fixture.componentRef.setInput('id', id);
    fixture.detectChanges();
    return fixture.componentInstance;
  };

  beforeEach(() => {
    getEmployee = vi.fn();
    editEmployee = vi.fn().mockReturnValue(of({ status: 200, responseMessage: 'ok' }));
    navigate = vi.fn().mockResolvedValue(true);
    selectedEmployee = signal<Employee | null>(null);

    TestBed.configureTestingModule({
      providers: [
        provideRouter([]),
        {
          provide: GetEmployeeService,
          useValue: {
            selectedEmployeeSignal: selectedEmployee,
            loadingSignal: signal(false),
            errorSignal: signal<string | null>(null),
            getEmployee,
          },
        },
        { provide: EditEmployeeService, useValue: { editEmployee } },
        // EmployeeFormFieldsComponent loads these to populate the job-info selects —
        // stubbed so the fixture doesn't need a real HttpClient in this suite.
        { provide: OfficeService, useValue: { officesSignal: signal([]), loadOffices: vi.fn() } },
        {
          provide: DepartmentService,
          useValue: { departmentsSignal: signal([]), loadDepartments: vi.fn() },
        },
        {
          provide: CostCenterService,
          useValue: { costCentersSignal: signal([]), loadCostCenters: vi.fn() },
        },
      ],
    });
    // RouterLink in the template needs the real Router; only stub navigate().
    TestBed.inject(Router).navigate = navigate as unknown as Router['navigate'];
  });

  describe('loading by id', () => {
    it('fetches the employee named by the id input', () => {
      createComponent('guid-1');

      expect(getEmployee).toHaveBeenCalledWith('guid-1');
    });

    it('does not fetch when there is no id', () => {
      createComponent(null);

      expect(getEmployee).not.toHaveBeenCalled();
    });
  });

  describe('form model derived from the loaded employee', () => {
    it('starts empty until a employee is loaded', () => {
      const component = createComponent();

      expect(component.model().firstName).toBe('');
    });

    it('pre-fills every field, converting gender to a string', () => {
      const component = createComponent();
      selectedEmployee.set(buildEmployee({ gender: 2 }));

      expect(component.model().firstName).toBe('Dan');
      expect(component.model().gender).toBe('2');
      expect(component.model().country).toBe('Romania');
    });

    it('zero-pads an unpadded stored birthdate for the date input', () => {
      const component = createComponent();
      selectedEmployee.set(buildEmployee({ birthdate: '2020-1-5' }));

      expect(component.model().birthdate).toBe('2020-01-05');
    });

    it('leaves the birthdate blank when the stored value is not a full date', () => {
      const component = createComponent();
      selectedEmployee.set(buildEmployee({ birthdate: '2020' }));

      expect(component.model().birthdate).toBe('');
    });
  });

  describe('submitting', () => {
    it('does nothing and reports the errors when the form is invalid', async () => {
      const component = createComponent();
      selectedEmployee.set(buildEmployee());
      component.model.update((m) => ({ ...m, firstName: '' }));

      await submit(component.employeeForm);

      expect(editEmployee).not.toHaveBeenCalled();
      expect(component.invalidSummary()).toBe(
        'The form has 1 error. Please correct the highlighted fields.',
      );
    });

    it('does nothing when no employee has been loaded yet', async () => {
      const component = createComponent();
      component.model.set({ ...component.model(), ...toModel(buildEmployee()) });

      await submit(component.employeeForm);

      expect(editEmployee).not.toHaveBeenCalled();
    });

    it('merges the form values onto the loaded employee and saves', async () => {
      const component = createComponent();
      selectedEmployee.set(buildEmployee({ guid: 'guid-1', creationDate: '2026-01-01' }));
      component.model.update((m) => ({ ...m, firstName: 'Updated' }));

      await submit(component.employeeForm);

      expect(editEmployee).toHaveBeenCalledWith(
        expect.objectContaining({
          guid: 'guid-1',
          creationDate: '2026-01-01', // preserved from the original record, not in the form
          firstName: 'Updated',
          gender: 1,
        }),
      );
    });

    it('navigates back to the employee list on success', async () => {
      const component = createComponent();
      selectedEmployee.set(buildEmployee());

      await submit(component.employeeForm);

      expect(navigate).toHaveBeenCalledWith(['/employees']);
    });

    it('surfaces the error and stops submitting on failure', async () => {
      const component = createComponent();
      selectedEmployee.set(buildEmployee());
      editEmployee.mockReturnValue(
        throwError(
          () =>
            new HttpErrorResponse({
              status: 400,
              error: { responseMessage: 'Email already registered.' },
            }),
        ),
      );

      await submit(component.employeeForm);

      expect(component.employeeForm().submitting()).toBe(false);
      expect(component.saveError()).toBe('Email already registered.');
      expect(navigate).not.toHaveBeenCalled();
    });
  });

  describe('unsaved changes', () => {
    it('has none after the employee loads (loading is not editing)', () => {
      const component = createComponent();
      selectedEmployee.set(buildEmployee());

      expect(component.hasUnsavedChanges()).toBe(false);
    });

    it('has some once the user changes a field', () => {
      const component = createComponent();
      selectedEmployee.set(buildEmployee());
      component.model.update((m) => ({ ...m, firstName: 'Updated' }));

      expect(component.hasUnsavedChanges()).toBe(true);
    });

    it('has none again if the user puts the original value back', () => {
      const component = createComponent();
      selectedEmployee.set(buildEmployee());
      component.model.update((m) => ({ ...m, firstName: 'Updated' }));
      component.model.update((m) => ({ ...m, firstName: 'Dan' }));

      expect(component.hasUnsavedChanges()).toBe(false);
    });

    it('keeps them when the save fails', async () => {
      const component = createComponent();
      selectedEmployee.set(buildEmployee());
      editEmployee.mockReturnValue(throwError(() => new HttpErrorResponse({ status: 500 })));
      component.model.update((m) => ({ ...m, firstName: 'Updated' }));

      await submit(component.employeeForm);

      expect(component.hasUnsavedChanges()).toBe(true);
    });

    it('clears them once saved, before navigating away', async () => {
      const component = createComponent();
      selectedEmployee.set(buildEmployee());
      let dirtyAtNavigation: boolean | undefined;
      navigate.mockImplementation(async () => {
        dirtyAtNavigation = component.hasUnsavedChanges();
        return true;
      });
      component.model.update((m) => ({ ...m, firstName: 'Updated' }));

      await submit(component.employeeForm);

      expect(dirtyAtNavigation).toBe(false);
    });

    it('asks the browser to confirm closing/reloading the tab only when dirty', () => {
      const component = createComponent();
      selectedEmployee.set(buildEmployee());

      const clean = new Event('beforeunload', { cancelable: true }) as BeforeUnloadEvent;
      component.onBeforeUnload(clean);
      expect(clean.defaultPrevented).toBe(false);

      component.model.update((m) => ({ ...m, firstName: 'Updated' }));
      const dirty = new Event('beforeunload', { cancelable: true }) as BeforeUnloadEvent;
      component.onBeforeUnload(dirty);
      expect(dirty.defaultPrevented).toBe(true);
    });
  });

  // Local helper: the same mapping the component uses, to build a valid model
  // without a loaded employee.
  function toModel(employee: Employee) {
    return {
      firstName: employee.firstName,
      lastName: employee.lastName,
      email: employee.email,
      msisdn: employee.msisdn,
      gender: String(employee.gender),
      birthdate: employee.birthdate,
      country: employee.address.country,
      county: employee.address.county,
      town: employee.address.town,
      street: employee.address.street,
      number: employee.address.number,
      zip: employee.address.zip,
      hireDate: employee.hireDate ?? '',
      officeGuid: employee.officeGuid ?? '',
      departmentGuid: employee.departmentGuid ?? '',
      costCenterGuid: employee.costCenterGuid ?? '',
    };
  }
});
