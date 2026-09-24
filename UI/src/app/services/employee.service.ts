import {
  HttpClient,
  HttpErrorResponse,
  HttpParams,
  httpResource,
} from '@angular/common/http';
import {
  computed,
  inject,
  Injectable,
  linkedSignal,
  signal,
} from '@angular/core';
import { Observable, map, retry, tap, throwError, timer } from 'rxjs';
import {
  CreateEmployeeRequest,
  Employee,
  EmployeeStatus,
  UpdateEmployeeRequest,
} from '../interfaces/employee';
import { environment } from '../../environments/environment';
import { extractErrorMessage } from '../utils/extract-error-message';
import { GenericResponse } from '../interfaces/generic-response';
import { PagedResponse } from '../interfaces/paged-response';
import { NotificationService } from './notification.service';

export interface LoadEmployeesParams {
  pageNumber: number;
  pageSize: number;
  searchTerm?: string;
  sortColumn?: 'name' | 'email' | 'phoneNumber';
  sortDirection?: 'asc' | 'desc';
}

export type ExportEmployeesParams = Omit<
  LoadEmployeesParams,
  'pageNumber' | 'pageSize'
>;

const DEFAULT_PAGE_SIZE = 10;

const TRANSIENT_ERROR_RETRY_CONFIG = {
  count: 3,
  delay: (error: unknown, retryCount: number) =>
    error instanceof HttpErrorResponse &&
    (error.status === 0 || error.status >= 500)
      ? timer(retryCount * 500)
      : throwError(() => error),
};

@Injectable({
  providedIn: 'root',
})
export class EmployeeService {
  private readonly apiUrl = `${environment.apiUrl}/api/employee`;

  private readonly http = inject(HttpClient);
  private readonly notificationService = inject(NotificationService);

  private readonly listParams = signal<LoadEmployeesParams | undefined>(
    undefined,
  );

  private readonly employeesResource = httpResource<
    GenericResponse<PagedResponse<Employee>>
  >(() => {
    const params = this.listParams();
    if (!params) return undefined;
    return {
      url: `${this.apiUrl}/all`,
      params: {
        pageNumber: params.pageNumber,
        pageSize: params.pageSize,
        sortColumn: params.sortColumn ?? 'name',
        sortDirection: params.sortDirection ?? 'asc',
        ...(params.searchTerm ? { searchTerm: params.searchTerm } : {}),
      },
    };
  });

  private readonly page = linkedSignal<
    PagedResponse<Employee> | undefined,
    PagedResponse<Employee> | undefined
  >({
    source: () =>
      this.employeesResource.hasValue()
        ? (this.employeesResource.value().data ?? undefined)
        : undefined,
    computation: (page, previous) => page ?? previous?.value,
  });

  readonly employees = computed(() => this.page()?.items ?? []);
  readonly pageNumber = computed(
    () => this.page()?.pageNumber ?? this.listParams()?.pageNumber ?? 1,
  );
  readonly pageSize = computed(
    () =>
      this.page()?.pageSize ?? this.listParams()?.pageSize ?? DEFAULT_PAGE_SIZE,
  );
  readonly totalItems = computed(() => this.page()?.totalItems ?? 0);
  readonly loading = this.employeesResource.isLoading;
  readonly error = computed(() => {
    const error = this.employeesResource.error();
    return error
      ? extractErrorMessage(
          error as HttpErrorResponse,
          'Failed to load employees',
        )
      : null;
  });

  private readonly activationState = signal({
    loading: false,
    error: null as string | null,
  });

  readonly activationLoading = computed(() => this.activationState().loading);
  readonly activationError = computed(() => this.activationState().error);

  readonly exportLoading = signal(false);
  readonly exportError = signal<string | null>(null);

  loadEmployees(params: LoadEmployeesParams): void {
    this.listParams.set({ ...params });
  }

  getEmployee(employeeId: string): Observable<Employee> {
    return this.http
      .get<GenericResponse<Employee>>(`${this.apiUrl}/get`, {
        params: { searchTerm: employeeId },
      })
      .pipe(
        map((response) => {
          if (!response.data) throw new Error('Employee not found.');
          return response.data;
        }),
      );
  }

  createEmployee(
    employee: CreateEmployeeRequest,
  ): Observable<GenericResponse<string>> {
    return this.createEmployeeSilently(employee).pipe(
      tap(() => this.notificationService.show('Employee added successfully.')),
    );
  }

  createEmployeeSilently(
    employee: CreateEmployeeRequest,
  ): Observable<GenericResponse<string>> {
    return this.http.post<GenericResponse<string>>(
      `${this.apiUrl}/create`,
      employee,
    );
  }

  updateEmployee(employee: Employee): Observable<GenericResponse<object>> {
    return this.http
      .patch<GenericResponse<object>>(
        `${this.apiUrl}/update`,
        toUpdateEmployeeRequest(employee),
      )
      .pipe(
        tap(() => {
          this.updateEmployeeLocally(employee);
          this.notificationService.show('Employee updated successfully.');
        }),
      );
  }

