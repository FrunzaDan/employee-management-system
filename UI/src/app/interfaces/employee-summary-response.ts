// A minimal projection of an employee, returned by the "employees belonging
// to this office/department/cost center" endpoints — not the full Employee
// shape (no address/hire-date/salary), which those endpoints don't join.
export interface EmployeeSummary {
  guid: string;
  firstName: string;
  lastName: string;
  email: string;
  employeeStatus: number;
}
