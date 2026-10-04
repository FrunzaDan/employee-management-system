import { EmployeeStatus } from '../interfaces/employee';

export function employeeStatusLabel(status: EmployeeStatus): string {
  switch (status) {
    case EmployeeStatus.Active:
      return 'Active';
    case EmployeeStatus.Deactivated:
      return 'Deactivated';
    case EmployeeStatus.Test:
      return 'Test';
    default:
      return 'Unknown';
  }
}
