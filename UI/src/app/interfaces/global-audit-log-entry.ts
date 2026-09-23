export interface GlobalAuditLogEntry {
  auditId: number;
  employeeGuid: string;
  // Absent when the employee no longer exists (the API LEFT JOINs Employee,
  // since audit history outlives a deleted employee, and omits null properties).
  employeeFirstName?: string;
  employeeLastName?: string;
  employerId: string;
  action: string;
  details?: string;
  actionDate: string; // UTC, ISO 8601
}
