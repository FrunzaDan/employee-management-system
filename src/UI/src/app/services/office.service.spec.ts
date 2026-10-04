import { provideHttpClient } from '@angular/common/http';
import {
  HttpTestingController,
  provideHttpClientTesting,
} from '@angular/common/http/testing';
import { ApplicationRef } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { environment } from '../../environments/environment';
import { Office } from '../interfaces/office';
import { NotificationService } from './notification.service';
import { OfficeService } from './office.service';

describe('OfficeService', () => {
  let service: OfficeService;
  let httpMock: HttpTestingController;
  let notificationShow: ReturnType<typeof vi.fn>;

  const API_URL = `${environment.apiUrl}/api/office`;
  const ALL_URL = `${API_URL}/all`;

  const buildOffice = (overrides: Partial<Office> = {}): Office => ({
    officeId: 'office-1',
    name: 'Head office',
    city: 'Bucharest',
    country: 'Romania',
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
    service = TestBed.inject(OfficeService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  const settle = () => TestBed.inject(ApplicationRef).whenStable();

  const load = () => {
    service.loadOffices();
    TestBed.tick();
  };

  it('makes no request until loadOffices() is called', () => {
    TestBed.tick();

    httpMock.expectNone(ALL_URL);
    expect(service.offices()).toEqual([]);
    expect(service.loading()).toBe(false);
  });

  it('populates offices from a successful response', async () => {
    const office = buildOffice();

    load();
    httpMock
      .expectOne(ALL_URL)
      .flush({ status: 200, responseMessage: 'ok', data: [office] });
    await settle();

    expect(service.offices()).toEqual([office]);
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

    expect(service.offices()).toEqual([]);
    expect(service.error()).toBe('The database is unavailable.');
  });

  it('falls back to a generic message naming what failed', async () => {
    load();
    httpMock
      .expectOne(ALL_URL)
      .flush(null, { status: 503, statusText: 'Service Unavailable' });
    await settle();

    expect(service.error()).toBe(
      'Failed to load offices (503). Please try again.',
    );
  });

  it('reloads the list and confirms with a toast after a create', async () => {
    load();
    httpMock
      .expectOne(ALL_URL)
      .flush({ status: 200, responseMessage: 'ok', data: [] });
    await settle();

    service.createOffice({ name: 'Branch' }).subscribe();
    const create = httpMock.expectOne(`${API_URL}/create`);
    expect(create.request.method).toBe('POST');
    expect(create.request.body).toEqual({ name: 'Branch' });
    create.flush({ status: 201, responseMessage: 'Created' });
    TestBed.tick();

    httpMock.expectOne(ALL_URL).flush({
      status: 200,
      responseMessage: 'ok',
      data: [buildOffice({ name: 'Branch' })],
    });
    await settle();

    expect(notificationShow).toHaveBeenCalledWith('Office added successfully.');
    expect(service.offices().map((o) => o.name)).toEqual(['Branch']);
  });

  it('fetchOffices returns the list as a value, without touching the resource signals', () => {
    const office = buildOffice();
    let result: Office[] | undefined;

    service.fetchOffices().subscribe((offices) => (result = offices));
    httpMock
      .expectOne(ALL_URL)
      .flush({ status: 200, responseMessage: 'ok', data: [office] });

    expect(result).toEqual([office]);
    expect(service.offices()).toEqual([]);
  });

  it('getOffice and getEmployees send the id as a query parameter', () => {
    let office: Office | undefined;
    let employees: unknown[] | undefined;

    service.getOffice('office-1').subscribe((o) => (office = o));
    const get = httpMock.expectOne(`${API_URL}/get?officeId=office-1`);
    get.flush({ status: 200, responseMessage: 'ok', data: buildOffice() });

    service.getEmployees('office-1').subscribe((e) => (employees = e));
    httpMock
      .expectOne(`${API_URL}/employees?officeId=office-1`)
      .flush({ status: 200, responseMessage: 'ok', data: null });

    expect(office).toEqual(buildOffice());
    expect(employees).toEqual([]);
  });

  it('getOffice errors when the response has no office', () => {
    let error: Error | undefined;

    service.getOffice('missing').subscribe({ error: (e) => (error = e) });
    httpMock
      .expectOne(`${API_URL}/get?officeId=missing`)
      .flush({ status: 200, responseMessage: 'ok', data: null });

    expect(error?.message).toBe('Office not found.');
  });

  it('updateOffice sends a PATCH, confirms with a toast and reloads the list', async () => {
    load();
    httpMock
      .expectOne(ALL_URL)
      .flush({ status: 200, responseMessage: 'ok', data: [] });
    await settle();

    service.updateOffice({ officeId: 'office-1', name: 'Renamed' }).subscribe();
    const update = httpMock.expectOne(`${API_URL}/update`);
    expect(update.request.method).toBe('PATCH');
    expect(update.request.body).toEqual({
      officeId: 'office-1',
      name: 'Renamed',
    });
    update.flush({ status: 200, responseMessage: 'ok' });
    TestBed.tick();

    httpMock
      .expectOne(ALL_URL)
      .flush({ status: 200, responseMessage: 'ok', data: [] });
    await settle();

    expect(notificationShow).toHaveBeenCalledWith(
      'Office updated successfully.',
    );
  });

  it('deleteOffice sends a DELETE with the id, confirms with a toast and reloads the list', async () => {
    load();
    httpMock
      .expectOne(ALL_URL)
      .flush({ status: 200, responseMessage: 'ok', data: [buildOffice()] });
    await settle();

    service.deleteOffice('office-1').subscribe();
    const remove = httpMock.expectOne(`${API_URL}/delete?officeId=office-1`);
    expect(remove.request.method).toBe('DELETE');
    remove.flush({ status: 200, responseMessage: 'ok' });
    TestBed.tick();

    httpMock
      .expectOne(ALL_URL)
      .flush({ status: 200, responseMessage: 'ok', data: [] });
    await settle();

    expect(notificationShow).toHaveBeenCalledWith(
      'Office deleted successfully.',
    );
    expect(service.offices()).toEqual([]);
  });

  it('does not toast or reload when a delete fails', () => {
    service.deleteOffice('office-1').subscribe({ error: () => undefined });
    httpMock
      .expectOne(`${API_URL}/delete?officeId=office-1`)
      .flush(
        { status: 409, detail: 'Office still has employees.' },
        { status: 409, statusText: 'Conflict' },
      );
    TestBed.tick();

    httpMock.expectNone(ALL_URL);
    expect(notificationShow).not.toHaveBeenCalled();
  });
});
