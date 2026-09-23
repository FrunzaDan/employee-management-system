import { HttpClient, HttpErrorResponse, HttpParams } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { Observable, map, tap } from 'rxjs';
import { environment } from '../../environments/environment';
import { GenericResponse } from '../interfaces/generic-response';
import { CostCenter } from '../interfaces/cost-center-response';
import { EmployeeSummary } from '../interfaces/employee-summary-response';
import { HttpHeaderService } from './http-header-service';
import { NotificationService } from './notification.service';

@Injectable({
  providedIn: 'root',
})
export class CostCenterService {
  private readonly API_URL = `${environment.apiUrl}/api/cost-center`;

  private readonly http = inject(HttpClient);
  private readonly httpHeaderService = inject(HttpHeaderService);
  private readonly notificationService = inject(NotificationService);

  private readonly state = signal({
    costCenters: [] as CostCenter[],
    loading: false,
    error: null as string | null,
  });

  readonly costCentersSignal = computed(() => this.state().costCenters);
  readonly loadingSignal = computed(() => this.state().loading);
  readonly errorSignal = computed(() => this.state().error);

  loadCostCenters(): void {
    this.state.update((s) => ({ ...s, loading: true, error: null }));
    const headers = this.httpHeaderService.getHeadersWithTokenSet();

    this.http
      .get<GenericResponse<CostCenter[]>>(`${this.API_URL}/all`, { headers })
      .subscribe({
        next: (response) =>
          this.state.set({ costCenters: response.data ?? [], loading: false, error: null }),
        error: (error: HttpErrorResponse) =>
          this.state.set({
            costCenters: [],
            loading: false,
            error: error.error?.message || 'Failed to load cost centers.',
          }),
      });
  }

  /** See OfficeService.fetchOfficesOnce for why this exists alongside loadCostCenters. */
  fetchCostCentersOnce(): Observable<CostCenter[]> {
    const headers = this.httpHeaderService.getHeadersWithTokenSet();
    return this.http
      .get<GenericResponse<CostCenter[]>>(`${this.API_URL}/all`, { headers })
      .pipe(map((response) => response.data ?? []));
  }

  /** A single cost center by costCenterId — for the cost center details page, reached directly by URL. */
  getCostCenter(costCenterId: string): Observable<CostCenter> {
    const headers = this.httpHeaderService.getHeadersWithTokenSet();
    const params = new HttpParams().set('costCenterId', costCenterId);
    return this.http
      .get<GenericResponse<CostCenter>>(`${this.API_URL}/get`, { headers, params })
      .pipe(
        map((response) => {
          if (!response.data) throw new Error('Cost center not found.');
          return response.data;
        }),
      );
  }

  /** The employees currently assigned to this cost center (see Employee_ListByCostCenter). */
  getEmployees(costCenterId: string): Observable<EmployeeSummary[]> {
    const headers = this.httpHeaderService.getHeadersWithTokenSet();
    const params = new HttpParams().set('costCenterId', costCenterId);
    return this.http
      .get<GenericResponse<EmployeeSummary[]>>(`${this.API_URL}/employees`, { headers, params })
      .pipe(map((response) => response.data ?? []));
  }

  createCostCenter(costCenter: Partial<CostCenter>): Observable<GenericResponse<object>> {
    const headers = this.httpHeaderService.getHeadersWithTokenSet();
    return this.http
      .post<GenericResponse<object>>(`${this.API_URL}/create`, costCenter, { headers })
      .pipe(
        tap(() => {
          this.notificationService.show('Cost center created successfully.');
          this.loadCostCenters();
        }),
      );
  }

  editCostCenter(costCenter: Partial<CostCenter>): Observable<GenericResponse<object>> {
    const headers = this.httpHeaderService.getHeadersWithTokenSet();
    return this.http
      .patch<GenericResponse<object>>(`${this.API_URL}/edit`, costCenter, { headers })
      .pipe(
        tap(() => {
          this.notificationService.show('Cost center updated successfully.');
          this.loadCostCenters();
        }),
      );
  }

  deleteCostCenter(costCenterId: string): Observable<GenericResponse<object>> {
    const headers = this.httpHeaderService.getHeadersWithTokenSet();
    const params = new HttpParams().set('costCenterId', costCenterId);
    return this.http
      .delete<GenericResponse<object>>(`${this.API_URL}/delete`, { headers, params })
      .pipe(
        tap(() => {
          this.notificationService.show('Cost center deleted successfully.');
          this.loadCostCenters();
        }),
      );
  }
}
