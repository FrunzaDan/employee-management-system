// employeeCount/totalGrossSalary are aggregates only the list endpoint computes; the
// single-office endpoint returns them as null.
export interface Office {
  officeId: string;
  name: string;
  city: string | null;
  country: string | null;
  employeeCount: number | null;
  totalGrossSalary: number | null;
}
