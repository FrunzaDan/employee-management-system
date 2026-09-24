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

  private readonly salaryHistoryResource = httpResource<
    GenericResponse<Salary[]>
  >(() => {
    const employeeId = this.employeeId();
    if (!employeeId) return undefined;
    return { url: this.apiUrl, params: { employeeId } };
  });

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

  createSalarySilently(
    entry: CreateSalaryRequest,
  ): Observable<GenericResponse<object>> {
    return this.http.post<GenericResponse<object>>(this.apiUrl, entry);
  }
}
