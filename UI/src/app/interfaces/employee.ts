import { IsoDate, IsoDateTime } from './iso-date';

export enum Gender {
  NotDeclared = 0,
  Male = 1,
  Female = 2,
}

export enum EmployeeStatus {
  Active = 1901,
  Deactivated = 1903,
  Test = 1904,
}

export interface Address {
  country: string;
  county: string;
  city: string;
  postalCode: string;
  street: string;
  streetNumber: string;
}

export interface Employee {
  employeeId: string;
  firstName: string;
  lastName: string;
  phoneNumber: string;
  email: string;
  gender: Gender;
  status: EmployeeStatus;
  createdAt: IsoDateTime;
  lastInteractionAt: IsoDateTime;
  birthDate: IsoDate | null;
  address: Address;
  hireDate: IsoDate | null;
  officeId: string | null;
  officeName: string | null;
  departmentId: string | null;
  departmentName: string | null;
  costCenterId: string | null;
  costCenterName: string | null;
  currentGrossSalary: number | null;
}

export interface CreateEmployeeRequest {
  firstName: string;
  lastName: string;
  email: string;
  phoneNumber: string;
  gender: Gender;
  birthDate?: IsoDate;
  status?: EmployeeStatus.Active | EmployeeStatus.Test;
  address: Address;
  hireDate?: IsoDate;
  officeId?: string;
  departmentId?: string;
  costCenterId?: string;
}

export interface UpdateEmployeeRequest extends Partial<
  Omit<CreateEmployeeRequest, 'status'>
> {
  employeeId: string;
}
