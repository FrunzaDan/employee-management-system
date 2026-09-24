import { AuditAction } from './audit-log-entry';
import { IsoDateTime } from './iso-date';

export interface GlobalAuditLogEntry {
  employeeAuditLogId: number;
  employeeId: string;
  employeeFirstName: string | null;
  employeeLastName: string | null;
  performedBy: string;
  actionType: AuditAction;
  details: string | null;
  occurredAt: IsoDateTime;
}
