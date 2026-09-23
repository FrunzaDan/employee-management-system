import { AuditAction } from '../interfaces/audit-log-entry';

// The API sends audit actions by their enum name ("SalaryChanged"); this is how they read in
// the UI ("Salary changed"). One-word names read the same either way.
export function auditActionLabel(action: AuditAction): string {
  return action.replace(/(?<=[a-z])(?=[A-Z])/g, ' ').replace(/ ([A-Z])/g, (_, letter: string) => ` ${letter.toLowerCase()}`);
}
