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
import { Department } from '../interfaces/department';
import { EmployeeSummary } from '../interfaces/employee-summary';
import { extractErrorMessage } from '../utils/extract-error-message';
import { NotificationService } from './notification.service';

// Same shape as OfficeService.
@Injectable({
  providedIn: 'root',
})
export class DepartmentService {
  private readonly API_URL = `${environment.apiUrl}/api/department`;

  private readonly http = inject(HttpClient);
  private readonly notificationService = inject(NotificationService);

  private readonly requested = signal(false);

  private readonly departmentsResource = httpResource<
    GenericResponse<Department[]>
  >(() => (this.requested() ? `${this.API_URL}/all` : undefined));

  // hasValue() guards the read: value() throws while the resource is in error.
  readonly departments = computed(() =>
    this.departmentsResource.hasValue()
      ? (this.departmentsResource.value().data ?? [])
      : [],
  );
  readonly loading = this.departmentsResource.isLoading;
  readonly error = computed(() => {
    const error = this.departmentsResource.error();
    return error
      ? extractErrorMessage(
          error as HttpErrorResponse,
          'Failed to load departments',
        )
      : null;
  });

  loadDepartments(): void {
    if (this.requested()) {
      this.departmentsResource.reload();
    } else {
      this.requested.set(true);
    }
  }

  /** See OfficeService.fetchOffices for why this exists alongside loadDepartments. */
  fetchDepartments(): Observable<Department[]> {
    return this.http
      .get<GenericResponse<Department[]>>(`${this.API_URL}/all`)
      .pipe(map((response) => response.data ?? []));
  }

  /** A single department by departmentId — for the department details page, reached directly by URL. */
  getDepartment(departmentId: string): Observable<Department> {
    const params = new HttpParams().set('departmentId', departmentId);
    return this.http
      .get<GenericResponse<Department>>(`${this.API_URL}/get`, { params })
      .pipe(
        map((response) => {
          if (!response.data) throw new Error('Department not found.');
          return response.data;
        }),
      );
  }

  /** The employees currently assigned to this department (see Employee_ListByDepartment). */
  getEmployees(departmentId: string): Observable<EmployeeSummary[]> {
    const params = new HttpParams().set('departmentId', departmentId);
    return this.http
      .get<GenericResponse<EmployeeSummary[]>>(`${this.API_URL}/employees`, {
        params,
      })
      .pipe(map((response) => response.data ?? []));
  }

  createDepartment(
    department: Partial<Department>,
  ): Observable<GenericResponse<object>> {
    return this.http
      .post<GenericResponse<object>>(`${this.API_URL}/create`, department)
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
    return this.http
      .patch<GenericResponse<object>>(`${this.API_URL}/update`, department)
      .pipe(
        tap(() => {
          this.notificationService.show('Department updated successfully.');
          this.loadDepartments();
        }),
      );
  }

  deleteDepartment(departmentId: string): Observable<GenericResponse<object>> {
    const params = new HttpParams().set('departmentId', departmentId);
    return this.http
      .delete<GenericResponse<object>>(`${this.API_URL}/delete`, { params })
      .pipe(
        tap(() => {
          this.notificationService.show('Department deleted successfully.');
          this.loadDepartments();
        }),
      );
  }
}
