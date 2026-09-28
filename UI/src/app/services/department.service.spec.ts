import { provideHttpClient } from '@angular/common/http';
import {
  HttpTestingController,
  provideHttpClientTesting,
} from '@angular/common/http/testing';
import { ApplicationRef } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { environment } from '../../environments/environment';
import { Department } from '../interfaces/department';
import { NotificationService } from './notification.service';
import { DepartmentService } from './department.service';

describe('DepartmentService', () => {
  let service: DepartmentService;
  let httpMock: HttpTestingController;
  let notificationShow: ReturnType<typeof vi.fn>;

  const API_URL = `${environment.apiUrl}/api/department`;
  const ALL_URL = `${API_URL}/all`;

  const buildDepartment = (
    overrides: Partial<Department> = {},
  ): Department => ({
    departmentId: 'department-1',
    name: 'Engineering',
    employeeCount: 3,
    totalGrossSalary: 21000,
    ...overrides,
  });

  beforeEach(() => {
    notificationShow = vi.fn();

    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        { provide: NotificationService, useValue: { show: notificationShow } },
      ],
    });
    service = TestBed.inject(DepartmentService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  const settle = () => TestBed.inject(ApplicationRef).whenStable();

  const load = () => {
    service.loadDepartments();
    TestBed.tick();
  };

  it('makes no request until loadDepartments() is called', () => {
    TestBed.tick();

    httpMock.expectNone(ALL_URL);
    expect(service.departments()).toEqual([]);
    expect(service.loading()).toBe(false);
  });

  it('populates departments from a successful response', async () => {
    const department = buildDepartment();

    load();
    httpMock
      .expectOne(ALL_URL)
      .flush({ status: 200, responseMessage: 'ok', data: [department] });
    await settle();

    expect(service.departments()).toEqual([department]);
    expect(service.error()).toBeNull();
  });

  it("shows the Problem Details' detail when the load fails", async () => {
    load();
    httpMock
      .expectOne(ALL_URL)
      .flush(
        { status: 500, detail: 'The database is unavailable.' },
        { status: 500, statusText: 'Server Error' },
      );
    await settle();

    expect(service.departments()).toEqual([]);
    expect(service.error()).toBe('The database is unavailable.');
  });

  it('falls back to a generic message naming what failed', async () => {
    load();
    httpMock
      .expectOne(ALL_URL)
      .flush(null, { status: 503, statusText: 'Service Unavailable' });
    await settle();

    expect(service.error()).toBe(
      'Failed to load departments (503). Please try again.',
    );
  });

  it('reloads the list and confirms with a toast after a create', async () => {
    load();
    httpMock
      .expectOne(ALL_URL)
      .flush({ status: 200, responseMessage: 'ok', data: [] });
    await settle();

    service.createDepartment({ name: 'Sales' }).subscribe();
    const create = httpMock.expectOne(`${API_URL}/create`);
    expect(create.request.method).toBe('POST');
    expect(create.request.body).toEqual({ name: 'Sales' });
    create.flush({ status: 201, responseMessage: 'Created' });
    TestBed.tick();

    httpMock.expectOne(ALL_URL).flush({
      status: 200,
      responseMessage: 'ok',
      data: [buildDepartment({ name: 'Sales' })],
    });
    await settle();

    expect(notificationShow).toHaveBeenCalledWith(
      'Department added successfully.',
    );
    expect(service.departments().map((o) => o.name)).toEqual(['Sales']);
  });

  it('fetchDepartments returns the list as a value, without touching the resource signals', () => {
    const department = buildDepartment();
    let result: Department[] | undefined;

    service
      .fetchDepartments()
      .subscribe((departments) => (result = departments));
    httpMock
      .expectOne(ALL_URL)
      .flush({ status: 200, responseMessage: 'ok', data: [department] });

    expect(result).toEqual([department]);
    expect(service.departments()).toEqual([]);
  });

  it('getDepartment and getEmployees send the id as a query parameter', () => {
    let item: Department | undefined;
    let employees: unknown[] | undefined;

    service.getDepartment('department-1').subscribe((o) => (item = o));
    const get = httpMock.expectOne(`${API_URL}/get?departmentId=department-1`);
    get.flush({ status: 200, responseMessage: 'ok', data: buildDepartment() });

    service.getEmployees('department-1').subscribe((e) => (employees = e));
    httpMock
      .expectOne(`${API_URL}/employees?departmentId=department-1`)
      .flush({ status: 200, responseMessage: 'ok', data: null });

    expect(item).toEqual(buildDepartment());
    expect(employees).toEqual([]);
  });

  it('getDepartment errors when the response has no department', () => {
    let error: Error | undefined;

    service.getDepartment('missing').subscribe({ error: (e) => (error = e) });
    httpMock
      .expectOne(`${API_URL}/get?departmentId=missing`)
      .flush({ status: 200, responseMessage: 'ok', data: null });

    expect(error?.message).toBe('Department not found.');
  });

  it('updateDepartment sends a PATCH, confirms with a toast and reloads the list', async () => {
    load();
    httpMock
      .expectOne(ALL_URL)
      .flush({ status: 200, responseMessage: 'ok', data: [] });
    await settle();

    service
      .updateDepartment({ departmentId: 'department-1', name: 'Renamed' })
      .subscribe();
    const update = httpMock.expectOne(`${API_URL}/update`);
    expect(update.request.method).toBe('PATCH');
    expect(update.request.body).toEqual({
      departmentId: 'department-1',
      name: 'Renamed',
    });
    update.flush({ status: 200, responseMessage: 'ok' });
    TestBed.tick();

    httpMock
      .expectOne(ALL_URL)
      .flush({ status: 200, responseMessage: 'ok', data: [] });
    await settle();

    expect(notificationShow).toHaveBeenCalledWith(
      'Department updated successfully.',
    );
  });

  it('deleteDepartment sends a DELETE with the id, confirms with a toast and reloads the list', async () => {
    load();
    httpMock
      .expectOne(ALL_URL)
      .flush({ status: 200, responseMessage: 'ok', data: [buildDepartment()] });
    await settle();

    service.deleteDepartment('department-1').subscribe();
    const remove = httpMock.expectOne(
      `${API_URL}/delete?departmentId=department-1`,
    );
    expect(remove.request.method).toBe('DELETE');
    remove.flush({ status: 200, responseMessage: 'ok' });
    TestBed.tick();

    httpMock
      .expectOne(ALL_URL)
      .flush({ status: 200, responseMessage: 'ok', data: [] });
    await settle();

    expect(notificationShow).toHaveBeenCalledWith(
      'Department deleted successfully.',
    );
    expect(service.departments()).toEqual([]);
  });

  it('does not toast or reload when a delete fails', () => {
    service
      .deleteDepartment('department-1')
      .subscribe({ error: () => undefined });
    httpMock
      .expectOne(`${API_URL}/delete?departmentId=department-1`)
      .flush(
        { status: 409, detail: 'Department still has employees.' },
        { status: 409, statusText: 'Conflict' },
      );
    TestBed.tick();

    httpMock.expectNone(ALL_URL);
    expect(notificationShow).not.toHaveBeenCalled();
  });
});
