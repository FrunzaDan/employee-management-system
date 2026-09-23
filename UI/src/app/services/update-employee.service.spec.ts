import { provideHttpClient } from '@angular/common/http';
import {
  HttpTestingController,
  provideHttpClientTesting,
} from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { environment } from '../../environments/environment';
import { Employee } from '../interfaces/employee-response';
import { UpdateEmployeeService } from './update-employee.service';
import { GetEmployeeService } from './get-employee.service';
import { HttpHeaderService } from './http-header.service';
import { NotificationService } from './notification.service';

describe('UpdateEmployeeService', () => {
  let service: UpdateEmployeeService;
  let httpMock: HttpTestingController;
  let updateEmployeeLocally: ReturnType<typeof vi.fn>;
  let notificationShow: ReturnType<typeof vi.fn>;

  const API_URL = `${environment.apiUrl}/api/employee/update`;

  const buildEmployee = (): Employee => ({
    employeeId: 'employeeId-1',
    firstName: 'Dan',
    lastName: 'Frunza',
    phoneNumber: '123456789',
    email: 'dan@example.com',
    gender: 1,
    status: 1901,
    createdAt: '2026-01-01',
    lastInteractionAt: '2026-01-01',
    birthDate: '1990-01-01',
    address: {
      country: 'Romania',
      county: 'Cluj',
      city: 'Cluj-Napoca',
      postalCode: '400000',
      street: 'Main',
      streetNumber: '1',
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
    service = TestBed.inject(UpdateEmployeeService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('PATCHes only the editable fields to the update endpoint', () => {
    const employee = buildEmployee();
    service.updateEmployee(employee).subscribe();

    const req = httpMock.expectOne(API_URL);
    expect(req.request.method).toBe('PATCH');
    // Server-owned fields (status, dates, joined names, salary) aren't part of an edit request.
    const {
      status,
      createdAt,
      lastInteractionAt,
      officeName,
      departmentName,
      costCenterName,
      currentGrossSalary,
      ...editable
    } = employee;
    expect(req.request.body).toEqual(editable);

    req.flush({ status: 200, responseMessage: 'Employee updated successfully.' });
  });

  it('updates the employee in the local cache and notifies on success', () => {
    const employee = buildEmployee();
    service.updateEmployee(employee).subscribe();

    httpMock
      .expectOne(API_URL)
      .flush({ status: 200, responseMessage: 'Employee updated successfully.' });

    expect(updateEmployeeLocally).toHaveBeenCalledWith(employee);
    expect(notificationShow).toHaveBeenCalledWith('Employee updated successfully.');
  });

  it('does not touch the local cache or notify when the request errors', () => {
    service.updateEmployee(buildEmployee()).subscribe({ error: () => {} });

    httpMock
      .expectOne(API_URL)
      .flush({ message: 'boom' }, { status: 400, statusText: 'Bad Request' });

    expect(updateEmployeeLocally).not.toHaveBeenCalled();
    expect(notificationShow).not.toHaveBeenCalled();
  });
});
