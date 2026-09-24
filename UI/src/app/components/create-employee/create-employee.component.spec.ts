import { HttpErrorResponse } from '@angular/common/http';
import { TestBed } from '@angular/core/testing';
import { submit } from '@angular/forms/signals';
import { Router } from '@angular/router';
import { of, throwError } from 'rxjs';
import { EmployeeService } from '../../services/employee.service';
import { EmployeeFormModel } from '../employee-form-fields/employee-form';
import { CreateEmployeeComponent } from './create-employee.component';

describe('CreateEmployeeComponent', () => {
  let createEmployee: ReturnType<typeof vi.fn>;
  let navigate: ReturnType<typeof vi.fn>;
  let component: CreateEmployeeComponent;

  const validModel: EmployeeFormModel = {
    firstName: 'Dan',
    lastName: 'Frunza',
    email: 'dan@example.com',
    phoneNumber: '123456789',
    gender: '1',
    birthDate: '1990-01-01',
    country: 'Romania',
    county: 'Cluj',
    city: 'Cluj-Napoca',
    street: 'Main',
    streetNumber: '1',
    postalCode: '400000',
    hireDate: '2020-01-01',
    officeId: '11111111-1111-1111-1111-111111111111',
    departmentId: '22222222-2222-2222-2222-222222222222',
    costCenterId: '33333333-3333-3333-3333-333333333333',
  };

  beforeEach(() => {
    createEmployee = vi
      .fn()
      .mockReturnValue(of({ status: 200, responseMessage: 'ok' }));
    navigate = vi.fn().mockResolvedValue(true);

    // The component resolves its dependencies (and builds its signal form) in
    // field initializers, so it needs an active injection context.
    TestBed.configureTestingModule({
      providers: [
        { provide: Router, useValue: { navigate } },
        { provide: EmployeeService, useValue: { createEmployee } },
      ],
    });

    component = TestBed.runInInjectionContext(
      () => new CreateEmployeeComponent(),
    );
  });

  it('does not call the service and reports the errors when the form is invalid', async () => {
    await submit(component.employeeForm);

    expect(createEmployee).not.toHaveBeenCalled();
    expect(component.invalidSummary()).toBe(
      'The form has 16 errors. Please correct the highlighted fields.',
    );
  });

  it('validates the email and phone formats', () => {
    component.model.set({
      ...validModel,
      email: 'not-an-email',
      phoneNumber: '12',
    });

    expect(component.employeeForm.email().errors()[0].message).toBe(
      'The Email should be a valid one',
    );
    expect(component.employeeForm.phoneNumber().errors()[0].message).toBe(
      'The phone number should be a valid one',
    );
  });

  it('maps the form value into a Employee (gender as a number, address nested) and registers it', async () => {
    component.model.set(validModel);

    await submit(component.employeeForm);

    expect(createEmployee).toHaveBeenCalledWith(
      expect.objectContaining({
        firstName: 'Dan',
        lastName: 'Frunza',
        email: 'dan@example.com',
        phoneNumber: '123456789',
        gender: 1,
        birthDate: '1990-01-01',
        address: {
          country: 'Romania',
          county: 'Cluj',
          city: 'Cluj-Napoca',
          street: 'Main',
          streetNumber: '1',
          postalCode: '400000',
        },
        hireDate: '2020-01-01',
        officeId: '11111111-1111-1111-1111-111111111111',
        departmentId: '22222222-2222-2222-2222-222222222222',
        costCenterId: '33333333-3333-3333-3333-333333333333',
      }),
    );
  });

  it('navigates to the employee list on success', async () => {
    component.model.set(validModel);

    await submit(component.employeeForm);

    expect(navigate).toHaveBeenCalledWith(['/employees']);
    expect(component.errorMessage()).toBeNull();
  });

  it('sets a friendly message and stops submitting on a network error (status 0)', async () => {
    createEmployee.mockReturnValue(
      throwError(() => new HttpErrorResponse({ status: 0 })),
    );
    component.model.set(validModel);

    await submit(component.employeeForm);

    expect(component.employeeForm().submitting()).toBe(false);
    expect(component.errorMessage()).toBe(
      'Could not reach the server. It may be offline, or your browser may not trust its security certificate.',
    );
    expect(navigate).not.toHaveBeenCalled();
  });

  it('surfaces the server-provided message on a non-zero error status', async () => {
    createEmployee.mockReturnValue(
      throwError(
        () =>
          new HttpErrorResponse({
            status: 400,
            error: { title: 'Error', detail: 'Email already registered.' },
          }),
      ),
    );
    component.model.set(validModel);

    await submit(component.employeeForm);

    expect(component.errorMessage()).toBe('Email already registered.');
  });

  describe('unsaved changes', () => {
    it('has none on a fresh form', () => {
      expect(component.hasUnsavedChanges()).toBe(false);
    });

    it('has some as soon as the user types anything', () => {
      component.model.update((m) => ({ ...m, firstName: 'Dan' }));

      expect(component.hasUnsavedChanges()).toBe(true);
    });

    it('has none again if the field is emptied back out', () => {
      component.model.update((m) => ({ ...m, firstName: 'Dan' }));
      component.model.update((m) => ({ ...m, firstName: '' }));

      expect(component.hasUnsavedChanges()).toBe(false);
    });

    it('keeps them when the save fails, so the user is still warned', async () => {
      createEmployee.mockReturnValue(
        throwError(() => new HttpErrorResponse({ status: 500 })),
      );
      component.model.set(validModel);

      await submit(component.employeeForm);

      expect(component.hasUnsavedChanges()).toBe(true);
    });

    it('clears them once the employee is saved, before navigating away', async () => {
      let dirtyAtNavigation: boolean | undefined;
      navigate.mockImplementation(async () => {
        dirtyAtNavigation = component.hasUnsavedChanges();
        return true;
      });
      component.model.set(validModel);

      await submit(component.employeeForm);

      expect(dirtyAtNavigation).toBe(false);
    });

    it('asks the browser to confirm closing/reloading the tab only when dirty', () => {
      const clean = new Event('beforeunload', {
        cancelable: true,
      }) as BeforeUnloadEvent;
      component.onBeforeUnload(clean);
      expect(clean.defaultPrevented).toBe(false);

      component.model.update((m) => ({ ...m, firstName: 'Dan' }));
      const dirty = new Event('beforeunload', {
        cancelable: true,
      }) as BeforeUnloadEvent;
      component.onBeforeUnload(dirty);
      expect(dirty.defaultPrevented).toBe(true);
    });
  });
});
