import {
  HttpClient,
  HttpErrorResponse,
  HttpHeaders,
  HttpParams,
} from '@angular/common/http';
import { computed, Injectable, signal, inject } from '@angular/core';
import { environment } from '../../environments/environment';
import { extractErrorMessage } from '../utils/extract-error-message';
import { GenericResponse } from '../interfaces/generic-response';
import { GetEmployeeService } from './get-employee.service';
import { HttpHeaderService } from './http-header.service';
import { EmployeeStatus } from '../interfaces/employee-response';
import { Observable, tap, throwError, timer } from 'rxjs';
import { retry } from 'rxjs/internal/operators/retry';
import { catchError } from 'rxjs/internal/operators/catchError';
import { NotificationService } from './notification.service';

interface ActivationState {
  loading: boolean;
  error: string | null;
}

// Only retry transient failures (no response reached the browser, or a 5xx from the
// server) — a definitive 4xx (expired session, already-deactivated, unknown GUID) will
// never succeed on retry, so retrying it just re-triggers side effects (e.g. the 401
// interceptor's logout/redirect) 3 extra times for nothing. Backed off, not immediate.
const TRANSIENT_ERROR_RETRY_CONFIG = {
  count: 3,
  delay: (error: unknown, retryCount: number) =>
    error instanceof HttpErrorResponse && (error.status === 0 || error.status >= 500)
      ? timer(retryCount * 500)
      : throwError(() => error),
};

@Injectable({
  providedIn: 'root',
})
export class ActivateEmployeeService {
  readonly APIURL_DEACTIVATE =
    environment.apiUrl + '/api/employee/deactivate';
  readonly APIURL_REACTIVATE =
    environment.apiUrl + '/api/employee/reactivate';

  private readonly state = signal<ActivationState>({
    loading: false,
    error: null,
  });

  readonly loading = computed(() => this.state().loading);
  readonly error = computed(() => this.state().error);

  private readonly http = inject(HttpClient);
  private readonly httpHeaderService = inject(HttpHeaderService);
  private readonly getEmployeeService = inject(GetEmployeeService);
  private readonly notificationService = inject(NotificationService);

  deactivateEmployee(employeeId: string): void {
    this.setLoading(true);
    const headers: HttpHeaders =
      this.httpHeaderService.getHeadersWithTokenSet();
    const params = new HttpParams().set('employeeId', employeeId);

    this.http
      .patch<GenericResponse<object>>(this.APIURL_DEACTIVATE, null, {
        headers,
        params,
      })
      .pipe(
        retry(TRANSIENT_ERROR_RETRY_CONFIG),
        catchError((error: HttpErrorResponse) => {
          this.handleError(error);
          throw error;
        }),
      )
      .subscribe({
        next: (response) => {
          if (response.status != 200) {
            this.handleError(new Error('Deactivation failed'));
            return;
          }

          const existingEmployee = this.getEmployeeService
            .employees()
            .find((c) => c.employeeId === employeeId);

          if (existingEmployee) {
            this.getEmployeeService.updateEmployeeLocally({
              ...existingEmployee,
              status: EmployeeStatus.Deactivated,
            });
            this.clearError();
            this.notificationService.show('Employee deactivated successfully.');
          } else {
            this.handleError(
              new Error(
                `Employee with GUID ${employeeId} not found locally.`,
              ),
            );
          }
        },
        error: (error: HttpErrorResponse) => this.handleError(error),
      });
  }

  reactivateEmployee(employeeId: string): void {
    this.setLoading(true);
    const headers: HttpHeaders =
      this.httpHeaderService.getHeadersWithTokenSet();
    const params = new HttpParams().set('employeeId', employeeId);

    this.http
      .patch<GenericResponse<object>>(this.APIURL_REACTIVATE, null, {
        headers,
        params,
      })
      .pipe(
        retry(TRANSIENT_ERROR_RETRY_CONFIG),
        catchError((error: HttpErrorResponse) => {
          this.handleError(error);
          throw error;
        }),
      )
      .subscribe({
        next: (response) => {
          if (response.status != 200) {
            this.handleError(new Error('Reactivation failed'));
            return;
          }

          const existingEmployee = this.getEmployeeService
            .employees()
            .find((c) => c.employeeId === employeeId);

          if (existingEmployee) {
            this.getEmployeeService.updateEmployeeLocally({
              ...existingEmployee,
              status: EmployeeStatus.Active,
            });
            this.clearError();
            this.notificationService.show('Employee reactivated successfully.');
          } else {
            this.handleError(
              new Error(
                `Employee with GUID ${employeeId} not found locally.`,
              ),
            );
          }
        },
        error: (error: HttpErrorResponse) => this.handleError(error),
      });
  }

  /**
   * Same endpoint as {@link deactivateEmployee}, without the shared loading/error
   * signal or the per-call success toast — for bulk-action callers that show one
   * summary notification and track their own in-flight state instead.
   */
  deactivateEmployeeSilently(
    employeeId: string,
  ): Observable<GenericResponse<object>> {
    const headers: HttpHeaders =
      this.httpHeaderService.getHeadersWithTokenSet();
    const params = new HttpParams().set('employeeId', employeeId);

    return this.http
      .patch<GenericResponse<object>>(this.APIURL_DEACTIVATE, null, {
        headers,
        params,
      })
      .pipe(
        tap(() => {
          const existingEmployee = this.getEmployeeService
            .employees()
            .find((c) => c.employeeId === employeeId);

          if (existingEmployee) {
            this.getEmployeeService.updateEmployeeLocally({
              ...existingEmployee,
              status: EmployeeStatus.Deactivated,
            });
          }
        }),
      );
  }

  private setLoading(loading: boolean): void {
    this.state.update((state) => ({
      ...state,
      loading,
    }));
  }

  private clearError(): void {
    this.state.update((state) => ({
      ...state,
      loading: false,
      error: null,
    }));
  }

  // An Error (not an HttpErrorResponse) is a request that succeeded at the HTTP
  // level but that the API reported as not done; its message is already user-facing.
  private handleError(error: HttpErrorResponse | Error): void {
    this.state.update((state) => ({
      ...state,
      loading: false,
      error:
        error instanceof HttpErrorResponse
          ? extractErrorMessage(error, 'Failed to update the employee status')
          : error.message,
    }));
  }
}
