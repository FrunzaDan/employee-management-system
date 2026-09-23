import { GenericResponse } from './generic-response';

// Property optionality mirrors the DB's nullability: the API serializes with
// NullValueHandling.Ignore, so a NULL column arrives as an *absent* property
// (undefined), never as `null`.
//
// Dates: `birthdate`/`hireDate` are calendar dates ("YYYY-MM-DD", no time zone);
// `creationDate`/`interactionDate` are UTC instants in ISO 8601 with a trailing
// "Z" — render them with the `date` pipe to show local time. JSON has no date
// type, so both stay strings here.
export interface Employee {
  guid: string;
  firstName: string;
  lastName: string;
  msisdn: string;
  email: string;
  gender?: Gender;
  employeeStatus: EmployeeActivationStatus;
  creationDate: string;
  interactionDate: string;
  birthdate?: string;
  address: Address;
  hireDate?: string;
  officeGuid?: string;
  officeName?: string;
  departmentGuid?: string;
  departmentName?: string;
  costCenterGuid?: string;
  costCenterName?: string;
  currentBruttoSalary?: number;
}

export interface Address {
  country?: string;
  county?: string;
  town?: string;
  zip?: string;
  street?: string;
  number?: string;
}

export interface EmployeeResponse extends GenericResponse<EmployeeResponse> {}

// Employee.StatusCode values (CHECK-constrained in the DB; the API's
// EmployeeStatus enum).
export enum EmployeeActivationStatus {
  Active = 1901,
  Deactivated = 1903,
  Test = 1904,
}

// Employee.Gender codes (CHECK-constrained in the DB; the API's Gender enum).
export enum Gender {
  NotDeclared = 0,
  Male = 1,
  Female = 2,
}
