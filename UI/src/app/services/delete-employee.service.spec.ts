import { provideHttpClient } from '@angular/common/http';
import {
  HttpTestingController,
  provideHttpClientTesting,
} from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { environment } from '../../environments/environment';
import { DeleteEmployeeService } from './delete-employee.service';
import { GetEmployeeService } from './get-employee.service';
import { HttpHeaderService } from './http-header.service';
import { NotificationService } from './notification.service';

describe('DeleteEmployeeService', () => {
  let service: DeleteEmployeeService;
  let httpMock: HttpTestingController;
  let removeEmployeeLocally: ReturnType<typeof vi.fn>;
  let notificationShow: ReturnType<typeof vi.fn>;

  const API_URL = `${environment.apiUrl}/api/employee/delete`;

  beforeEach(() => {
    removeEmployeeLocally = vi.fn();
    notificationShow = vi.fn();

    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        {
          provide: HttpHeaderService,
          useValue: { getHeadersWithTokenSet: () => ({}) },
        },
        { provide: GetEmployeeService, useValue: { removeEmployeeLocally } },
        { provide: NotificationService, useValue: { show: notificationShow } },
      ],
    });
    service = TestBed.inject(DeleteEmployeeService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  describe('deleteEmployee', () => {
    it('DELETEs with the employeeId as a query param', () => {
      service.deleteEmployee('employeeId-1').subscribe();

      const req = httpMock.expectOne((r) => r.url === API_URL);
      expect(req.request.method).toBe('DELETE');
      expect(req.request.params.get('employeeId')).toBe('employeeId-1');

      req.flush({ status: 200, responseMessage: 'Employee deleted successfully.' });
    });

    it('removes the employee from the local cache and notifies on success', () => {
      service.deleteEmployee('employeeId-1').subscribe();

      httpMock
        .expectOne((r) => r.url === API_URL)
        .flush({ status: 200, responseMessage: 'Employee deleted successfully.' });

      expect(removeEmployeeLocally).toHaveBeenCalledWith('employeeId-1');
      expect(notificationShow).toHaveBeenCalledWith('Employee deleted successfully.');
    });

    it('does not touch the local cache or notify when the request errors', () => {
      service.deleteEmployee('employeeId-1').subscribe({ error: () => {} });

      httpMock
        .expectOne((r) => r.url === API_URL)
        .flush(
          { message: 'Employee must be deactivated before it can be deleted.' },
          { status: 409, statusText: 'Conflict' },
        );

      expect(removeEmployeeLocally).not.toHaveBeenCalled();
      expect(notificationShow).not.toHaveBeenCalled();
    });
  });

  describe('deleteEmployeeSilently', () => {
    it('DELETEs the same endpoint and updates the local cache, but never notifies', () => {
      service.deleteEmployeeSilently('employeeId-1').subscribe();

      const req = httpMock.expectOne((r) => r.url === API_URL);
      expect(req.request.method).toBe('DELETE');
      expect(req.request.params.get('employeeId')).toBe('employeeId-1');
      req.flush({ status: 200, responseMessage: 'Employee deleted successfully.' });

      expect(removeEmployeeLocally).toHaveBeenCalledWith('employeeId-1');
      expect(notificationShow).not.toHaveBeenCalled();
    });
  });
});
