import { AuditAction } from './audit-log-entry';
import { IsoDateTime } from './iso-date';

export interface GlobalAuditLogEntry {
  employeeAuditLogId: number;
  employeeId: string;
  // null when the employee no longer exists (the API LEFT JOINs Employee, since audit
  // history outlives a deleted employee).
  employeeFirstName: string | null;
  employeeLastName: string | null;
  performedBy: string;
  actionType: AuditAction;
  details: string | null;
  occurredAt: IsoDateTime;
}
