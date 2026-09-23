import { provideHttpClient } from '@angular/common/http';
import {
  HttpTestingController,
  provideHttpClientTesting,
} from '@angular/common/http/testing';
import { signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { environment } from '../../environments/environment';
import { Employee, EmployeeStatus } from '../interfaces/employee-response';
import { ActivateEmployeeService } from './activate-employee.service';
import { GetEmployeeService } from './get-employee.service';
import { HttpHeaderService } from './http-header-service';
import { NotificationService } from './notification.service';

describe('ActivateEmployeeService', () => {
  let service: ActivateEmployeeService;
  let httpMock: HttpTestingController;
  let employeesSignal: ReturnType<typeof signal<Employee[]>>;
  let updateEmployeeLocally: ReturnType<typeof vi.fn>;
  let notificationShow: ReturnType<typeof vi.fn>;

  const DEACTIVATE_URL =
    environment.apiUrl + '/api/employee/deactivate';
  const REACTIVATE_URL =
    environment.apiUrl + '/api/employee/reactivate';

  const buildEmployee = (overrides: Partial<Employee> = {}): Employee => ({
    employeeId: 'employeeId-1',
    firstName: 'Dan',
    lastName: 'Frunza',
    phoneNumber: '123456789',
    email: 'dan@example.com',
    gender: 1,
    status: EmployeeStatus.Active,
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
    ...overrides,
  });

  beforeEach(() => {
    updateEmployeeLocally = vi.fn();
    notificationShow = vi.fn();
    employeesSignal = signal<Employee[]>([buildEmployee()]);

    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        {
          provide: HttpHeaderService,
          useValue: { getHeadersWithTokenSet: () => ({}) },
        },
        {
          provide: GetEmployeeService,
          useValue: { employeesSignal, updateEmployeeLocally },
        },
        { provide: NotificationService, useValue: { show: notificationShow } },
      ],
    });
    service = TestBed.inject(ActivateEmployeeService);
    httpMock = TestBed.inject(HttpTestingController);
    vi.spyOn(console, 'error').mockImplementation(() => {});
  });

  afterEach(() => {
    httpMock.verify();
    vi.restoreAllMocks();
    vi.useRealTimers();
  });

  it('sets loadingSignal true synchronously while deactivation is in flight', () => {
    service.deactivateEmployee('employeeId-1');

    expect(service.loadingSignal()).toBe(true);

    httpMock
      .expectOne((r) => r.url === DEACTIVATE_URL)
      .flush({ status: 200, responseMessage: 'ok' });
  });

  it('deactivateEmployee marks the local employee Deactivated and notifies on success', () => {
    service.deactivateEmployee('employeeId-1');

    const req = httpMock.expectOne((r) => r.url === DEACTIVATE_URL);
    expect(req.request.method).toBe('PATCH');
    expect(req.request.params.get('employeeId')).toBe('employeeId-1');
    req.flush({ status: 200, responseMessage: 'ok' });

    expect(updateEmployeeLocally).toHaveBeenCalledWith(
      expect.objectContaining({
        employeeId: 'employeeId-1',
        status: EmployeeStatus.Deactivated,
      }),
    );
    expect(notificationShow).toHaveBeenCalledWith(
      'Employee deactivated successfully.',
    );
    expect(service.loadingSignal()).toBe(false);
    expect(service.errorSignal()).toBeNull();
  });

  it('reactivateEmployee marks the local employee Active and hits the reactivate endpoint', () => {
    employeesSignal.set([buildEmployee({ status: EmployeeStatus.Deactivated })]);

    service.reactivateEmployee('employeeId-1');

    const req = httpMock.expectOne((r) => r.url === REACTIVATE_URL);
    expect(req.request.method).toBe('PATCH');
    req.flush({ status: 200, responseMessage: 'ok' });

    expect(updateEmployeeLocally).toHaveBeenCalledWith(
      expect.objectContaining({
        employeeId: 'employeeId-1',
        status: EmployeeStatus.Active,
      }),
    );
    expect(notificationShow).toHaveBeenCalledWith(
      'Employee reactivated successfully.',
    );
  });

  it('sets an error and skips the local update/notification when the response status is not 200', () => {
    service.deactivateEmployee('employeeId-1');

    httpMock
      .expectOne((r) => r.url === DEACTIVATE_URL)
      .flush({ status: 409, responseMessage: 'Employee is already deactivated.' });

    expect(updateEmployeeLocally).not.toHaveBeenCalled();
    expect(notificationShow).not.toHaveBeenCalled();
    expect(service.loadingSignal()).toBe(false);
    expect(service.errorSignal()).toBe('Deactivation failed');
  });

  it('sets a not-found error and skips notification when the employee is not in the local cache', () => {
    employeesSignal.set([]);

    service.deactivateEmployee('missing-employeeId');

    httpMock
      .expectOne((r) => r.url === DEACTIVATE_URL)
      .flush({ status: 200, responseMessage: 'ok' });

    expect(updateEmployeeLocally).not.toHaveBeenCalled();
    expect(notificationShow).not.toHaveBeenCalled();
    expect(service.errorSignal()).toContain('not found locally');
  });

  it('does not retry a definitive 4xx error and surfaces the server message', () => {
    service.deactivateEmployee('employeeId-1');

    httpMock
      .expectOne((r) => r.url === DEACTIVATE_URL)
      .flush(
        { message: 'Employee is already deactivated.' },
        { status: 409, statusText: 'Conflict' },
      );

    expect(service.loadingSignal()).toBe(false);
    expect(service.errorSignal()).toBe('Employee is already deactivated.');
    // httpMock.verify() in afterEach confirms no retry request was made.
  });

  it('retries once on a transient (5xx) failure and then succeeds', () => {
    vi.useFakeTimers();

    service.deactivateEmployee('employeeId-1');

    const firstAttempt = httpMock.expectOne((r) => r.url === DEACTIVATE_URL);
    firstAttempt.flush(null, { status: 500, statusText: 'Server Error' });

    vi.advanceTimersByTime(500);

    const secondAttempt = httpMock.expectOne((r) => r.url === DEACTIVATE_URL);
    secondAttempt.flush({ status: 200, responseMessage: 'ok' });

    expect(updateEmployeeLocally).toHaveBeenCalledWith(
      expect.objectContaining({ status: EmployeeStatus.Deactivated }),
    );
    expect(service.loadingSignal()).toBe(false);
    expect(service.errorSignal()).toBeNull();
  });
});
