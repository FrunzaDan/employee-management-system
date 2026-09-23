import { IsoDate, IsoDateTime } from './iso-date';

// Employee.Gender — serialized by the API as its number.
export enum Gender {
  NotDeclared = 0,
  Male = 1,
  Female = 2,
}

// Employee.StatusCode values — see ai_docs/database.md.
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

// An employee as the API returns it. Optionality mirrors the DB's nullability: birth date and
// the job-info fields are the only NULLable columns, and an unset one arrives as null.
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

// POST /api/employee/create. No employeeId: the DB generates it and the response returns it.
export interface CreateEmployeeRequest {
  firstName: string;
  lastName: string;
  email: string;
  phoneNumber: string;
  gender: Gender;
  birthDate?: IsoDate;
  // Omitted = Active. The only other value the API accepts is Test.
  status?: EmployeeStatus.Active | EmployeeStatus.Test;
  address: Address;
  hireDate?: IsoDate;
  officeId?: string;
  departmentId?: string;
  costCenterId?: string;
}

// PATCH /api/employee/update — a partial update: an omitted field is left unchanged. There's no
// status: status only changes through deactivate/reactivate/delete.
export interface UpdateEmployeeRequest extends Partial<
  Omit<CreateEmployeeRequest, 'status'>
> {
  employeeId: string;
}
