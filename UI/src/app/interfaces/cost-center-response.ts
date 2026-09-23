// employeeCount/totalGrossSalary are aggregates only the list endpoint computes; the
// single-cost-center endpoint omits them.
export interface CostCenter {
  costCenterId: string;
  code: string;
  name?: string;
  employeeCount?: number;
  totalGrossSalary?: number;
}
