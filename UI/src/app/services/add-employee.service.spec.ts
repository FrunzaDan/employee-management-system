import { provideHttpClient } from '@angular/common/http';
import {
  HttpTestingController,
  provideHttpClientTesting,
} from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { environment } from '../../environments/environment';
import { Employee } from '../interfaces/employee-response';
import { AddEmployeeService } from './add-employee.service';
import { HttpHeaderService } from './http-header-service';
import { NotificationService } from './notification.service';

describe('AddEmployeeService', () => {
  let service: AddEmployeeService;
  let httpMock: HttpTestingController;
  let notificationShow: ReturnType<typeof vi.fn>;

  const API_URL = `${environment.EmployeeManagementSystemAPI}/api/Employee/register`;

  const buildEmployee = (): Employee => ({
    guid: '',
    firstName: 'Dan',
    lastName: 'Frunza',
    msisdn: '123456789',
    email: 'dan@example.com',
    gender: 1,
    employeeStatus: 1901,
    creationDate: '',
    interactionDate: '',
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
    notificationShow = vi.fn();

    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        {
          provide: HttpHeaderService,
          useValue: { getHeadersWithTokenSet: () => ({}) },
        },
        { provide: NotificationService, useValue: { show: notificationShow } },
      ],
    });
    service = TestBed.inject(AddEmployeeService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('addEmployee POSTs the employee to the register endpoint', () => {
    service.addEmployee(buildEmployee()).subscribe();

    const req = httpMock.expectOne(API_URL);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual(buildEmployee());

    req.flush({ status: 200, responseMessage: 'Employee created successfully.' });
  });

  it('addEmployee shows a success notification once the request resolves', () => {
    service.addEmployee(buildEmployee()).subscribe();

    httpMock
      .expectOne(API_URL)
      .flush({ status: 200, responseMessage: 'Employee created successfully.' });

    expect(notificationShow).toHaveBeenCalledWith('Employee registered successfully.');
  });

  it('addEmployeeSilently POSTs to the same endpoint without showing a notification', () => {
    service.addEmployeeSilently(buildEmployee()).subscribe();

    const req = httpMock.expectOne(API_URL);
    expect(req.request.method).toBe('POST');
    req.flush({ status: 200, responseMessage: 'Employee created successfully.' });

    expect(notificationShow).not.toHaveBeenCalled();
  });
});
