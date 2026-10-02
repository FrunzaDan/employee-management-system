import { signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { form } from '@angular/forms/signals';
import {
  emptyEmployeeForm,
  employeeFormSchema,
  isEmployeeFormDirty,
  toCreateEmployeeRequest,
} from './employee-form';
import { environment } from '../../../environments/environment';

describe('isEmployeeFormDirty', () => {
  it('is false when nothing differs from the baseline', () => {
    expect(isEmployeeFormDirty(emptyEmployeeForm(), emptyEmployeeForm())).toBe(
      false,
    );
  });

  it('is true when any single field differs', () => {
    const changed = { ...emptyEmployeeForm(), postalCode: '400000' };

    expect(isEmployeeFormDirty(changed, emptyEmployeeForm())).toBe(true);
  });

  it('is false again once a changed value is put back', () => {
    const baseline = { ...emptyEmployeeForm(), firstName: 'Dan' };
    const edited = { ...baseline, firstName: 'Daniel' };
    const reverted = { ...edited, firstName: 'Dan' };

    expect(isEmployeeFormDirty(edited, baseline)).toBe(true);
    expect(isEmployeeFormDirty(reverted, baseline)).toBe(false);
  });
});

describe('environment.emailRegex', () => {
  const emailRegex = new RegExp(environment.emailRegex);

  it('accepts a plain address', () => {
    expect(emailRegex.test('ana.pop@example.com')).toBe(true);
  });

  it.each(['a@b@c.com', 'ana pop@example.com', 'ana@example', '@example.com'])(
    'rejects %s',
    (email) => {
      expect(emailRegex.test(email)).toBe(false);
    },
  );
});

describe('toCreateEmployeeRequest', () => {
  it('sends gender as a number, nests the address and leaves blank optional values out', () => {
    const request = toCreateEmployeeRequest({
      ...emptyEmployeeForm(),
      gender: '2',
      city: 'Cluj-Napoca',
    });

    expect(request.gender).toBe(2);
    expect(request.address.city).toBe('Cluj-Napoca');
    expect(request.birthDate).toBeUndefined();
    expect(request.hireDate).toBeUndefined();
    expect(request.officeId).toBeUndefined();
    expect(request.departmentId).toBeUndefined();
    expect(request.costCenterId).toBeUndefined();
  });
});

describe('employeeFormSchema', () => {
  const birthDateErrors = (birthDate: string) =>
    TestBed.runInInjectionContext(() =>
      form(signal({ ...emptyEmployeeForm(), birthDate }), employeeFormSchema),
    )
      .birthDate()
      .errors()
      .map((error) => error.message);

  it('rejects a birth date in the future', () => {
    expect(birthDateErrors('2999-01-01')).toContain(
      'Birth date cannot be in the future',
    );
  });

  it('accepts a birth date in the past', () => {
    expect(birthDateErrors('1990-01-01')).toEqual([]);
  });
});
