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
  // Optional in the DB; the API omits it when there are none.
  details?: string;
  occurredAt: IsoDateTime;
}
