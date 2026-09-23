import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { CreateEmployeeRequest } from '../interfaces/employee-response';
import { GenericResponse } from '../../../src/app/interfaces/generic-response';
import { environment } from '../../environments/environment';
import { HttpHeaderService } from './http-header-service';
import { NotificationService } from './notification.service';

@Injectable({
  providedIn: 'root',
})
export class AddEmployeeService {
  private readonly httpHeaderService = inject(HttpHeaderService);
  private readonly http = inject(HttpClient);
  private readonly notificationService = inject(NotificationService);
  readonly APIURL =
    environment.apiUrl + '/api/employee/register';

  // On success, `data` is the new employee's server-generated ID.
  addEmployee(employee: CreateEmployeeRequest): Observable<GenericResponse<string>> {
    const headers = this.httpHeaderService.getHeadersWithTokenSet();
    return this.http
      .post<GenericResponse<string>>(this.APIURL, employee, {
        headers: headers,
      })
      .pipe(
        tap(() =>
          this.notificationService.show('Employee registered successfully.'),
        ),
      );
  }

  /**
   * Same endpoint as {@link addEmployee}, without the per-call success toast —
   * for callers (e.g. bulk test-data generation) that show one summary
   * notification instead of one per request.
   */
  addEmployeeSilently(
    employee: CreateEmployeeRequest,
  ): Observable<GenericResponse<string>> {
    const headers = this.httpHeaderService.getHeadersWithTokenSet();
    return this.http.post<GenericResponse<string>>(this.APIURL, employee, {
      headers: headers,
    });
  }
}
