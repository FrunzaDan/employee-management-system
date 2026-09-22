import { provideHttpClient } from '@angular/common/http';
import {
  HttpTestingController,
  provideHttpClientTesting,
} from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { environment } from '../../environments/environment';
import { Employee } from '../interfaces/employee-response';
import { EditEmployeeService } from './edit-employee.service';
import { GetEmployeeService } from './get-employee.service';
import { HttpHeaderService } from './http-header-service';
import { NotificationService } from './notification.service';

describe('EditEmployeeService', () => {
  let service: EditEmployeeService;
  let httpMock: HttpTestingController;
  let updateEmployeeLocally: ReturnType<typeof vi.fn>;
  let notificationShow: ReturnType<typeof vi.fn>;

  const API_URL = `${environment.EmployeeManagementSystemAPI}/api/Employee/edit`;

  const buildEmployee = (): Employee => ({
    guid: 'guid-1',
    firstName: 'Dan',
    lastName: 'Frunza',
    msisdn: '123456789',
    email: 'dan@example.com',
    gender: 1,
    employeeStatus: 1901,
    creationDate: '2026-01-01',
    interactionDate: '2026-01-01',
    birthdate: '1990-01-01',
    address: {
      country: 'Romania',
      county: 'Cluj',
      town: 'Cluj-Napoca',
      zip: '400000',
      street: 'Main',
      number: '1',
    },
  });

  beforeEach(() => {
    updateEmployeeLocally = vi.fn();
    notificationShow = vi.fn();

    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        {
          provide: HttpHeaderService,
          useValue: { getHeadersWithTokenSet: () => ({}) },
        },
        { provide: GetEmployeeService, useValue: { updateEmployeeLocally } },
        { provide: NotificationService, useValue: { show: notificationShow } },
      ],
    });
    service = TestBed.inject(EditEmployeeService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('PATCHes the employee to the edit endpoint', () => {
    const employee = buildEmployee();
    service.editEmployee(employee).subscribe();

    const req = httpMock.expectOne(API_URL);
    expect(req.request.method).toBe('PATCH');
    expect(req.request.body).toEqual(employee);

    req.flush({ status: 200, responseMessage: 'Employee updated successfully.' });
  });

  it('updates the employee in the local cache and notifies on success', () => {
    const employee = buildEmployee();
    service.editEmployee(employee).subscribe();

    httpMock
      .expectOne(API_URL)
      .flush({ status: 200, responseMessage: 'Employee updated successfully.' });

    expect(updateEmployeeLocally).toHaveBeenCalledWith(employee);
    expect(notificationShow).toHaveBeenCalledWith('Employee updated successfully.');
  });

  it('does not touch the local cache or notify when the request errors', () => {
    service.editEmployee(buildEmployee()).subscribe({ error: () => {} });

    httpMock
      .expectOne(API_URL)
      .flush({ message: 'boom' }, { status: 400, statusText: 'Bad Request' });

    expect(updateEmployeeLocally).not.toHaveBeenCalled();
    expect(notificationShow).not.toHaveBeenCalled();
  });
});
