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
  private readonly apiUrl = `${environment.apiUrl}/api/employee/audit-log`;

  private readonly employeeId = signal<string | undefined>(undefined);

  // Declarative fetch: the request is a function of `employeeId`, so a new
  // employeeId cancels the in-flight request and starts another, and no request is
  // made at all until a employeeId has been set (returning undefined idles it).
  private readonly auditLogResource = httpResource<
    GenericResponse<AuditLogEntry[]>
  >(() => {
    const employeeId = this.employeeId();
    if (!employeeId) return undefined;
    return {
      url: this.apiUrl,
      params: { employeeId: employeeId },
    };
  });

  // hasValue() guards the read: value() throws while the resource is in error.
  readonly entries = computed(() =>
    this.auditLogResource.hasValue()
      ? (this.auditLogResource.value().data ?? [])
      : [],
  );
  readonly loading = this.auditLogResource.isLoading;
  readonly error = computed(() => {
    const error = this.auditLogResource.error();
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
      this.auditLogResource.reload();
    } else {
      this.employeeId.set(employeeId);
    }
  }
}
