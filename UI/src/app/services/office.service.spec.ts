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

// DepartmentService and CostCenterService have the same shape; this pins it once.
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

  // The response is applied asynchronously, so wait for the app to settle
  // after flushing before asserting on the signals.
  const settle = () => TestBed.inject(ApplicationRef).whenStable();

  // httpResource issues its request from an effect, so flush effects after
  // calling loadOffices() before expecting the HTTP call.
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
    expect(service.offices()).toEqual([]); // the resource was never loaded
  });

  it('getOffice and getEmployees send the id as a query parameter', () => {
    service.getOffice('office-1').subscribe();
    const get = httpMock.expectOne(`${API_URL}/get?officeId=office-1`);
    get.flush({ status: 200, responseMessage: 'ok', data: buildOffice() });

    service.getEmployees('office-1').subscribe();
    httpMock
      .expectOne(`${API_URL}/employees?officeId=office-1`)
      .flush({ status: 200, responseMessage: 'ok', data: [] });
  });
});
