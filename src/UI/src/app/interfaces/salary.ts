import { IsoDate, IsoDateTime } from './iso-date';

export interface Salary {
  employeeSalaryId: number;
  employeeId: string;
  grossSalary: number;
  effectiveDate: IsoDate;
  createdAt: IsoDateTime;
}

export type CreateSalaryRequest = Pick<
  Salary,
  'employeeId' | 'grossSalary' | 'effectiveDate'
>;
