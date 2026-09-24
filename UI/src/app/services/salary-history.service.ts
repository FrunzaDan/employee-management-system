import {
  HttpClient,
  HttpErrorResponse,
  httpResource,
} from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { Observable, tap } from 'rxjs';
import { environment } from '../../environments/environment';
import { GenericResponse } from '../interfaces/generic-response';
import { CreateSalaryRequest, Salary } from '../interfaces/salary';
import { extractErrorMessage } from '../utils/extract-error-message';
import { NotificationService } from './notification.service';

@Injectable({
  providedIn: 'root',
})
export class SalaryHistoryService {
  private readonly apiUrl = `${environment.apiUrl}/api/employee/salary-history`;

  private readonly http = inject(HttpClient);
  private readonly notificationService = inject(NotificationService);

  private readonly employeeId = signal<string | undefined>(undefined);

  // Same shape as AuditLogService: the request is a function of `employeeId`, so a
  // new employeeId cancels the in-flight request, and nothing is fetched until one is set.
  private readonly salaryHistoryResource = httpResource<
    GenericResponse<Salary[]>
  >(() => {
    const employeeId = this.employeeId();
    if (!employeeId) return undefined;
    return { url: this.apiUrl, params: { employeeId } };
  });

  // hasValue() guards the read: value() throws while the resource is in error.
  readonly entries = computed(() =>
    this.salaryHistoryResource.hasValue()
      ? (this.salaryHistoryResource.value().data ?? [])
      : [],
  );
  readonly loading = this.salaryHistoryResource.isLoading;
  readonly error = computed(() => {
    const error = this.salaryHistoryResource.error();
    return error
      ? extractErrorMessage(
          error as HttpErrorResponse,
          'Failed to load salary history',
        )
      : null;
  });

  loadSalaryHistory(employeeId: string): void {
    if (this.employeeId() === employeeId) {
      // Same employee (e.g. right after adding an entry) — the request itself
      // hasn't changed, so ask for a fresh copy.
      this.salaryHistoryResource.reload();
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
        this.loadSalaryHistory(entry.employeeId);
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
    return this.http.post<GenericResponse<object>>(this.apiUrl, entry);
  }
}
