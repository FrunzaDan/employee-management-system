import { EmployeeStatus } from './employee';

export interface EmployeeSummary {
  employeeId: string;
  firstName: string;
  lastName: string;
  email: string;
  status: EmployeeStatus;
}
