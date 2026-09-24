import { provideHttpClient } from '@angular/common/http';
import {
  HttpTestingController,
  provideHttpClientTesting,
} from '@angular/common/http/testing';
import { ApplicationRef } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { environment } from '../../environments/environment';
import { Salary } from '../interfaces/salary';
import { NotificationService } from './notification.service';
import { SalaryHistoryService } from './salary-history.service';

describe('SalaryHistoryService', () => {
  let service: SalaryHistoryService;
  let httpMock: HttpTestingController;
  let notificationShow: ReturnType<typeof vi.fn>;

  const API_URL = `${environment.apiUrl}/api/employee/salary-history`;
  const urlFor = (employeeId: string) => `${API_URL}?employeeId=${employeeId}`;

  const buildEntry = (overrides: Partial<Salary> = {}): Salary => ({
    employeeSalaryId: 1,
    employeeId: 'employee-1',
    grossSalary: 7000,
    effectiveDate: '2026-01-01',
    createdAt: '2026-01-01T08:00:00Z',
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
    service = TestBed.inject(SalaryHistoryService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  const settle = () => TestBed.inject(ApplicationRef).whenStable();

  const load = (employeeId: string) => {
    service.loadSalaryHistory(employeeId);
    TestBed.tick();
  };

  it('makes no request until an employee is set', () => {
    TestBed.tick();

    httpMock.expectNone(() => true);
    expect(service.entries()).toEqual([]);
  });

  it("loads the employee's history", async () => {
    const entry = buildEntry();

    load('employee-1');
    httpMock
      .expectOne(urlFor('employee-1'))
      .flush({ status: 200, responseMessage: 'ok', data: [entry] });
    await settle();

    expect(service.entries()).toEqual([entry]);
    expect(service.error()).toBeNull();
  });

  it("shows the Problem Details' detail when the load fails", async () => {
    load('employee-1');
    httpMock
      .expectOne(urlFor('employee-1'))
      .flush(
        { status: 404, detail: 'Employee not found.' },
        { status: 404, statusText: 'Not Found' },
      );
    await settle();

    expect(service.entries()).toEqual([]);
    expect(service.error()).toBe('Employee not found.');
  });

  it('createSalary posts the entry, confirms with a toast and reloads the history', async () => {
    load('employee-1');
    httpMock
      .expectOne(urlFor('employee-1'))
      .flush({ status: 200, responseMessage: 'ok', data: [] });
    await settle();

    const request = {
      employeeId: 'employee-1',
      grossSalary: 7500,
      effectiveDate: '2026-06-01',
    };
    service.createSalary(request).subscribe();
    const post = httpMock.expectOne(API_URL);
    expect(post.request.method).toBe('POST');
    expect(post.request.body).toEqual(request);
    post.flush({ status: 201, responseMessage: 'Created' });
    TestBed.tick();

    httpMock.expectOne(urlFor('employee-1')).flush({
      status: 200,
      responseMessage: 'ok',
      data: [buildEntry({ grossSalary: 7500 })],
    });
    await settle();

    expect(notificationShow).toHaveBeenCalledWith(
      'Salary entry added successfully.',
    );
    expect(service.entries()[0].grossSalary).toBe(7500);
  });

  it('createSalarySilently neither toasts nor reloads', () => {
    service
      .createSalarySilently({
        employeeId: 'employee-2',
        grossSalary: 5000,
        effectiveDate: '2026-01-01',
      })
      .subscribe();
    httpMock
      .expectOne(API_URL)
      .flush({ status: 201, responseMessage: 'Created' });
    TestBed.tick();

    expect(notificationShow).not.toHaveBeenCalled();
    httpMock.expectNone(urlFor('employee-2'));
  });
});
