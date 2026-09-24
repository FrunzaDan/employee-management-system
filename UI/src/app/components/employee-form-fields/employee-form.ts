import { pattern, required, schema } from '@angular/forms/signals';
import { environment } from '../../../environments/environment';
import {
  CreateEmployeeRequest,
  Employee,
  Gender,
} from '../../interfaces/employee';

export interface EmployeeFormModel {
  firstName: string;
  lastName: string;
  email: string;
  phoneNumber: string;
  gender: string;
  birthDate: string;
  country: string;
  county: string;
  city: string;
  street: string;
  streetNumber: string;
  postalCode: string;
  hireDate: string;
  officeId: string;
  departmentId: string;
  costCenterId: string;
}

export const emptyEmployeeForm = (): EmployeeFormModel => ({
  firstName: '',
  lastName: '',
  email: '',
  phoneNumber: '',
  gender: '',
  birthDate: '',
  country: '',
  county: '',
  city: '',
  street: '',
  streetNumber: '',
  postalCode: '',
  hireDate: '',
  officeId: '',
  departmentId: '',
  costCenterId: '',
});

export const employeeFormSchema = schema<EmployeeFormModel>((p) => {
  required(p.firstName, { message: 'First name is required' });
  required(p.lastName, { message: 'Last name is required' });
  required(p.email, { message: 'Email is required' });
  pattern(p.email, new RegExp(environment.emailRegex), {
    message: 'The email should be a valid one',
  });
  required(p.phoneNumber, { message: 'Phone number is required' });
  pattern(p.phoneNumber, new RegExp(environment.phoneNumberRegex), {
    message: 'The phone number should be a valid one',
  });
  required(p.gender, { message: 'Gender is required' });
  required(p.birthDate, { message: 'Birth date is required' });
  required(p.country, { message: 'Country is required' });
  required(p.county, { message: 'County is required' });
  required(p.city, { message: 'City is required' });
  required(p.street, { message: 'Street is required' });
  required(p.streetNumber, { message: 'Street number is required' });
  required(p.postalCode, { message: 'Postal code is required' });
  required(p.hireDate, { message: 'Hire date is required' });
  required(p.officeId, { message: 'Office is required' });
  required(p.departmentId, { message: 'Department is required' });
  required(p.costCenterId, { message: 'Cost center is required' });
});

export function toDateInputValue(date: string): string {
  const [year, month, day] = date.split('-');
  if (!year || !month || !day) return '';
  return `${year.padStart(4, '0')}-${month.padStart(2, '0')}-${day.padStart(2, '0')}`;
}

export function toFormModel(employee: Employee): EmployeeFormModel {
  return {
    firstName: employee.firstName,
    lastName: employee.lastName,
    email: employee.email,
    phoneNumber: employee.phoneNumber,
    gender: employee.gender.toString(),
    birthDate: toDateInputValue(employee.birthDate ?? ''),
    country: employee.address.country,
    county: employee.address.county,
    city: employee.address.city,
    street: employee.address.street,
    streetNumber: employee.address.streetNumber,
    postalCode: employee.address.postalCode,
    hireDate: toDateInputValue(employee.hireDate ?? ''),
    officeId: employee.officeId ?? '',
    departmentId: employee.departmentId ?? '',
    costCenterId: employee.costCenterId ?? '',
  };
}

export function toCreateEmployeeRequest(
  model: EmployeeFormModel,
): CreateEmployeeRequest {
  return {
    firstName: model.firstName,
    lastName: model.lastName,
    email: model.email,
    phoneNumber: model.phoneNumber,
    gender: Number(model.gender) as Gender,
    birthDate: model.birthDate || undefined,
    address: {
      country: model.country,
      county: model.county,
      city: model.city,
      street: model.street,
      streetNumber: model.streetNumber,
      postalCode: model.postalCode,
    },
    hireDate: model.hireDate || undefined,
    officeId: model.officeId || undefined,
    departmentId: model.departmentId || undefined,
    costCenterId: model.costCenterId || undefined,
  };
}

export function applyFormModel(
  model: EmployeeFormModel,
  current: Employee,
): Employee {
  return { ...current, ...toCreateEmployeeRequest(model) };
}

export function isEmployeeFormDirty(
  model: EmployeeFormModel,
  baseline: EmployeeFormModel,
): boolean {
  return (Object.keys(model) as (keyof EmployeeFormModel)[]).some(
    (key) => model[key] !== baseline[key],
  );
}
