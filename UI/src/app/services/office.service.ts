import { HttpClient, HttpErrorResponse, HttpParams } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { Observable, map, tap } from 'rxjs';
import { environment } from '../../environments/environment';
import { GenericResponse } from '../interfaces/generic-response';
import { Office } from '../interfaces/office-response';
import { EmployeeSummary } from '../interfaces/employee-summary-response';
import { HttpHeaderService } from './http-header.service';
import { NotificationService } from './notification.service';

// Reference data (offices), not a growing operational table — a flat, unpaginated
// list, unlike GetEmployeeService's paged state. Mutations reload the list rather
// than patching it locally, since the whole list is always small.
@Injectable({
  providedIn: 'root',
})
export class OfficeService {
  private readonly API_URL = `${environment.apiUrl}/api/office`;

  private readonly http = inject(HttpClient);
  private readonly httpHeaderService = inject(HttpHeaderService);
  private readonly notificationService = inject(NotificationService);

  private readonly state = signal({
    offices: [] as Office[],
    loading: false,
    error: null as string | null,
  });

  readonly offices = computed(() => this.state().offices);
  readonly loading = computed(() => this.state().loading);
  readonly error = computed(() => this.state().error);

  loadOffices(): void {
    this.state.update((s) => ({ ...s, loading: true, error: null }));
    const headers = this.httpHeaderService.getHeadersWithTokenSet();

    this.http
      .get<GenericResponse<Office[]>>(`${this.API_URL}/all`, { headers })
      .subscribe({
        next: (response) =>
          this.state.set({ offices: response.data ?? [], loading: false, error: null }),
        error: (error: HttpErrorResponse) =>
          this.state.set({
            offices: [],
            loading: false,
            error: error.error?.message || 'Failed to load offices.',
          }),
      });
  }

  /**
   * Same list as {@link loadOffices}, but as a one-off Observable rather than
   * updating this service's signal state — for callers (e.g. bulk test-data
   * generation) that need the list once, up front, without touching the page
   * that's actually browsing/managing offices.
   */
  fetchOfficesOnce(): Observable<Office[]> {
    const headers = this.httpHeaderService.getHeadersWithTokenSet();
    return this.http
      .get<GenericResponse<Office[]>>(`${this.API_URL}/all`, { headers })
      .pipe(map((response) => response.data ?? []));
  }

  /** A single office by officeId — for the office details page, reached directly by URL. */
  getOffice(officeId: string): Observable<Office> {
    const headers = this.httpHeaderService.getHeadersWithTokenSet();
    const params = new HttpParams().set('officeId', officeId);
    return this.http
      .get<GenericResponse<Office>>(`${this.API_URL}/get`, { headers, params })
      .pipe(
        map((response) => {
          if (!response.data) throw new Error('Office not found.');
          return response.data;
        }),
      );
  }

  /** The employees currently assigned to this office (see Employee_ListByOffice). */
  getEmployees(officeId: string): Observable<EmployeeSummary[]> {
    const headers = this.httpHeaderService.getHeadersWithTokenSet();
    const params = new HttpParams().set('officeId', officeId);
    return this.http
      .get<GenericResponse<EmployeeSummary[]>>(`${this.API_URL}/employees`, { headers, params })
      .pipe(map((response) => response.data ?? []));
  }

  createOffice(office: Partial<Office>): Observable<GenericResponse<object>> {
    const headers = this.httpHeaderService.getHeadersWithTokenSet();
    return this.http
      .post<GenericResponse<object>>(`${this.API_URL}/create`, office, { headers })
      .pipe(
        tap(() => {
          this.notificationService.show('Office created successfully.');
          this.loadOffices();
        }),
      );
  }

  updateOffice(office: Partial<Office>): Observable<GenericResponse<object>> {
    const headers = this.httpHeaderService.getHeadersWithTokenSet();
    return this.http
      .patch<GenericResponse<object>>(`${this.API_URL}/update`, office, { headers })
      .pipe(
        tap(() => {
          this.notificationService.show('Office updated successfully.');
          this.loadOffices();
        }),
      );
  }

  deleteOffice(officeId: string): Observable<GenericResponse<object>> {
    const headers = this.httpHeaderService.getHeadersWithTokenSet();
    const params = new HttpParams().set('officeId', officeId);
    return this.http
      .delete<GenericResponse<object>>(`${this.API_URL}/delete`, { headers, params })
      .pipe(
        tap(() => {
          this.notificationService.show('Office deleted successfully.');
          this.loadOffices();
        }),
      );
  }
}
