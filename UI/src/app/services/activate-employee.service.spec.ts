import { provideHttpClient } from '@angular/common/http';
import {
  HttpTestingController,
  provideHttpClientTesting,
} from '@angular/common/http/testing';
import { signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { environment } from '../../environments/environment';
import { Employee, EmployeeActivationStatus } from '../interfaces/employee-response';
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
    environment.EmployeeManagementSystemAPI + '/api/Employee/deactivate';
  const REACTIVATE_URL =
    environment.EmployeeManagementSystemAPI + '/api/Employee/reactivate';

  const buildEmployee = (overrides: Partial<Employee> = {}): Employee => ({
    guid: 'guid-1',
    firstName: 'Dan',
    lastName: 'Frunza',
    msisdn: '123456789',
    email: 'dan@example.com',
    gender: 1,
    employeeStatus: EmployeeActivationStatus.Active,
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
    service.deactivateEmployee('guid-1');

    expect(service.loadingSignal()).toBe(true);

    httpMock
      .expectOne((r) => r.url === DEACTIVATE_URL)
      .flush({ status: 200, responseMessage: 'ok' });
  });

  it('deactivateEmployee marks the local employee Deactivated and notifies on success', () => {
    service.deactivateEmployee('guid-1');

    const req = httpMock.expectOne((r) => r.url === DEACTIVATE_URL);
    expect(req.request.method).toBe('PATCH');
    expect(req.request.params.get('employeeGUID')).toBe('guid-1');
    req.flush({ status: 200, responseMessage: 'ok' });

    expect(updateEmployeeLocally).toHaveBeenCalledWith(
      expect.objectContaining({
        guid: 'guid-1',
        employeeStatus: EmployeeActivationStatus.Deactivated,
      }),
    );
    expect(notificationShow).toHaveBeenCalledWith(
      'Employee deactivated successfully.',
    );
    expect(service.loadingSignal()).toBe(false);
    expect(service.errorSignal()).toBeNull();
  });

  it('reactivateEmployee marks the local employee Active and hits the reactivate endpoint', () => {
    employeesSignal.set([buildEmployee({ employeeStatus: EmployeeActivationStatus.Deactivated })]);

    service.reactivateEmployee('guid-1');

    const req = httpMock.expectOne((r) => r.url === REACTIVATE_URL);
    expect(req.request.method).toBe('PATCH');
    req.flush({ status: 200, responseMessage: 'ok' });

    expect(updateEmployeeLocally).toHaveBeenCalledWith(
      expect.objectContaining({
        guid: 'guid-1',
        employeeStatus: EmployeeActivationStatus.Active,
      }),
    );
    expect(notificationShow).toHaveBeenCalledWith(
      'Employee reactivated successfully.',
    );
  });

  it('sets an error and skips the local update/notification when the response status is not 200', () => {
    service.deactivateEmployee('guid-1');

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

    service.deactivateEmployee('missing-guid');

    httpMock
      .expectOne((r) => r.url === DEACTIVATE_URL)
      .flush({ status: 200, responseMessage: 'ok' });

    expect(updateEmployeeLocally).not.toHaveBeenCalled();
    expect(notificationShow).not.toHaveBeenCalled();
    expect(service.errorSignal()).toContain('not found locally');
  });

  it('does not retry a definitive 4xx error and surfaces the server message', () => {
    service.deactivateEmployee('guid-1');

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

    service.deactivateEmployee('guid-1');

    const firstAttempt = httpMock.expectOne((r) => r.url === DEACTIVATE_URL);
    firstAttempt.flush(null, { status: 500, statusText: 'Server Error' });

    vi.advanceTimersByTime(500);

    const secondAttempt = httpMock.expectOne((r) => r.url === DEACTIVATE_URL);
    secondAttempt.flush({ status: 200, responseMessage: 'ok' });

    expect(updateEmployeeLocally).toHaveBeenCalledWith(
      expect.objectContaining({ employeeStatus: EmployeeActivationStatus.Deactivated }),
    );
    expect(service.loadingSignal()).toBe(false);
    expect(service.errorSignal()).toBeNull();
  });
});
