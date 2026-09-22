import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { environment } from '../../environments/environment';
import { GenericResponse } from '../../../src/app/interfaces/generic-response';
import { Employee } from '../interfaces/employee-response';
import { HttpHeaderService } from './http-header-service';
import { GetEmployeeService } from './get-employee.service'; // Inject to update locally
import { NotificationService } from './notification.service';

@Injectable({
  providedIn: 'root',
})
export class EditEmployeeService {
  private readonly APIURL =
    environment.EmployeeManagementSystemAPI + '/api/Employee/edit';

  private readonly http = inject(HttpClient);
  private readonly httpHeaderService = inject(HttpHeaderService);
  private readonly getEmployeeService = inject(GetEmployeeService);
  private readonly notificationService = inject(NotificationService);

  editEmployee(employee: Employee): Observable<GenericResponse<object>> {
    const headers: HttpHeaders =
      this.httpHeaderService.getHeadersWithTokenSet();

    return this.http
      .patch<GenericResponse<object>>(this.APIURL, employee, { headers })
      .pipe(
        tap(() => {
          this.getEmployeeService.updateEmployeeLocally(employee);
          this.notificationService.show('Employee updated successfully.');
        }),
      );
  }
}
