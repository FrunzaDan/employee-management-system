import { provideHttpClient } from '@angular/common/http';
import {
  HttpTestingController,
  provideHttpClientTesting,
} from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { environment } from '../../environments/environment';
import {
  CreateEmployeeRequest,
  Gender,
} from '../interfaces/employee-response';
import { CreateEmployeeService } from './create-employee.service';
import { HttpHeaderService } from './http-header.service';
import { NotificationService } from './notification.service';

describe('CreateEmployeeService', () => {
  let service: CreateEmployeeService;
  let httpMock: HttpTestingController;
  let notificationShow: ReturnType<typeof vi.fn>;

  const API_URL = `${environment.apiUrl}/api/employee/create`;

  const buildEmployee = (): CreateEmployeeRequest => ({
    firstName: 'Dan',
    lastName: 'Frunza',
    phoneNumber: '123456789',
    email: 'dan@example.com',
    gender: Gender.Male,
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
    service = TestBed.inject(CreateEmployeeService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('createEmployee POSTs the employee to the create endpoint', () => {
    service.createEmployee(buildEmployee()).subscribe();

    const req = httpMock.expectOne(API_URL);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual(buildEmployee());

    req.flush({ status: 200, responseMessage: 'Employee created successfully.' });
  });

  it('createEmployee shows a success notification once the request resolves', () => {
    service.createEmployee(buildEmployee()).subscribe();

    httpMock
      .expectOne(API_URL)
      .flush({ status: 200, responseMessage: 'Employee created successfully.' });

    expect(notificationShow).toHaveBeenCalledWith('Employee registered successfully.');
  });

  it('createEmployeeSilently POSTs to the same endpoint without showing a notification', () => {
    service.createEmployeeSilently(buildEmployee()).subscribe();

    const req = httpMock.expectOne(API_URL);
    expect(req.request.method).toBe('POST');
    req.flush({ status: 200, responseMessage: 'Employee created successfully.' });

    expect(notificationShow).not.toHaveBeenCalled();
  });
});
