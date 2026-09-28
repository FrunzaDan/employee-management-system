import { provideHttpClient } from '@angular/common/http';
import {
  HttpTestingController,
  provideHttpClientTesting,
} from '@angular/common/http/testing';
import { ApplicationRef } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { environment } from '../../environments/environment';
import { EmployeeInsights } from '../interfaces/employee-insights';
import { EmployeeInsightsService } from './employee-insights.service';

describe('EmployeeInsightsService', () => {
  let service: EmployeeInsightsService;
  let httpMock: HttpTestingController;

  const API_URL = `${environment.apiUrl}/api/employee/insights`;
  const EMPTY = { employees: [] };

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    service = TestBed.inject(EmployeeInsightsService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  const load = () => {
    service.loadInsights();
    TestBed.tick();
  };

  const settle = () => TestBed.inject(ApplicationRef).whenStable();

  it('makes no request until loadInsights() is called', () => {
    TestBed.tick();

    httpMock.expectNone(API_URL);
    expect(service.insights()).toEqual(EMPTY);
    expect(service.loading()).toBe(false);
  });

  it('populates insights from a successful response', async () => {
    const insights: EmployeeInsights = {
      employees: [
        {
          status: 1901,
          gender: 2,
          birthDate: '1990-04-01',
          hireDate: '2012-05-20',
          departmentName: 'Engineering',
          officeName: 'HQ',
          currentGrossSalary: 8000,
        },
      ],
    };

    load();
    httpMock
      .expectOne(API_URL)
      .flush({ status: 200, responseMessage: 'ok', data: insights });
    await settle();

    expect(service.insights()).toEqual(insights);
    expect(service.error()).toBeNull();
  });

  it('reloads on a second loadInsights() call, so the page shows fresh data', async () => {
    load();
    httpMock
      .expectOne(API_URL)
      .flush({ status: 200, responseMessage: 'ok', data: EMPTY });
    await settle();

    load();

    httpMock
      .expectOne(API_URL)
      .flush({ status: 200, responseMessage: 'ok', data: EMPTY });
    await settle();
  });

  it('falls back to empty insights when the response has no data', async () => {
    load();
    httpMock
      .expectOne(API_URL)
      .flush({ status: 200, responseMessage: 'ok', data: undefined });
    await settle();

    expect(service.insights()).toEqual(EMPTY);
  });

  it('surfaces the server-provided error message when present', async () => {
    load();

    httpMock
      .expectOne(API_URL)
      .flush(
        { title: 'Server Error', status: 500, detail: 'boom' },
        { status: 500, statusText: 'Server Error' },
      );
    await settle();

    expect(service.loading()).toBe(false);
    expect(service.error()).toBe('boom');
    expect(service.insights()).toEqual(EMPTY);
  });
});
