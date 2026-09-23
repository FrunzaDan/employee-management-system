import { IsoDateTime } from './iso-date';

// EmployeeAuditLog.ActionType — serialized by the API by name.
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
  // NULL in the DB when there are none, sent as null.
  details: string | null;
  occurredAt: IsoDateTime;
}
