import { HttpErrorResponse, httpResource } from '@angular/common/http';
import { computed, inject, Injectable, signal } from '@angular/core';
import { environment } from '../../environments/environment';
import { AuditLogEntry } from '../interfaces/audit-log-entry';
import { GenericResponse } from '../interfaces/generic-response';
import { extractErrorMessage } from '../utils/extract-error-message';

@Injectable({
  providedIn: 'root',
})
export class AuditLogService {
  private readonly API_URL = `${environment.apiUrl}/api/employee/audit-log`;

  private readonly employeeId = signal<string | undefined>(undefined);

  // Declarative fetch: the request is a function of `employeeId`, so a new
  // employeeId cancels the in-flight request and starts another, and no request is
  // made at all until a employeeId has been set (returning undefined idles it).
  private readonly auditLog = httpResource<GenericResponse<AuditLogEntry[]>>(
    () => {
      const employeeId = this.employeeId();
      if (!employeeId) return undefined;
      return {
        url: this.API_URL,
        params: { employeeId: employeeId },
      };
    },
  );

  // hasValue() guards the read: value() throws while the resource is in error.
  readonly entries = computed(() =>
    this.auditLog.hasValue() ? (this.auditLog.value().data ?? []) : [],
  );
  readonly loading = this.auditLog.isLoading;
  readonly error = computed(() => {
    const error = this.auditLog.error();
    return error
      ? extractErrorMessage(
          error as HttpErrorResponse,
          'Failed to load the audit trail',
        )
      : null;
  });

  loadAuditLog(employeeId: string): void {
    if (this.employeeId() === employeeId) {
      // Same employee (e.g. after a deactivate/reactivate) — the request itself
      // hasn't changed, so ask for a fresh copy.
      this.auditLog.reload();
    } else {
      this.employeeId.set(employeeId);
    }
  }
}
