import { provideHttpClient } from '@angular/common/http';
import {
  HttpTestingController,
  provideHttpClientTesting,
} from '@angular/common/http/testing';
import { ApplicationRef } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { firstValueFrom } from 'rxjs';
import { environment } from '../../environments/environment';
import {
  CreateEmployeeRequest,
  Employee,
  EmployeeStatus,
  Gender,
} from '../interfaces/employee';
import { EmployeeService } from './employee.service';
import { NotificationService } from './notification.service';

describe('EmployeeService', () => {
  let service: EmployeeService;
  let httpMock: HttpTestingController;
  let notificationShow: ReturnType<typeof vi.fn>;

  const API_URL = `${environment.apiUrl}/api/employee`;

  const buildEmployee = (overrides: Partial<Employee> = {}): Employee => ({
    employeeId: 'employee-1',
    firstName: 'Dan',
    lastName: 'Frunza',
    phoneNumber: '123456789',
    email: 'dan@example.com',
    gender: Gender.Male,
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
    hireDate: null,
    officeId: null,
    officeName: null,
    departmentId: null,
    departmentName: null,
    costCenterId: null,
    costCenterName: null,
    currentGrossSalary: null,
    ...overrides,
  });

  const settle = () => TestBed.inject(ApplicationRef).whenStable();

  const load = (params: Parameters<EmployeeService['loadEmployees']>[0]) => {
    service.loadEmployees(params);
    TestBed.tick();
  };

  const seedEmployees = async (employees: Employee[]) => {
    load({ pageNumber: 1, pageSize: 10 });
    httpMock
      .expectOne((r) => r.url === `${API_URL}/all`)
      .flush({
        status: 200,
        responseMessage: 'ok',
        data: {
          pageNumber: 1,
          pageSize: 10,
          totalItems: employees.length,
          items: employees,
        },
      });
    await settle();
  };

  beforeEach(() => {
    notificationShow = vi.fn();

    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: NotificationService, useValue: { show: notificationShow } },
      ],
    });
    service = TestBed.inject(EmployeeService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
    vi.restoreAllMocks();
    vi.useRealTimers();
  });

  describe('loadEmployees', () => {
    const emptyPage = (pageNumber: number) => ({
      status: 200,
      responseMessage: 'ok',
      data: { pageNumber, pageSize: 10, totalItems: 0, items: [] },
    });

    it('sends pageNumber, pageSize, sortColumn, and sortDirection as query params', () => {
      load({
        pageNumber: 2,
        pageSize: 10,
        sortColumn: 'email',
        sortDirection: 'desc',
      });

      const req = httpMock.expectOne((r) => r.url === `${API_URL}/all`);
      expect(req.request.params.get('pageNumber')).toBe('2');
      expect(req.request.params.get('pageSize')).toBe('10');
      expect(req.request.params.get('sortColumn')).toBe('email');
      expect(req.request.params.get('sortDirection')).toBe('desc');
      expect(req.request.params.has('searchTerm')).toBe(false);

      req.flush(emptyPage(2));
    });

    it('defaults sortColumn to name and sortDirection to asc when not provided', () => {
      load({ pageNumber: 1, pageSize: 10 });

      const req = httpMock.expectOne((r) => r.url === `${API_URL}/all`);
      expect(req.request.params.get('sortColumn')).toBe('name');
      expect(req.request.params.get('sortDirection')).toBe('asc');

      req.flush(emptyPage(1));
    });

    it('includes searchTerm only when a non-empty one is provided', () => {
      load({ pageNumber: 1, pageSize: 10, searchTerm: 'dan' });

      const req = httpMock.expectOne((r) => r.url === `${API_URL}/all`);
      expect(req.request.params.get('searchTerm')).toBe('dan');

      req.flush(emptyPage(1));
    });

    it('populates employees/totalItems/pageNumber/pageSize from a successful response', async () => {
      const employee = buildEmployee();

      await seedEmployees([employee]);

      expect(service.employees()).toEqual([employee]);
      expect(service.totalItems()).toBe(1);
      expect(service.pageNumber()).toBe(1);
      expect(service.pageSize()).toBe(10);
      expect(service.loading()).toBe(false);
      expect(service.error()).toBeNull();
    });

    it('sets loading true synchronously while the request is in flight', async () => {
      load({ pageNumber: 1, pageSize: 10 });

      expect(service.loading()).toBe(true);

      httpMock.expectOne((r) => r.url === `${API_URL}/all`).flush(emptyPage(1));
      await settle();

      expect(service.loading()).toBe(false);
    });

    it('sets a friendly message and clears loading on a network error (status 0)', async () => {
      load({ pageNumber: 1, pageSize: 10 });

      httpMock
        .expectOne((r) => r.url === `${API_URL}/all`)
        .error(new ProgressEvent('error'), { status: 0 });
      await settle();

      expect(service.loading()).toBe(false);
      expect(service.error()).toBe(
        'Could not reach the server. It may be offline, or your browser may not trust its security certificate.',
      );
    });
  });

  describe('loadEmployees (resource behaviour)', () => {
    const pageOf = (employees: Employee[], pageNumber = 1) => ({
      status: 200,
      responseMessage: 'ok',
      data: {
        pageNumber,
        pageSize: 10,
        totalItems: employees.length,
        items: employees,
      },
    });

    it('makes no request until loadEmployees() is called', () => {
      TestBed.tick();

      httpMock.expectNone((r) => r.url === `${API_URL}/all`);
      expect(service.employees()).toEqual([]);
      expect(service.loading()).toBe(false);
    });

    it('keeps the loaded page on screen while the next page loads', async () => {
      const first = buildEmployee();
      await seedEmployees([first]);
      expect(service.employees()).toEqual([first]);

      load({ pageNumber: 2, pageSize: 10 });

      expect(service.loading()).toBe(true);
      expect(service.employees()).toEqual([first]);

      const second = buildEmployee({ employeeId: 'employee-2' });
      httpMock
        .expectOne((r) => r.url === `${API_URL}/all`)
        .flush(pageOf([second], 2));
      await settle();

      expect(service.employees()).toEqual([second]);
      expect(service.pageNumber()).toBe(2);
    });

    it('cancels the request still in flight when a newer page is requested', async () => {
      load({ pageNumber: 1, pageSize: 10 });
      const stale = httpMock.expectOne((r) => r.url === `${API_URL}/all`);

      load({ pageNumber: 2, pageSize: 10 });

      expect(stale.cancelled).toBe(true);
      httpMock
        .expectOne((r) => r.url === `${API_URL}/all`)
        .flush(pageOf([], 2));
      await settle();
      expect(service.pageNumber()).toBe(2);
    });

    it('fetches the same page again when asked with the same params', async () => {
      await seedEmployees([buildEmployee()]);

      load({ pageNumber: 1, pageSize: 10 });

      httpMock.expectOne((r) => r.url === `${API_URL}/all`).flush(pageOf([]));
      await settle();
      expect(service.employees()).toEqual([]);
    });
  });

  describe('getEmployee', () => {
    it('GETs one employee by id and emits it', async () => {
      const employee = buildEmployee();

      const result = firstValueFrom(service.getEmployee(employee.employeeId));
      const req = httpMock.expectOne((r) => r.url === `${API_URL}/get`);
      expect(req.request.params.get('searchTerm')).toBe(employee.employeeId);
      req.flush({ status: 200, responseMessage: 'ok', data: employee });

      expect(await result).toEqual(employee);
    });

    it('errors when the response carries no employee', async () => {
      const result = firstValueFrom(service.getEmployee('employee-1'));
      httpMock
        .expectOne((r) => r.url === `${API_URL}/get`)
        .flush({ status: 200, responseMessage: 'ok', data: null });

      await expect(result).rejects.toThrow('Employee not found.');
    });
  });

  describe('createEmployee', () => {
    const buildRequest = (): CreateEmployeeRequest => ({
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

    it('POSTs the employee to the create endpoint', () => {
      service.createEmployee(buildRequest()).subscribe();

      const req = httpMock.expectOne(`${API_URL}/create`);
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual(buildRequest());

      req.flush({
        status: 200,
        responseMessage: 'Employee created successfully.',
      });
    });

    it('shows a success notification once the request resolves', () => {
      service.createEmployee(buildRequest()).subscribe();

      httpMock.expectOne(`${API_URL}/create`).flush({
        status: 200,
        responseMessage: 'Employee created successfully.',
      });

      expect(notificationShow).toHaveBeenCalledWith(
        'Employee added successfully.',
      );
    });

    it('createEmployeeSilently POSTs to the same endpoint without showing a notification', () => {
      service.createEmployeeSilently(buildRequest()).subscribe();

      const req = httpMock.expectOne(`${API_URL}/create`);
      expect(req.request.method).toBe('POST');
      req.flush({
        status: 200,
        responseMessage: 'Employee created successfully.',
      });

      expect(notificationShow).not.toHaveBeenCalled();
    });
  });

  describe('updateEmployee', () => {
    it('PATCHes only the editable fields to the update endpoint', () => {
      const employee = buildEmployee({
        hireDate: '2020-01-01',
        officeId: 'office-1',
        departmentId: 'department-1',
        costCenterId: 'cost-center-1',
      });
      service.updateEmployee(employee).subscribe();

      const req = httpMock.expectOne(`${API_URL}/update`);
      expect(req.request.method).toBe('PATCH');
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

      req.flush({
        status: 200,
        responseMessage: 'Employee updated successfully.',
      });
    });

    it('updates the employee in the loaded list and notifies on success', async () => {
      await seedEmployees([buildEmployee()]);
      const updated = buildEmployee({ firstName: 'Updated' });

      service.updateEmployee(updated).subscribe();
      httpMock.expectOne(`${API_URL}/update`).flush({
        status: 200,
        responseMessage: 'Employee updated successfully.',
      });

      expect(service.employees()).toEqual([updated]);
      expect(notificationShow).toHaveBeenCalledWith(
        'Employee updated successfully.',
      );
    });

    it('does not touch the loaded list or notify when the request errors', async () => {
      const original = buildEmployee();
      await seedEmployees([original]);

      service
        .updateEmployee(buildEmployee({ firstName: 'Updated' }))
        .subscribe({ error: () => {} });
      httpMock
        .expectOne(`${API_URL}/update`)
        .flush(
          { title: 'Bad Request', status: 400, detail: 'boom' },
          { status: 400, statusText: 'Bad Request' },
        );

      expect(service.employees()).toEqual([original]);
      expect(notificationShow).not.toHaveBeenCalled();
    });
  });

  describe('deleteEmployee', () => {
    it('DELETEs with the employeeId as a query param', () => {
      service.deleteEmployee('employee-1').subscribe();

      const req = httpMock.expectOne((r) => r.url === `${API_URL}/delete`);
      expect(req.request.method).toBe('DELETE');
      expect(req.request.params.get('employeeId')).toBe('employee-1');

      req.flush({
        status: 200,
        responseMessage: 'Employee deleted successfully.',
      });
    });

    it('removes the employee from the loaded list and notifies on success', async () => {
      await seedEmployees([buildEmployee()]);

      service.deleteEmployee('employee-1').subscribe();
      httpMock
        .expectOne((r) => r.url === `${API_URL}/delete`)
        .flush({
          status: 200,
          responseMessage: 'Employee deleted successfully.',
        });

      expect(service.employees()).toEqual([]);
      expect(notificationShow).toHaveBeenCalledWith(
        'Employee deleted successfully.',
      );
    });

    it('does not touch the loaded list or notify when the request errors', async () => {
      await seedEmployees([buildEmployee()]);

      service.deleteEmployee('employee-1').subscribe({ error: () => {} });
      httpMock
        .expectOne((r) => r.url === `${API_URL}/delete`)
        .flush(
          {
            title: 'Conflict',
            status: 409,
            detail: 'Employee must be deactivated before it can be deleted.',
          },
          { status: 409, statusText: 'Conflict' },
        );

      expect(service.employees()).toHaveLength(1);
      expect(notificationShow).not.toHaveBeenCalled();
    });

    it('deleteEmployeeSilently removes the employee from the loaded list, but never notifies', async () => {
      await seedEmployees([buildEmployee()]);

      service.deleteEmployeeSilently('employee-1').subscribe();
      const req = httpMock.expectOne((r) => r.url === `${API_URL}/delete`);
      expect(req.request.method).toBe('DELETE');
      expect(req.request.params.get('employeeId')).toBe('employee-1');
      req.flush({
        status: 200,
        responseMessage: 'Employee deleted successfully.',
      });

      expect(service.employees()).toEqual([]);
      expect(notificationShow).not.toHaveBeenCalled();
    });
  });

  describe('deactivateEmployee / reactivateEmployee', () => {
    beforeEach(() => {
      vi.spyOn(console, 'error').mockImplementation(() => {});
    });

    it('sets activationLoading true synchronously while deactivation is in flight', () => {
      service.deactivateEmployee('employee-1');

      expect(service.activationLoading()).toBe(true);

      httpMock
        .expectOne((r) => r.url === `${API_URL}/deactivate`)
        .flush({ status: 200, responseMessage: 'ok' });
    });

    it('deactivateEmployee marks the loaded employee Deactivated and notifies on success', async () => {
      await seedEmployees([buildEmployee()]);

      service.deactivateEmployee('employee-1');
      const req = httpMock.expectOne((r) => r.url === `${API_URL}/deactivate`);
      expect(req.request.method).toBe('PATCH');
      expect(req.request.params.get('employeeId')).toBe('employee-1');
      req.flush({ status: 200, responseMessage: 'ok' });

      expect(service.employees()[0].status).toBe(EmployeeStatus.Deactivated);
      expect(notificationShow).toHaveBeenCalledWith(
        'Employee deactivated successfully.',
      );
      expect(service.activationLoading()).toBe(false);
      expect(service.activationError()).toBeNull();
    });

    it('reactivateEmployee reloads the employee row so a Test record stays Test', async () => {
      await seedEmployees([
        buildEmployee({ status: EmployeeStatus.Deactivated }),
      ]);

      service.reactivateEmployee('employee-1');
      const req = httpMock.expectOne((r) => r.url === `${API_URL}/reactivate`);
      expect(req.request.method).toBe('PATCH');
      req.flush({ status: 200, responseMessage: 'ok' });

      const reload = httpMock.expectOne((r) => r.url === `${API_URL}/get`);
      expect(reload.request.params.get('searchTerm')).toBe('employee-1');
      reload.flush({
        status: 200,
        data: buildEmployee({ status: EmployeeStatus.Test }),
      });

      expect(service.employees()[0].status).toBe(EmployeeStatus.Test);
      expect(notificationShow).toHaveBeenCalledWith(
        'Employee reactivated successfully.',
      );
    });

    it('succeeds and notifies even when the employee is not in the loaded list (e.g. from the details page)', () => {
      service.deactivateEmployee('missing-employeeId');
      httpMock
        .expectOne((r) => r.url === `${API_URL}/deactivate`)
        .flush({ status: 200, responseMessage: 'ok' });

      expect(notificationShow).toHaveBeenCalled();
      expect(service.activationError()).toBeNull();
    });

    it('does not retry a definitive 4xx error and surfaces the server message', () => {
      service.deactivateEmployee('employee-1');

      httpMock
        .expectOne((r) => r.url === `${API_URL}/deactivate`)
        .flush(
          {
            title: 'Conflict',
            status: 409,
            detail: 'Employee is already deactivated.',
          },
          { status: 409, statusText: 'Conflict' },
        );

      expect(service.activationLoading()).toBe(false);
      expect(service.activationError()).toBe(
        'Employee is already deactivated.',
      );
    });

    it('retries once on a transient (5xx) failure and then succeeds', async () => {
      await seedEmployees([buildEmployee()]);
      vi.useFakeTimers();

      service.deactivateEmployee('employee-1');

      httpMock
        .expectOne((r) => r.url === `${API_URL}/deactivate`)
        .flush(null, { status: 500, statusText: 'Server Error' });

      vi.advanceTimersByTime(500);

      httpMock
        .expectOne((r) => r.url === `${API_URL}/deactivate`)
        .flush({ status: 200, responseMessage: 'ok' });

      expect(service.employees()[0].status).toBe(EmployeeStatus.Deactivated);
      expect(service.activationLoading()).toBe(false);
      expect(service.activationError()).toBeNull();
    });
  });

  describe('exportEmployees', () => {
    let triggerDownloadSpy: ReturnType<typeof vi.fn>;

    beforeEach(() => {
      triggerDownloadSpy = vi
        .spyOn(service as any, 'triggerDownload')
        .mockImplementation(() => {});
    });

    it('sends sortColumn and sortDirection as query params, defaulting when not provided', () => {
      service.exportEmployees({});

      const req = httpMock.expectOne((r) => r.url === `${API_URL}/export`);
      expect(req.request.params.get('sortColumn')).toBe('name');
      expect(req.request.params.get('sortDirection')).toBe('asc');
      expect(req.request.params.has('searchTerm')).toBe(false);
      expect(req.request.responseType).toBe('blob');

      req.flush(new Blob(['csv content']));
    });

    it('includes searchTerm only when a non-empty one is provided', () => {
      service.exportEmployees({
        searchTerm: 'dan',
        sortColumn: 'email',
        sortDirection: 'desc',
      });

      const req = httpMock.expectOne((r) => r.url === `${API_URL}/export`);
      expect(req.request.params.get('searchTerm')).toBe('dan');
      expect(req.request.params.get('sortColumn')).toBe('email');
      expect(req.request.params.get('sortDirection')).toBe('desc');

      req.flush(new Blob(['csv content']));
    });

    it('sets exportLoading true synchronously while the request is in flight, then false on success', () => {
      service.exportEmployees({});

      expect(service.exportLoading()).toBe(true);

      httpMock
        .expectOne((r) => r.url === `${API_URL}/export`)
        .flush(new Blob(['csv content']));

      expect(service.exportLoading()).toBe(false);
      expect(service.exportError()).toBeNull();
    });

    it('triggers a download with the received blob on success', () => {
      service.exportEmployees({});

      const blob = new Blob(['csv content']);
      httpMock.expectOne((r) => r.url === `${API_URL}/export`).flush(blob);

      expect(triggerDownloadSpy).toHaveBeenCalledWith(
        blob,
        expect.stringMatching(/^employees_.*\.csv$/),
      );
    });

    it('sets a friendly message and clears exportLoading on a network error (status 0)', () => {
      service.exportEmployees({});

      httpMock
        .expectOne((r) => r.url === `${API_URL}/export`)
        .error(new ProgressEvent('error'), { status: 0 });

      expect(service.exportLoading()).toBe(false);
      expect(service.exportError()).toBe(
        'Could not reach the server. It may be offline, or your browser may not trust its security certificate.',
      );
    });

    it('names the failed export on a server error (status 500)', async () => {
      service.exportEmployees({});

      httpMock
        .expectOne((r) => r.url === `${API_URL}/export`)
        .flush(new Blob(['error']), {
          status: 500,
          statusText: 'Server Error',
        });

      expect(service.exportLoading()).toBe(false);
      await vi.waitFor(() =>
        expect(service.exportError()).toBe(
          'Failed to export employees (500). Please try again.',
        ),
      );
    });

    it('shows the server detail from a problem body delivered as a blob', async () => {
      service.exportEmployees({});

      httpMock
        .expectOne((r) => r.url === `${API_URL}/export`)
        .flush(
          new Blob([
            JSON.stringify({ status: 400, detail: 'Narrow the search.' }),
          ]),
          { status: 400, statusText: 'Bad Request' },
        );

      await vi.waitFor(() =>
        expect(service.exportError()).toBe('Narrow the search.'),
      );
    });
  });
});
