import { pattern, required, schema } from '@angular/forms/signals';
import { environment } from '../../../environments/environment';
import { Employee } from '../../interfaces/employee-response';

// Shared by add-employee and edit-employee: one model shape, one validation
// schema, and the two-way mapping between the form and the API's Employee.
export interface EmployeeFormModel {
  firstName: string;
  lastName: string;
  email: string;
  msisdn: string;
  gender: string; // <select> emits strings; the API wants an integer (see toEmployee)
  birthdate: string;
  country: string;
  county: string;
  town: string;
  street: string;
  number: string;
  zip: string;
  hireDate: string;
  officeGuid: string;
  departmentGuid: string;
  costCenterGuid: string;
}

export const emptyEmployeeForm = (): EmployeeFormModel => ({
  firstName: '',
  lastName: '',
  email: '',
  msisdn: '',
  gender: '',
  birthdate: '',
  country: '',
  county: '',
  town: '',
  street: '',
  number: '',
  zip: '',
  hireDate: '',
  officeGuid: '',
  departmentGuid: '',
  costCenterGuid: '',
});

export const employeeFormSchema = schema<EmployeeFormModel>((p) => {
  required(p.firstName, { message: 'First Name is required' });
  required(p.lastName, { message: 'Last Name is required' });
  required(p.email, { message: 'Email is required' });
  pattern(p.email, new RegExp(environment.EmailRegex), {
    message: 'The Email should be a valid one',
  });
  required(p.msisdn, { message: 'Phone Number is required' });
  pattern(p.msisdn, new RegExp(environment.PhoneRegex), {
    message: 'The phone number should be a valid one',
  });
  required(p.gender, { message: 'Gender is required' });
  required(p.birthdate, { message: 'Birthdate is required' });
  required(p.country, { message: 'Country is required' });
  required(p.county, { message: 'County is required' });
  required(p.town, { message: 'Town is required' });
  required(p.street, { message: 'Street is required' });
  required(p.number, { message: 'Street number is required' });
  required(p.zip, { message: 'Zip code is required' });
  required(p.hireDate, { message: 'Hire date is required' });
  required(p.officeGuid, { message: 'Office is required' });
  required(p.departmentGuid, { message: 'Department is required' });
  required(p.costCenterGuid, { message: 'Cost center is required' });
});

// <input type="date"> requires a strictly zero-padded "YYYY-MM-DD" value to
// pre-fill correctly. Older records saved via the previous year/month/day
// text-box form could store unpadded values (e.g. "2020-1-5"), so normalize.
export function toDateInputValue(birthdate: string): string {
  const [year, month, day] = birthdate.split('-');
  if (!year || !month || !day) return '';
  return `${year.padStart(4, '0')}-${month.padStart(2, '0')}-${day.padStart(2, '0')}`;
}

export function toFormModel(employee: Employee): EmployeeFormModel {
  return {
    firstName: employee.firstName,
    lastName: employee.lastName,
    email: employee.email,
    msisdn: employee.msisdn,
    gender: employee.gender?.toString() ?? '',
    birthdate: toDateInputValue(employee.birthdate),
    country: employee.address.country,
    county: employee.address.county,
    town: employee.address.town,
    street: employee.address.street,
    number: employee.address.number,
    zip: employee.address.zip,
    hireDate: toDateInputValue(employee.hireDate ?? ''),
    officeGuid: employee.officeGuid ?? '',
    departmentGuid: employee.departmentGuid ?? '',
    costCenterGuid: employee.costCenterGuid ?? '',
  };
}

// `base` carries the server-owned fields (guid, status, dates) when editing;
// for a new employee they're simply absent and the server generates them.
export function toEmployee(
  model: EmployeeFormModel,
  base: Partial<Employee> = {},
): Employee {
  return {
    ...base,
    firstName: model.firstName,
    lastName: model.lastName,
    email: model.email,
    msisdn: model.msisdn,
    gender: Number(model.gender),
    birthdate: model.birthdate,
    address: {
      country: model.country,
      county: model.county,
      town: model.town,
      street: model.street,
      number: model.number,
      zip: model.zip,
    },
    hireDate: model.hireDate,
    officeGuid: model.officeGuid,
    departmentGuid: model.departmentGuid,
    costCenterGuid: model.costCenterGuid,
  } as Employee;
}

// True when the user has changed anything relative to `baseline` (the blank
// form when adding, the loaded employee when editing). Comparing values —
// rather than trusting a "touched" flag — means typing something and then
// putting it back doesn't count as an unsaved change.
export function isEmployeeFormDirty(
  model: EmployeeFormModel,
  baseline: EmployeeFormModel,
): boolean {
  return (Object.keys(model) as (keyof EmployeeFormModel)[]).some(
    (key) => model[key] !== baseline[key],
  );
}
