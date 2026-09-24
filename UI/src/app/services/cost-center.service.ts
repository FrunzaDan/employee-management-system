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
import { CostCenter } from '../interfaces/cost-center';
import { EmployeeSummary } from '../interfaces/employee-summary';
import { extractErrorMessage } from '../utils/extract-error-message';
import { NotificationService } from './notification.service';

// Same shape as OfficeService.
@Injectable({
  providedIn: 'root',
})
export class CostCenterService {
  private readonly apiUrl = `${environment.apiUrl}/api/cost-center`;

  private readonly http = inject(HttpClient);
  private readonly notificationService = inject(NotificationService);

  private readonly requested = signal(false);

  private readonly costCentersResource = httpResource<
    GenericResponse<CostCenter[]>
  >(() => (this.requested() ? `${this.apiUrl}/all` : undefined));

  // hasValue() guards the read: value() throws while the resource is in error.
  readonly costCenters = computed(() =>
    this.costCentersResource.hasValue()
      ? (this.costCentersResource.value().data ?? [])
      : [],
  );
  readonly loading = this.costCentersResource.isLoading;
  readonly error = computed(() => {
    const error = this.costCentersResource.error();
    return error
      ? extractErrorMessage(
          error as HttpErrorResponse,
          'Failed to load cost centers',
        )
      : null;
  });

  loadCostCenters(): void {
    if (this.requested()) {
      this.costCentersResource.reload();
    } else {
      this.requested.set(true);
    }
  }

  /** See OfficeService.fetchOffices for why this exists alongside loadCostCenters. */
  fetchCostCenters(): Observable<CostCenter[]> {
    return this.http
      .get<GenericResponse<CostCenter[]>>(`${this.apiUrl}/all`)
      .pipe(map((response) => response.data ?? []));
  }

  /** A single cost center by costCenterId — for the cost center details page, reached directly by URL. */
  getCostCenter(costCenterId: string): Observable<CostCenter> {
    const params = new HttpParams().set('costCenterId', costCenterId);
    return this.http
      .get<GenericResponse<CostCenter>>(`${this.apiUrl}/get`, { params })
      .pipe(
        map((response) => {
          if (!response.data) throw new Error('Cost center not found.');
          return response.data;
        }),
      );
  }

  /** The employees currently assigned to this cost center (see Employee_ListByCostCenter). */
  getEmployees(costCenterId: string): Observable<EmployeeSummary[]> {
    const params = new HttpParams().set('costCenterId', costCenterId);
    return this.http
      .get<GenericResponse<EmployeeSummary[]>>(`${this.apiUrl}/employees`, {
        params,
      })
      .pipe(map((response) => response.data ?? []));
  }

  createCostCenter(
    costCenter: Partial<CostCenter>,
  ): Observable<GenericResponse<object>> {
    return this.http
      .post<GenericResponse<object>>(`${this.apiUrl}/create`, costCenter)
      .pipe(
        tap(() => {
          this.notificationService.show('Cost center added successfully.');
          this.loadCostCenters();
        }),
      );
  }

  updateCostCenter(
    costCenter: Partial<CostCenter>,
  ): Observable<GenericResponse<object>> {
    return this.http
      .patch<GenericResponse<object>>(`${this.apiUrl}/update`, costCenter)
      .pipe(
        tap(() => {
          this.notificationService.show('Cost center updated successfully.');
          this.loadCostCenters();
        }),
      );
  }

  deleteCostCenter(costCenterId: string): Observable<GenericResponse<object>> {
    const params = new HttpParams().set('costCenterId', costCenterId);
    return this.http
      .delete<GenericResponse<object>>(`${this.apiUrl}/delete`, { params })
      .pipe(
        tap(() => {
          this.notificationService.show('Cost center deleted successfully.');
          this.loadCostCenters();
        }),
      );
  }
}
