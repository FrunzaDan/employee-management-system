import { provideHttpClient } from '@angular/common/http';
import {
  HttpTestingController,
  provideHttpClientTesting,
} from '@angular/common/http/testing';
import { ApplicationRef } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { environment } from '../../environments/environment';
import { CostCenter } from '../interfaces/cost-center';
import { NotificationService } from './notification.service';
import { CostCenterService } from './cost-center.service';

describe('CostCenterService', () => {
  let service: CostCenterService;
  let httpMock: HttpTestingController;
  let notificationShow: ReturnType<typeof vi.fn>;

  const API_URL = `${environment.apiUrl}/api/cost-center`;
  const ALL_URL = `${API_URL}/all`;

  const buildCostCenter = (
    overrides: Partial<CostCenter> = {},
  ): CostCenter => ({
    costCenterId: 'cost-center-1',
    code: 'CC-100',
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
    service = TestBed.inject(CostCenterService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  const settle = () => TestBed.inject(ApplicationRef).whenStable();

  const load = () => {
    service.loadCostCenters();
    TestBed.tick();
  };

  it('makes no request until loadCostCenters() is called', () => {
    TestBed.tick();

    httpMock.expectNone(ALL_URL);
    expect(service.costCenters()).toEqual([]);
    expect(service.loading()).toBe(false);
  });

  it('populates cost centers from a successful response', async () => {
    const costCenter = buildCostCenter();

    load();
    httpMock
      .expectOne(ALL_URL)
      .flush({ status: 200, responseMessage: 'ok', data: [costCenter] });
    await settle();

    expect(service.costCenters()).toEqual([costCenter]);
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

    expect(service.costCenters()).toEqual([]);
    expect(service.error()).toBe('The database is unavailable.');
  });

  it('falls back to a generic message naming what failed', async () => {
    load();
    httpMock
      .expectOne(ALL_URL)
      .flush(null, { status: 503, statusText: 'Service Unavailable' });
    await settle();

    expect(service.error()).toBe(
      'Failed to load cost centers (503). Please try again.',
    );
  });

  it('reloads the list and confirms with a toast after a create', async () => {
    load();
    httpMock
      .expectOne(ALL_URL)
      .flush({ status: 200, responseMessage: 'ok', data: [] });
    await settle();

    service.createCostCenter({ code: 'CC-200' }).subscribe();
    const create = httpMock.expectOne(`${API_URL}/create`);
    expect(create.request.method).toBe('POST');
    expect(create.request.body).toEqual({ code: 'CC-200' });
    create.flush({ status: 201, responseMessage: 'Created' });
    TestBed.tick();

    httpMock.expectOne(ALL_URL).flush({
      status: 200,
      responseMessage: 'ok',
      data: [buildCostCenter({ code: 'CC-200' })],
    });
    await settle();

    expect(notificationShow).toHaveBeenCalledWith(
      'Cost center added successfully.',
    );
    expect(service.costCenters().map((o) => o.code)).toEqual(['CC-200']);
  });

  it('fetchCostCenters returns the list as a value, without touching the resource signals', () => {
    const costCenter = buildCostCenter();
    let result: CostCenter[] | undefined;

    service
      .fetchCostCenters()
      .subscribe((costCenters) => (result = costCenters));
    httpMock
      .expectOne(ALL_URL)
      .flush({ status: 200, responseMessage: 'ok', data: [costCenter] });

    expect(result).toEqual([costCenter]);
    expect(service.costCenters()).toEqual([]);
  });

  it('getCostCenter and getEmployees send the id as a query parameter', () => {
    let item: CostCenter | undefined;
    let employees: unknown[] | undefined;

    service.getCostCenter('cost-center-1').subscribe((o) => (item = o));
    const get = httpMock.expectOne(`${API_URL}/get?costCenterId=cost-center-1`);
    get.flush({ status: 200, responseMessage: 'ok', data: buildCostCenter() });

    service.getEmployees('cost-center-1').subscribe((e) => (employees = e));
    httpMock
      .expectOne(`${API_URL}/employees?costCenterId=cost-center-1`)
      .flush({ status: 200, responseMessage: 'ok', data: null });

    expect(item).toEqual(buildCostCenter());
    expect(employees).toEqual([]);
  });

  it('getCostCenter errors when the response has no cost center', () => {
    let error: Error | undefined;

    service.getCostCenter('missing').subscribe({ error: (e) => (error = e) });
    httpMock
      .expectOne(`${API_URL}/get?costCenterId=missing`)
      .flush({ status: 200, responseMessage: 'ok', data: null });

    expect(error?.message).toBe('Cost center not found.');
  });

  it('updateCostCenter sends a PATCH, confirms with a toast and reloads the list', async () => {
    load();
    httpMock
      .expectOne(ALL_URL)
      .flush({ status: 200, responseMessage: 'ok', data: [] });
    await settle();

    service
      .updateCostCenter({ costCenterId: 'cost-center-1', name: 'Renamed' })
      .subscribe();
    const update = httpMock.expectOne(`${API_URL}/update`);
    expect(update.request.method).toBe('PATCH');
    expect(update.request.body).toEqual({
      costCenterId: 'cost-center-1',
      name: 'Renamed',
    });
    update.flush({ status: 200, responseMessage: 'ok' });
    TestBed.tick();

    httpMock
      .expectOne(ALL_URL)
      .flush({ status: 200, responseMessage: 'ok', data: [] });
    await settle();

    expect(notificationShow).toHaveBeenCalledWith(
      'Cost center updated successfully.',
    );
  });

  it('deleteCostCenter sends a DELETE with the id, confirms with a toast and reloads the list', async () => {
    load();
    httpMock
      .expectOne(ALL_URL)
      .flush({ status: 200, responseMessage: 'ok', data: [buildCostCenter()] });
    await settle();

    service.deleteCostCenter('cost-center-1').subscribe();
    const remove = httpMock.expectOne(
      `${API_URL}/delete?costCenterId=cost-center-1`,
    );
    expect(remove.request.method).toBe('DELETE');
    remove.flush({ status: 200, responseMessage: 'ok' });
    TestBed.tick();

    httpMock
      .expectOne(ALL_URL)
      .flush({ status: 200, responseMessage: 'ok', data: [] });
    await settle();

    expect(notificationShow).toHaveBeenCalledWith(
      'Cost center deleted successfully.',
    );
    expect(service.costCenters()).toEqual([]);
  });

  it('does not toast or reload when a delete fails', () => {
    service
      .deleteCostCenter('cost-center-1')
      .subscribe({ error: () => undefined });
    httpMock
      .expectOne(`${API_URL}/delete?costCenterId=cost-center-1`)
      .flush(
        { status: 409, detail: 'Cost center still has employees.' },
        { status: 409, statusText: 'Conflict' },
      );
    TestBed.tick();

    httpMock.expectNone(ALL_URL);
    expect(notificationShow).not.toHaveBeenCalled();
  });
});
