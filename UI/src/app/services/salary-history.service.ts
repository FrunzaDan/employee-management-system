import { HttpClient, HttpErrorResponse, HttpParams } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { Observable, tap } from 'rxjs';
import { environment } from '../../environments/environment';
import { GenericResponse } from '../interfaces/generic-response';
import { SalaryHistoryEntry } from '../interfaces/salary-history-response';
import { HttpHeaderService } from './http-header-service';
import { NotificationService } from './notification.service';

@Injectable({
  providedIn: 'root',
})
export class SalaryHistoryService {
  private readonly API_URL = `${environment.EmployeeManagementSystemAPI}/api/Employee/salaryHistory`;

  private readonly http = inject(HttpClient);
  private readonly httpHeaderService = inject(HttpHeaderService);
  private readonly notificationService = inject(NotificationService);

  private readonly state = signal({
    entries: [] as SalaryHistoryEntry[],
    loading: false,
    error: null as string | null,
  });

  readonly entriesSignal = computed(() => this.state().entries);
  readonly loadingSignal = computed(() => this.state().loading);
  readonly errorSignal = computed(() => this.state().error);

  loadHistory(employeeGuid: string): void {
    this.state.update((s) => ({ ...s, loading: true, error: null }));
    const headers = this.httpHeaderService.getHeadersWithTokenSet();
    const params = new HttpParams().set('employeeGuid', employeeGuid);

    this.http
      .get<GenericResponse<SalaryHistoryEntry[]>>(this.API_URL, { headers, params })
      .subscribe({
        next: (response) =>
          this.state.set({ entries: response.data ?? [], loading: false, error: null }),
        error: (error: HttpErrorResponse) =>
          this.state.set({
            entries: [],
            loading: false,
            error: error.error?.message || 'Failed to load salary history.',
          }),
      });
  }

  addSalary(
    entry: Pick<SalaryHistoryEntry, 'employeeGuid' | 'bruttoSalary' | 'effectiveDate'>,
  ): Observable<GenericResponse<object>> {
    const headers = this.httpHeaderService.getHeadersWithTokenSet();
    return this.http
      .post<GenericResponse<object>>(this.API_URL, entry, { headers })
      .pipe(
        tap(() => {
          this.notificationService.show('Salary entry added successfully.');
          this.loadHistory(entry.employeeGuid);
        }),
      );
  }

  /**
   * Same endpoint as {@link addSalary}, without the per-call success toast or
   * history reload — for bulk callers (test-data generation) adding many
   * entries for employees whose history page isn't even open.
   */
  addSalarySilently(
    entry: Pick<SalaryHistoryEntry, 'employeeGuid' | 'bruttoSalary' | 'effectiveDate'>,
  ): Observable<GenericResponse<object>> {
    const headers = this.httpHeaderService.getHeadersWithTokenSet();
    return this.http.post<GenericResponse<object>>(this.API_URL, entry, { headers });
  }
}
