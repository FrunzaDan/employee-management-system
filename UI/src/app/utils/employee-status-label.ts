import { EmployeeStatus } from '../interfaces/employee-response';

// Shared by any page that renders an EmployeeSummary/Employee status badge
// outside the main employee list (which keeps its own Map for historical
// reasons) — e.g. the organization admin pages' "employees in this office/
// department/cost center" lists.
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
