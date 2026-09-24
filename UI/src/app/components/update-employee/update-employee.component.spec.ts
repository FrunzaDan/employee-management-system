import { HttpErrorResponse } from '@angular/common/http';
import { signal } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { submit } from '@angular/forms/signals';
import { Router, provideRouter } from '@angular/router';
import { ReplaySubject, of, throwError } from 'rxjs';
import { Employee } from '../../interfaces/employee';
import { EmployeeService } from '../../services/employee.service';
import { OfficeService } from '../../services/office.service';
import { DepartmentService } from '../../services/department.service';
import { CostCenterService } from '../../services/cost-center.service';
import { UpdateEmployeeComponent } from './update-employee.component';

describe('UpdateEmployeeComponent', () => {
  let getEmployee: ReturnType<typeof vi.fn>;
  let updateEmployee: ReturnType<typeof vi.fn>;
  let navigate: ReturnType<typeof vi.fn>;
  let employee$: ReplaySubject<Employee>;
  let fixture: ComponentFixture<UpdateEmployeeComponent>;

  // Emits the employee the page's rxResource streams, and waits for it to land.
  const loadEmployee = async (employee: Employee) => {
    employee$.next(employee);
    await fixture.whenStable();
  };

  const buildEmployee = (overrides: Partial<Employee> = {}): Employee => ({
    employeeId: 'employeeId-1',
    firstName: 'Dan',
    lastName: 'Frunza',
    phoneNumber: '123456789',
    email: 'dan@example.com',
    gender: 1,
    status: 1901,
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
    officeId: '11111111-1111-1111-1111-111111111111',
    departmentId: '22222222-2222-2222-2222-222222222222',
    costCenterId: '33333333-3333-3333-3333-333333333333',
    officeName: null,
    departmentName: null,
    costCenterName: null,
    currentGrossSalary: null,
    ...overrides,
  });

  // `employeeId` is what withComponentInputBinding() binds from the `:employeeId` route param.
  const createComponent = (id: string | null = 'employeeId-1') => {
    fixture = TestBed.createComponent(UpdateEmployeeComponent);
    if (id) fixture.componentRef.setInput('employeeId', id);
    fixture.detectChanges();
    return fixture.componentInstance;
  };

  beforeEach(() => {
    employee$ = new ReplaySubject<Employee>(1);
    getEmployee = vi.fn(() => employee$);
    updateEmployee = vi
      .fn()
      .mockReturnValue(of({ status: 200, responseMessage: 'ok' }));
    navigate = vi.fn().mockResolvedValue(true);

    TestBed.configureTestingModule({
      providers: [
        provideRouter([]),
        {
          provide: EmployeeService,
          useValue: {
            getEmployee,
            updateEmployee,
          },
        },
        // EmployeeFormFieldsComponent loads these to populate the job-info selects —
        // stubbed so the fixture doesn't need a real HttpClient in this suite.
        {
          provide: OfficeService,
          useValue: { offices: signal([]), loadOffices: vi.fn() },
        },
        {
          provide: DepartmentService,
          useValue: { departments: signal([]), loadDepartments: vi.fn() },
        },
        {
          provide: CostCenterService,
          useValue: { costCenters: signal([]), loadCostCenters: vi.fn() },
        },
      ],
    });
    // RouterLink in the template needs the real Router; only stub navigate().
    TestBed.inject(Router).navigate = navigate as unknown as Router['navigate'];
  });

  describe('loading by id', () => {
    it('fetches the employee named by the id input', () => {
      createComponent('employeeId-1');

      expect(getEmployee).toHaveBeenCalledWith('employeeId-1');
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

    it('pre-fills every field, converting gender to a string', async () => {
      const component = createComponent();
      await loadEmployee(buildEmployee({ gender: 2 }));

      expect(component.model().firstName).toBe('Dan');
      expect(component.model().gender).toBe('2');
      expect(component.model().country).toBe('Romania');
    });

    it('zero-pads an unpadded stored birthDate for the date input', async () => {
      const component = createComponent();
      await loadEmployee(buildEmployee({ birthDate: '2020-1-5' }));

      expect(component.model().birthDate).toBe('2020-01-05');
    });

    it('leaves the birthDate blank when the stored value is not a full date', async () => {
      const component = createComponent();
      await loadEmployee(buildEmployee({ birthDate: '2020' }));

      expect(component.model().birthDate).toBe('');
    });
  });

  describe('submitting', () => {
    it('does nothing and reports the errors when the form is invalid', async () => {
      const component = createComponent();
      await loadEmployee(buildEmployee());
      component.model.update((m) => ({ ...m, firstName: '' }));

      await submit(component.employeeForm);

      expect(updateEmployee).not.toHaveBeenCalled();
      expect(component.invalidSummary()).toBe(
        'The form has 1 error. Please correct the highlighted fields.',
      );
    });

    it('does nothing when no employee has been loaded yet', async () => {
      const component = createComponent();
      component.model.set({
        ...component.model(),
        ...toModel(buildEmployee()),
      });

      await submit(component.employeeForm);

      expect(updateEmployee).not.toHaveBeenCalled();
    });

    it('merges the form values onto the loaded employee and saves', async () => {
      const component = createComponent();
      await loadEmployee(
        buildEmployee({ employeeId: 'employeeId-1', createdAt: '2026-01-01' }),
      );
      component.model.update((m) => ({ ...m, firstName: 'Updated' }));

      await submit(component.employeeForm);

      expect(updateEmployee).toHaveBeenCalledWith(
        expect.objectContaining({
          employeeId: 'employeeId-1',
          createdAt: '2026-01-01', // preserved from the original record, not in the form
          firstName: 'Updated',
          gender: 1,
        }),
      );
    });

    it('navigates back to the employee list on success', async () => {
      const component = createComponent();
      await loadEmployee(buildEmployee());

      await submit(component.employeeForm);

      expect(navigate).toHaveBeenCalledWith(['/employees']);
    });

    it('surfaces the error and stops submitting on failure', async () => {
      const component = createComponent();
      await loadEmployee(buildEmployee());
      updateEmployee.mockReturnValue(
        throwError(
          () =>
            new HttpErrorResponse({
              status: 400,
              error: { title: 'Error', detail: 'Email already registered.' },
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
    it('has none after the employee loads (loading is not editing)', async () => {
      const component = createComponent();
      await loadEmployee(buildEmployee());

      expect(component.hasUnsavedChanges()).toBe(false);
    });

    it('has some once the user changes a field', async () => {
      const component = createComponent();
      await loadEmployee(buildEmployee());
      component.model.update((m) => ({ ...m, firstName: 'Updated' }));

      expect(component.hasUnsavedChanges()).toBe(true);
    });

    it('has none again if the user puts the original value back', async () => {
      const component = createComponent();
      await loadEmployee(buildEmployee());
      component.model.update((m) => ({ ...m, firstName: 'Updated' }));
      component.model.update((m) => ({ ...m, firstName: 'Dan' }));

      expect(component.hasUnsavedChanges()).toBe(false);
    });

    it('keeps them when the save fails', async () => {
      const component = createComponent();
      await loadEmployee(buildEmployee());
      updateEmployee.mockReturnValue(
        throwError(() => new HttpErrorResponse({ status: 500 })),
      );
      component.model.update((m) => ({ ...m, firstName: 'Updated' }));

      await submit(component.employeeForm);

      expect(component.hasUnsavedChanges()).toBe(true);
    });

    it('clears them once saved, before navigating away', async () => {
      const component = createComponent();
      await loadEmployee(buildEmployee());
      let dirtyAtNavigation: boolean | undefined;
      navigate.mockImplementation(async () => {
        dirtyAtNavigation = component.hasUnsavedChanges();
        return true;
      });
      component.model.update((m) => ({ ...m, firstName: 'Updated' }));

      await submit(component.employeeForm);

      expect(dirtyAtNavigation).toBe(false);
    });

    it('asks the browser to confirm closing/reloading the tab only when dirty', async () => {
      const component = createComponent();
      await loadEmployee(buildEmployee());

      const clean = new Event('beforeunload', {
        cancelable: true,
      }) as BeforeUnloadEvent;
      component.onBeforeUnload(clean);
      expect(clean.defaultPrevented).toBe(false);

      component.model.update((m) => ({ ...m, firstName: 'Updated' }));
      const dirty = new Event('beforeunload', {
        cancelable: true,
      }) as BeforeUnloadEvent;
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
      phoneNumber: employee.phoneNumber,
      gender: String(employee.gender),
      birthDate: employee.birthDate ?? '',
      country: employee.address.country ?? '',
      county: employee.address.county ?? '',
      city: employee.address.city ?? '',
      street: employee.address.street ?? '',
      streetNumber: employee.address.streetNumber ?? '',
      postalCode: employee.address.postalCode ?? '',
      hireDate: employee.hireDate ?? '',
      officeId: employee.officeId ?? '',
      departmentId: employee.departmentId ?? '',
      costCenterId: employee.costCenterId ?? '',
    };
  }
});
