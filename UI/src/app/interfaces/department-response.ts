// employeeCount/totalGrossSalary are aggregates only the list endpoint computes; the
// single-department endpoint returns them as null.
export interface Department {
  departmentId: string;
  name: string;
  employeeCount: number | null;
  totalGrossSalary: number | null;
}
