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

// Reference data (offices, departments, cost centers), not a growing operational
// table — a flat, unpaginated list, unlike EmployeeService's paged state. The list
// is an httpResource (like ProductService in the customer app): nothing is fetched
// until loadOffices() is first called, and mutations reload it rather than patching
// it locally, since the whole list is always small.
@Injectable({
  providedIn: 'root',
})
export class OfficeService {
  private readonly API_URL = `${environment.apiUrl}/api/office`;

  private readonly http = inject(HttpClient);
  private readonly notificationService = inject(NotificationService);

  private readonly requested = signal(false);

  private readonly officesResource = httpResource<GenericResponse<Office[]>>(
    () => (this.requested() ? `${this.API_URL}/all` : undefined),
  );

  // hasValue() guards the read: value() throws while the resource is in error.
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

  /**
   * Same list as {@link loadOffices}, but as a one-off Observable rather than
   * this service's resource — for callers (e.g. bulk test-data generation) that
   * need the list once, up front, without touching the page that's actually
   * browsing/managing offices.
   */
  fetchOffices(): Observable<Office[]> {
    return this.http
      .get<GenericResponse<Office[]>>(`${this.API_URL}/all`)
      .pipe(map((response) => response.data ?? []));
  }

  /** A single office by officeId — for the office details page, reached directly by URL. */
  getOffice(officeId: string): Observable<Office> {
    const params = new HttpParams().set('officeId', officeId);
    return this.http
      .get<GenericResponse<Office>>(`${this.API_URL}/get`, { params })
      .pipe(
        map((response) => {
          if (!response.data) throw new Error('Office not found.');
          return response.data;
        }),
      );
  }

  /** The employees currently assigned to this office (see Employee_ListByOffice). */
  getEmployees(officeId: string): Observable<EmployeeSummary[]> {
    const params = new HttpParams().set('officeId', officeId);
    return this.http
      .get<GenericResponse<EmployeeSummary[]>>(`${this.API_URL}/employees`, {
        params,
      })
      .pipe(map((response) => response.data ?? []));
  }

  createOffice(office: Partial<Office>): Observable<GenericResponse<object>> {
    return this.http
      .post<GenericResponse<object>>(`${this.API_URL}/create`, office)
      .pipe(
        tap(() => {
          this.notificationService.show('Office created successfully.');
          this.loadOffices();
        }),
      );
  }

  updateOffice(office: Partial<Office>): Observable<GenericResponse<object>> {
    return this.http
      .patch<GenericResponse<object>>(`${this.API_URL}/update`, office)
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
      .delete<GenericResponse<object>>(`${this.API_URL}/delete`, { params })
      .pipe(
        tap(() => {
          this.notificationService.show('Office deleted successfully.');
          this.loadOffices();
        }),
      );
  }
}
