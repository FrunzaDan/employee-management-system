import { GenericResponse } from './generic-response';

export interface Employee {
  guid: string;
  firstName: string;
  lastName: string;
  msisdn: string;
  email: string;
  gender: number;
  employeeStatus: number;
  creationDate: string;
  interactionDate: string;
  birthdate: string;
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
  country: string;
  county: string;
  town: string;
  zip: string;
  street: string;
  number: string;
}

export interface EmployeeResponse extends GenericResponse<EmployeeResponse> {}

export enum EmployeeActivationStatus {
  Active = 1901,
  Deactivated = 1903,
  Test = 1904,
}
