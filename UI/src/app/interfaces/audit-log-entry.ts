import { IsoDateTime } from './iso-date';

export type AuditAction =
  | 'Created'
  | 'Edited'
  | 'Deactivated'
  | 'Reactivated'
  | 'Deleted'
  | 'SalaryChanged';

export interface AuditLogEntry {
  employeeAuditLogId: number;
  employeeId: string;
  performedBy: string;
  actionType: AuditAction;
  details: string | null;
  occurredAt: IsoDateTime;
}
