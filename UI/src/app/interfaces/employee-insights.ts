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
}

export interface EmployeeInsights {
  employees: EmployeeProfile[];
}
