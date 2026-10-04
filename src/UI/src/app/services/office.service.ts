import {
  HttpClient,
  HttpErrorResponse,
  HttpParams,
  httpResource,
} from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { Observable, map, tap } from 'rxjs';
import { environment } from '../../environments/environment';
import { GenericResponse } from '../interfaces/generic-response';
import { Office } from '../interfaces/office';
import { EmployeeSummary } from '../interfaces/employee-summary';
import { extractErrorMessage } from '../utils/extract-error-message';
import { NotificationService } from './notification.service';

@Injectable({
  providedIn: 'root',
})
export class OfficeService {
  private readonly apiUrl = `${environment.apiUrl}/api/office`;

  private readonly http = inject(HttpClient);
  private readonly notificationService = inject(NotificationService);

  private readonly requested = signal(false);

  private readonly officesResource = httpResource<GenericResponse<Office[]>>(
    () => (this.requested() ? `${this.apiUrl}/all` : undefined),
  );

  readonly offices = computed(() =>
    this.officesResource.hasValue()
      ? (this.officesResource.value().data ?? [])
      : [],
  );
  readonly loading = this.officesResource.isLoading;
  readonly error = computed(() => {
    const error = this.officesResource.error();
    return error
      ? extractErrorMessage(
          error as HttpErrorResponse,
          'Failed to load offices',
        )
      : null;
  });

  loadOffices(): void {
    if (this.requested()) {
      this.officesResource.reload();
    } else {
      this.requested.set(true);
    }
  }

  fetchOffices(): Observable<Office[]> {
    return this.http
      .get<GenericResponse<Office[]>>(`${this.apiUrl}/all`)
      .pipe(map((response) => response.data ?? []));
  }

  getOffice(officeId: string): Observable<Office> {
    const params = new HttpParams().set('officeId', officeId);
    return this.http
      .get<GenericResponse<Office>>(`${this.apiUrl}/get`, { params })
      .pipe(
        map((response) => {
          if (!response.data) throw new Error('Office not found.');
          return response.data;
        }),
      );
  }

  getEmployees(officeId: string): Observable<EmployeeSummary[]> {
    const params = new HttpParams().set('officeId', officeId);
    return this.http
      .get<GenericResponse<EmployeeSummary[]>>(`${this.apiUrl}/employees`, {
        params,
      })
      .pipe(map((response) => response.data ?? []));
  }

  createOffice(office: Partial<Office>): Observable<GenericResponse<object>> {
    return this.http
      .post<GenericResponse<object>>(`${this.apiUrl}/create`, office)
      .pipe(
        tap(() => {
          this.notificationService.show('Office added successfully.');
          this.loadOffices();
        }),
      );
  }

  updateOffice(office: Partial<Office>): Observable<GenericResponse<object>> {
    return this.http
      .patch<GenericResponse<object>>(`${this.apiUrl}/update`, office)
      .pipe(
        tap(() => {
          this.notificationService.show('Office updated successfully.');
          this.loadOffices();
        }),
      );
  }

  deleteOffice(officeId: string): Observable<GenericResponse<object>> {
    const params = new HttpParams().set('officeId', officeId);
    return this.http
      .delete<GenericResponse<object>>(`${this.apiUrl}/delete`, { params })
      .pipe(
        tap(() => {
          this.notificationService.show('Office deleted successfully.');
          this.loadOffices();
        }),
      );
  }
}
