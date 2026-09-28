import { EmployeeStatus, Gender } from './employee';
import { IsoDate } from './iso-date';

export interface EmployeeProfile {
  status: EmployeeStatus;
  gender: Gender;
  birthDate: IsoDate | null;
  hireDate: IsoDate | null;
  departmentName: string | null;
  officeName: string | null;
  currentGrossSalary: number | null;
  // Oldest first, one entry per date, nothing dated in the future.
  salaryHistory: SalaryPoint[];
}

export interface SalaryPoint {
  effectiveDate: IsoDate;
  grossSalary: number;
}

export interface EmployeeInsights {
  employees: EmployeeProfile[];
}
