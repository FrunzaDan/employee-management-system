import { EmployeeActivationStatus } from '../interfaces/employee-response';

// Shared by any page that renders an EmployeeSummary/Employee status badge
// outside the main employee list (which keeps its own Map for historical
// reasons) — e.g. the organization admin pages' "employees in this office/
// department/cost center" lists.
export function employeeStatusLabel(status: number): string {
  switch (status) {
    case EmployeeActivationStatus.Active:
      return 'Active';
    case EmployeeActivationStatus.Deactivated:
      return 'Deactivated';
    case EmployeeActivationStatus.Test:
      return 'Test';
    default:
      return 'Unknown';
  }
}