  deleteEmployee(employeeId: string): Observable<GenericResponse<object>> {
    return this.deleteEmployeeSilently(employeeId).pipe(
      tap(() =>
        this.notificationService.show('Employee deleted successfully.'),
      ),
    );
  }

  deleteEmployeeSilently(
    employeeId: string,
  ): Observable<GenericResponse<object>> {
    const params = new HttpParams().set('employeeId', employeeId);

    return this.http
      .delete<GenericResponse<object>>(`${this.apiUrl}/delete`, { params })
      .pipe(tap(() => this.removeEmployeeLocally(employeeId)));
  }

  deactivateEmployee(employeeId: string): void {
    this.changeStatus(employeeId, 'deactivate', EmployeeStatus.Deactivated);
  }

  reactivateEmployee(employeeId: string): void {
    this.changeStatus(employeeId, 'reactivate', EmployeeStatus.Active);
  }

  deactivateEmployeeSilently(
    employeeId: string,
  ): Observable<GenericResponse<object>> {
    const params = new HttpParams().set('employeeId', employeeId);

    return this.http
      .patch<GenericResponse<object>>(`${this.apiUrl}/deactivate`, null, {
        params,
      })
      .pipe(
        tap(() =>
          this.setStatusLocally(employeeId, EmployeeStatus.Deactivated),
        ),
      );
  }

  exportEmployees(params: ExportEmployeesParams): void {
    this.exportLoading.set(true);
    this.exportError.set(null);

    let httpParams = new HttpParams()
      .set('sortColumn', params.sortColumn ?? 'name')
      .set('sortDirection', params.sortDirection ?? 'asc');

    if (params.searchTerm) {
      httpParams = httpParams.set('searchTerm', params.searchTerm);
    }

    this.http
      .get(`${this.apiUrl}/export`, {
        params: httpParams,
        responseType: 'blob',
      })
      .subscribe({
        next: (blob) => {
          this.exportLoading.set(false);
          this.triggerDownload(blob, this.buildExportFilename());
        },
        error: (error: HttpErrorResponse) => {
          this.exportLoading.set(false);
          this.exportError.set(
            extractErrorMessage(error, 'Failed to export employees'),
          );
        },
      });
  }

  private changeStatus(
    employeeId: string,
    action: 'deactivate' | 'reactivate',
    status: EmployeeStatus,
  ): void {
    this.activationState.set({ loading: true, error: null });
    const params = new HttpParams().set('employeeId', employeeId);

    this.http
      .patch<GenericResponse<object>>(`${this.apiUrl}/${action}`, null, {
        params,
      })
      .pipe(retry(TRANSIENT_ERROR_RETRY_CONFIG))
      .subscribe({
        next: () => {
          this.setStatusLocally(employeeId, status);
          this.activationState.set({ loading: false, error: null });
          this.notificationService.show(`Employee ${action}d successfully.`);
        },
        error: (error: HttpErrorResponse) => this.handleActivationError(error),
      });
  }

  private setStatusLocally(employeeId: string, status: EmployeeStatus): void {
    const existingEmployee = this.employees().find(
      (c) => c.employeeId === employeeId,
    );
    if (existingEmployee)
      this.updateEmployeeLocally({ ...existingEmployee, status });
  }

  private updateEmployeeLocally(updatedEmployee: Employee): void {
    this.updateLoadedPage((items) =>
      items.map((c) =>
        c.employeeId === updatedEmployee.employeeId ? updatedEmployee : c,
      ),
    );
  }

  private removeEmployeeLocally(employeeId: string): void {
    this.updateLoadedPage((items) =>
      items.filter((c) => c.employeeId !== employeeId),
    );
  }

  private updateLoadedPage(update: (items: Employee[]) => Employee[]): void {
    this.page.update((page) => page && { ...page, items: update(page.items) });
  }

  private buildExportFilename(): string {
    const timestamp = new Date().toISOString().replace(/[:.]/g, '-');
    return `employees_${timestamp}.csv`;
  }

  private triggerDownload(blob: Blob, filename: string): void {
    const url = URL.createObjectURL(blob);
    const anchor = document.createElement('a');
    anchor.href = url;
    anchor.download = filename;
    anchor.click();
    URL.revokeObjectURL(url);
  }

  private handleActivationError(error: HttpErrorResponse): void {
    this.activationState.set({
      loading: false,
      error: extractErrorMessage(error, 'Failed to update the employee status'),
    });
  }
}

function toUpdateEmployeeRequest(employee: Employee): UpdateEmployeeRequest {
  return {
    employeeId: employee.employeeId,
    firstName: employee.firstName,
    lastName: employee.lastName,
    email: employee.email,
    phoneNumber: employee.phoneNumber,
    gender: employee.gender,
    birthDate: employee.birthDate ?? undefined,
    hireDate: employee.hireDate ?? undefined,
    officeId: employee.officeId ?? undefined,
    departmentId: employee.departmentId ?? undefined,
    costCenterId: employee.costCenterId ?? undefined,
    address: employee.address,
  };
}
