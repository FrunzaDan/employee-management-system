export interface AuditLogEntry {
  auditId: number;
  employeeGuid: string;
  employerId: string;
  action: string;
  details?: string;
  actionDate: string; // UTC, ISO 8601
}
