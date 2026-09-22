export interface AuditLogEntry {
  auditId: number;
  employeeGuid: string;
  employerId: string;
  action: string;
  details: string;
  actionDate: string;
}
