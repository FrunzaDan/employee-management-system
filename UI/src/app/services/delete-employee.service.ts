import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, tap } from 'rxjs';
import { environment } from '../../environments/environment';
import { GenericResponse } from '../interfaces/generic-response';
import { GetEmployeeService } from './get-employee.service';
import { HttpHeaderService } from './http-header-service';
import { NotificationService } from './notification.service';

@Injectable({
  providedIn: 'root',
})
export class DeleteEmployeeService {
  readonly APIURL =
    environment.EmployeeManagementSystemAPI + '/api/Employee/delete';

  private readonly http = inject(HttpClient);
  private readonly httpHeaderService = inject(HttpHeaderService);
  private readonly getEmployeeService = inject(GetEmployeeService);
  private readonly notificationService = inject(NotificationService);

  deleteEmployee(employeeGUID: string): Observable<GenericResponse<object>> {
    const headers: HttpHeaders =
      this.httpHeaderService.getHeadersWithTokenSet();
    const params = new HttpParams().set('employeeGUID', employeeGUID);

    return this.http
      .delete<GenericResponse<object>>(this.APIURL, { headers, params })
      .pipe(
        tap(() => {
          this.getEmployeeService.removeEmployeeLocally(employeeGUID);
          this.notificationService.show('Employee deleted successfully.');
        }),
      );
  }

  /**
   * Same endpoint as {@link deleteEmployee}, without the per-call success toast —
   * for bulk-delete callers that show one summary notification instead of one per
   * employee.
   */
  deleteEmployeeSilently(
    employeeGUID: string,
  ): Observable<GenericResponse<object>> {
    const headers: HttpHeaders =
      this.httpHeaderService.getHeadersWithTokenSet();
    const params = new HttpParams().set('employeeGUID', employeeGUID);

    return this.http
      .delete<GenericResponse<object>>(this.APIURL, { headers, params })
      .pipe(tap(() => this.getEmployeeService.removeEmployeeLocally(employeeGUID)));
  }
}
