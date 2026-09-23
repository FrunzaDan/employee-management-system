import { provideHttpClient } from '@angular/common/http';
import {
  HttpTestingController,
  provideHttpClientTesting,
} from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { environment } from '../../environments/environment';
import { Employee } from '../interfaces/employee-response';
import { GetEmployeeService } from './get-employee.service';

describe('GetEmployeeService', () => {
  let service: GetEmployeeService;
  let httpMock: HttpTestingController;

  const API_URL = `${environment.apiUrl}/api/employee/all`;

  const buildEmployee = (overrides: Partial<Employee> = {}): Employee => ({
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
    ...overrides,
  });

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    service = TestBed.inject(GetEmployeeService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('sends pageNumber, pageSize, sortColumn, and sortDirection as query params', () => {
    service.loadEmployees({
      pageNumber: 2,
      pageSize: 10,
      sortColumn: 'email',
      sortDirection: 'desc',
    });

    const req = httpMock.expectOne((r) => r.url === API_URL);
    expect(req.request.params.get('pageNumber')).toBe('2');
    expect(req.request.params.get('pageSize')).toBe('10');
    expect(req.request.params.get('sortColumn')).toBe('email');
    expect(req.request.params.get('sortDirection')).toBe('desc');
    expect(req.request.params.has('searchTerm')).toBe(false);

    req.flush({
      status: 200,
      responseMessage: 'ok',
      data: { pageNumber: 2, pageSize: 10, totalItems: 0, items: [] },
    });
  });

  it('defaults sortColumn to name and sortDirection to asc when not provided', () => {
    service.loadEmployees({ pageNumber: 1, pageSize: 10 });

    const req = httpMock.expectOne((r) => r.url === API_URL);
    expect(req.request.params.get('sortColumn')).toBe('name');
    expect(req.request.params.get('sortDirection')).toBe('asc');

    req.flush({
      status: 200,
      responseMessage: 'ok',
      data: { pageNumber: 1, pageSize: 10, totalItems: 0, items: [] },
    });
  });

  it('includes searchTerm only when a non-empty one is provided', () => {
    service.loadEmployees({ pageNumber: 1, pageSize: 10, searchTerm: 'dan' });

    const req = httpMock.expectOne((r) => r.url === API_URL);
    expect(req.request.params.get('searchTerm')).toBe('dan');

    req.flush({
      status: 200,
      responseMessage: 'ok',
      data: { pageNumber: 1, pageSize: 10, totalItems: 0, items: [] },
    });
  });

  it('populates employeesSignal/totalItemsSignal/pageNumberSignal/pageSizeSignal from a successful response', () => {
    const employee = buildEmployee();

    service.loadEmployees({ pageNumber: 1, pageSize: 10 });
    httpMock.expectOne((r) => r.url === API_URL).flush({
      status: 200,
      responseMessage: 'ok',
      data: { pageNumber: 1, pageSize: 10, totalItems: 1, items: [employee] },
    });

    expect(service.employeesSignal()).toEqual([employee]);
    expect(service.totalItemsSignal()).toBe(1);
    expect(service.pageNumberSignal()).toBe(1);
    expect(service.pageSizeSignal()).toBe(10);
    expect(service.loadingSignal()).toBe(false);
    expect(service.errorSignal()).toBeNull();
  });

  it('sets loading true synchronously while the request is in flight', () => {
    service.loadEmployees({ pageNumber: 1, pageSize: 10 });

    expect(service.loadingSignal()).toBe(true);

    httpMock.expectOne((r) => r.url === API_URL).flush({
      status: 200,
      responseMessage: 'ok',
      data: { pageNumber: 1, pageSize: 10, totalItems: 0, items: [] },
    });

    expect(service.loadingSignal()).toBe(false);
  });

  it('sets a friendly message and clears loading on a network error (status 0)', () => {
    service.loadEmployees({ pageNumber: 1, pageSize: 10 });

    httpMock
      .expectOne((r) => r.url === API_URL)
      .error(new ProgressEvent('error'), { status: 0 });

    expect(service.loadingSignal()).toBe(false);
    expect(service.errorSignal()).toBe(
      'Could not reach the server. It may be offline, or your browser does not trust its security certificate.',
    );
  });

  it('updateEmployeeLocally replaces a matching employee in employeesSignal', () => {
    const original = buildEmployee();

    service.loadEmployees({ pageNumber: 1, pageSize: 10 });
    httpMock.expectOne((r) => r.url === API_URL).flush({
      status: 200,
      responseMessage: 'ok',
      data: { pageNumber: 1, pageSize: 10, totalItems: 1, items: [original] },
    });

    const updated = { ...original, firstName: 'Updated' };
    service.updateEmployeeLocally(updated);

    expect(service.employeesSignal()).toEqual([updated]);
  });

  it('removeEmployeeLocally drops a matching employee from employeesSignal', () => {
    const employee = buildEmployee();

    service.loadEmployees({ pageNumber: 1, pageSize: 10 });
    httpMock.expectOne((r) => r.url === API_URL).flush({
      status: 200,
      responseMessage: 'ok',
      data: { pageNumber: 1, pageSize: 10, totalItems: 1, items: [employee] },
    });

    service.removeEmployeeLocally(employee.employeeId);

    expect(service.employeesSignal()).toEqual([]);
  });
});
