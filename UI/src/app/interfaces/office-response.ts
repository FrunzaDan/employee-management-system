// employeeCount/totalGrossSalary are aggregates only the list endpoint computes; the
// single-office endpoint omits them.
export interface Office {
  officeId: string;
  name: string;
  city?: string;
  country?: string;
  employeeCount?: number;
  totalGrossSalary?: number;
}
