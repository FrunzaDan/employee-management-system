export interface SalaryHistoryEntry {
  salaryGuid: string;
  employeeGuid: string;
  bruttoSalary: number;
  effectiveDate: string; // calendar date, "YYYY-MM-DD"
  createdDate: string; // UTC, ISO 8601
}
