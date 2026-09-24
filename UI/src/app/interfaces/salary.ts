import { IsoDate, IsoDateTime } from './iso-date';

// One EmployeeSalary row: the salary history is append-only.
export interface Salary {
  employeeSalaryId: number;
  employeeId: string;
  grossSalary: number;
  effectiveDate: IsoDate;
  createdAt: IsoDateTime;
}

// POST /api/employee/salary-history. No employeeSalaryId or createdAt: the DB sets both.
export type CreateSalaryRequest = Pick<
  Salary,
  'employeeId' | 'grossSalary' | 'effectiveDate'
>;
