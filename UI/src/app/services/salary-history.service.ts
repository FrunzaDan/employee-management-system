import {
  HttpClient,
  HttpErrorResponse,
  httpResource,
} from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { Observable, tap } from 'rxjs';
import { environment } from '../../environments/environment';
import { GenericResponse } from '../interfaces/generic-response';
import {
  CreateSalaryRequest,
  SalaryHistoryEntry,
} from '../interfaces/salary-history';
import { extractErrorMessage } from '../utils/extract-error-message';
import { NotificationService } from './notification.service';

@Injectable({
  providedIn: 'root',
})
export class SalaryHistoryService {
  private readonly API_URL = `${environment.apiUrl}/api/employee/salary-history`;

  private readonly http = inject(HttpClient);
  private readonly notificationService = inject(NotificationService);

  private readonly employeeId = signal<string | undefined>(undefined);

  // Same shape as AuditLogService: the request is a function of `employeeId`, so a
  // new employeeId cancels the in-flight request, and nothing is fetched until one is set.
  private readonly historyResource = httpResource<
    GenericResponse<SalaryHistoryEntry[]>
  >(() => {
    const employeeId = this.employeeId();
    if (!employeeId) return undefined;
    return { url: this.API_URL, params: { employeeId } };
  });

  // hasValue() guards the read: value() throws while the resource is in error.
  readonly entries = computed(() =>
    this.historyResource.hasValue()
      ? (this.historyResource.value().data ?? [])
      : [],
  );
  readonly loading = this.historyResource.isLoading;
  readonly error = computed(() => {
    const error = this.historyResource.error();
    return error
      ? extractErrorMessage(
          error as HttpErrorResponse,
          'Failed to load salary history',
        )
      : null;
  });

  loadHistory(employeeId: string): void {
    if (this.employeeId() === employeeId) {
      // Same employee (e.g. right after adding an entry) — the request itself
      // hasn't changed, so ask for a fresh copy.
      this.historyResource.reload();
    } else {
      this.employeeId.set(employeeId);
    }
  }

  createSalary(
    entry: CreateSalaryRequest,
  ): Observable<GenericResponse<object>> {
    return this.createSalarySilently(entry).pipe(
      tap(() => {
        this.notificationService.show('Salary entry added successfully.');
        this.loadHistory(entry.employeeId);
      }),
    );
  }

  /**
   * Same endpoint as {@link createSalary}, without the per-call success toast or
   * history reload — for bulk callers (test-data generation) adding many
   * entries for employees whose history page isn't even open.
   */
  createSalarySilently(
    entry: CreateSalaryRequest,
  ): Observable<GenericResponse<object>> {
    return this.http.post<GenericResponse<object>>(this.API_URL, entry);
  }
}
