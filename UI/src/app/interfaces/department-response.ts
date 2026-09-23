// employeeCount/totalGrossSalary are aggregates only the list endpoint computes; the
// single-department endpoint omits them.
export interface Department {
  departmentId: string;
  name: string;
  employeeCount?: number;
  totalGrossSalary?: number;
}
