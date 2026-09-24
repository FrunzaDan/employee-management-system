import { EmployeeStatus } from '../interfaces/employee';

// The one place an employee status code becomes its label: the employee list, the
// details page and the organization pages' employee lists all render it.
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
