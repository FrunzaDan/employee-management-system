// employeeCount/totalGrossSalary are aggregates only the list endpoint computes; the
// single-cost-center endpoint returns them as null.
export interface CostCenter {
  costCenterId: string;
  code: string;
  name: string | null;
  employeeCount: number | null;
  totalGrossSalary: number | null;
}
