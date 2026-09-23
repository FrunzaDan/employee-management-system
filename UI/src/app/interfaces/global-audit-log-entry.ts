import { AuditAction } from './audit-log-entry';
import { IsoDateTime } from './iso-date';

export interface GlobalAuditLogEntry {
  employeeAuditLogId: number;
  employeeId: string;
  // Absent when the employee no longer exists (the API LEFT JOINs Employee, since audit
  // history outlives a deleted employee, and omits null properties).
  employeeFirstName?: string;
  employeeLastName?: string;
  performedBy: string;
  actionType: AuditAction;
  details?: string;
  occurredAt: IsoDateTime;
}
