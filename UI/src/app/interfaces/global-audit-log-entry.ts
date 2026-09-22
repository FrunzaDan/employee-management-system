export interface GlobalAuditLogEntry {
  auditId: number;
  employeeGuid: string;
  // Null when the employee no longer exists (the API LEFT JOINs tbl_employees,
  // since audit history outlives a deleted employee).
  employeeFirstName: string | null;
  employeeLastName: string | null;
  employerId: string;
  action: string;
  details: string;
  actionDate: string;
}
