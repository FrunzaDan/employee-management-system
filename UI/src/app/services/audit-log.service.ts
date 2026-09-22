import { HttpErrorResponse, httpResource } from '@angular/common/http';
import { computed, inject, Injectable, signal } from '@angular/core';
import { environment } from '../../environments/environment';
import { AuditLogEntry } from '../interfaces/audit-log-entry';
import { GenericResponse } from '../interfaces/generic-response';
import { extractErrorMessage } from '../utils/extract-error-message';
import { HttpHeaderService } from './http-header-service';

@Injectable({
  providedIn: 'root',
})
export class AuditLogService {
  private readonly API_URL = `${environment.EmployeeManagementSystemAPI}/api/Employee/auditLog`;
  private readonly httpHeaderService = inject(HttpHeaderService);

  private readonly employeeGuid = signal<string | undefined>(undefined);

  // Declarative fetch: the request is a function of `employeeGuid`, so a new
  // guid cancels the in-flight request and starts another, and no request is
  // made at all until a guid has been set (returning undefined idles it).
  private readonly auditLog = httpResource<GenericResponse<AuditLogEntry[]>>(
    () => {
      const guid = this.employeeGuid();
      if (!guid) return undefined;
      return {
        url: this.API_URL,
        params: { employeeGuid: guid },
        headers: this.httpHeaderService.getHeadersWithTokenSet(),
      };
    },
  );

  // hasValue() guards the read: value() throws while the resource is in error.
  public readonly entriesSignal = computed(() =>
    this.auditLog.hasValue() ? (this.auditLog.value().data ?? []) : [],
  );
  public readonly loadingSignal = this.auditLog.isLoading;
  public readonly errorSignal = computed(() => {
    const error = this.auditLog.error();
    return error ? extractErrorMessage(error as HttpErrorResponse) : null;
  });

  loadAuditLog(employeeGuid: string): void {
    if (this.employeeGuid() === employeeGuid) {
      // Same employee (e.g. after a deactivate/reactivate) — the request itself
      // hasn't changed, so ask for a fresh copy.
      this.auditLog.reload();
    } else {
      this.employeeGuid.set(employeeGuid);
    }
  }
}
