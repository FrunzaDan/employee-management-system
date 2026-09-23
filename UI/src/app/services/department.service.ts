import {
  HttpClient,
  HttpErrorResponse,
  HttpParams,
} from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { Observable, map, tap } from 'rxjs';
import { environment } from '../../environments/environment';
import { GenericResponse } from '../interfaces/generic-response';
import { Department } from '../interfaces/department-response';
import { EmployeeSummary } from '../interfaces/employee-summary-response';
import { HttpHeaderService } from './http-header.service';
import { NotificationService } from './notification.service';

@Injectable({
  providedIn: 'root',
})
export class DepartmentService {
  private readonly API_URL = `${environment.apiUrl}/api/department`;

  private readonly http = inject(HttpClient);
  private readonly httpHeaderService = inject(HttpHeaderService);
  private readonly notificationService = inject(NotificationService);

  private readonly state = signal({
    departments: [] as Department[],
    loading: false,
    error: null as string | null,
  });

  readonly departments = computed(() => this.state().departments);
  readonly loading = computed(() => this.state().loading);
  readonly error = computed(() => this.state().error);

  loadDepartments(): void {
    this.state.update((s) => ({ ...s, loading: true, error: null }));
    const headers = this.httpHeaderService.getHeadersWithTokenSet();

    this.http
      .get<GenericResponse<Department[]>>(`${this.API_URL}/all`, { headers })
      .subscribe({
        next: (response) =>
          this.state.set({
            departments: response.data ?? [],
            loading: false,
            error: null,
          }),
        error: (error: HttpErrorResponse) =>
          this.state.set({
            departments: [],
            loading: false,
            error: error.error?.message || 'Failed to load departments.',
          }),
      });
  }

  /** See OfficeService.fetchOfficesOnce for why this exists alongside loadDepartments. */
  fetchDepartmentsOnce(): Observable<Department[]> {
    const headers = this.httpHeaderService.getHeadersWithTokenSet();
    return this.http
      .get<GenericResponse<Department[]>>(`${this.API_URL}/all`, { headers })
      .pipe(map((response) => response.data ?? []));
  }

  /** A single department by departmentId — for the department details page, reached directly by URL. */
  getDepartment(departmentId: string): Observable<Department> {
    const headers = this.httpHeaderService.getHeadersWithTokenSet();
    const params = new HttpParams().set('departmentId', departmentId);
    return this.http
      .get<GenericResponse<Department>>(`${this.API_URL}/get`, {
        headers,
        params,
      })
      .pipe(
        map((response) => {
          if (!response.data) throw new Error('Department not found.');
          return response.data;
        }),
      );
  }

  /** The employees currently assigned to this department (see Employee_ListByDepartment). */
  getEmployees(departmentId: string): Observable<EmployeeSummary[]> {
    const headers = this.httpHeaderService.getHeadersWithTokenSet();
    const params = new HttpParams().set('departmentId', departmentId);
    return this.http
      .get<GenericResponse<EmployeeSummary[]>>(`${this.API_URL}/employees`, {
        headers,
        params,
      })
      .pipe(map((response) => response.data ?? []));
  }

  createDepartment(
    department: Partial<Department>,
  ): Observable<GenericResponse<object>> {
    const headers = this.httpHeaderService.getHeadersWithTokenSet();
    return this.http
      .post<GenericResponse<object>>(`${this.API_URL}/create`, department, {
        headers,
      })
      .pipe(
        tap(() => {
          this.notificationService.show('Department created successfully.');
          this.loadDepartments();
        }),
      );
  }

  updateDepartment(
    department: Partial<Department>,
  ): Observable<GenericResponse<object>> {
    const headers = this.httpHeaderService.getHeadersWithTokenSet();
    return this.http
      .patch<GenericResponse<object>>(`${this.API_URL}/update`, department, {
        headers,
      })
      .pipe(
        tap(() => {
          this.notificationService.show('Department updated successfully.');
          this.loadDepartments();
        }),
      );
  }

  deleteDepartment(departmentId: string): Observable<GenericResponse<object>> {
    const headers = this.httpHeaderService.getHeadersWithTokenSet();
    const params = new HttpParams().set('departmentId', departmentId);
    return this.http
      .delete<GenericResponse<object>>(`${this.API_URL}/delete`, {
        headers,
        params,
      })
      .pipe(
        tap(() => {
          this.notificationService.show('Department deleted successfully.');
          this.loadDepartments();
        }),
      );
  }
}
