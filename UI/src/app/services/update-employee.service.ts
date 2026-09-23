import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { environment } from '../../environments/environment';
import { GenericResponse } from '../../../src/app/interfaces/generic-response';
import {
  Employee,
  UpdateEmployeeRequest,
} from '../interfaces/employee-response';
import { HttpHeaderService } from './http-header.service';
import { GetEmployeeService } from './get-employee.service'; // Inject to update locally
import { NotificationService } from './notification.service';

@Injectable({
  providedIn: 'root',
})
export class UpdateEmployeeService {
  private readonly APIURL =
    environment.apiUrl + '/api/employee/update';

  private readonly http = inject(HttpClient);
  private readonly httpHeaderService = inject(HttpHeaderService);
  private readonly getEmployeeService = inject(GetEmployeeService);
  private readonly notificationService = inject(NotificationService);

  // Takes the whole edited employee (to update the local list with once saved) but sends
  // only the editable fields — the server-owned ones (status, dates) aren't part of an edit.
  updateEmployee(employee: Employee): Observable<GenericResponse<object>> {
    const headers: HttpHeaders =
      this.httpHeaderService.getHeadersWithTokenSet();

    return this.http
      .patch<GenericResponse<object>>(
        this.APIURL,
        toUpdateEmployeeRequest(employee),
        { headers },
      )
      .pipe(
        tap(() => {
          this.getEmployeeService.updateEmployeeLocally(employee);
          this.notificationService.show('Employee updated successfully.');
        }),
      );
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
    birthDate: employee.birthDate,
    address: employee.address,
    hireDate: employee.hireDate,
    officeId: employee.officeId,
    departmentId: employee.departmentId,
    costCenterId: employee.costCenterId,
  };
}
