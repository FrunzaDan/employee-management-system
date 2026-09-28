import { HttpErrorResponse } from '@angular/common/http';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { of, throwError } from 'rxjs';
import { Office } from '../../../interfaces/office';
import { EmployeeSummary } from '../../../interfaces/employee-summary';
import { EmployeeStatus } from '../../../interfaces/employee';
import { OfficeService } from '../../../services/office.service';
import { OfficeDetailsComponent } from './office-details.component';

describe('OfficeDetailsComponent', () => {
  let getOffice: ReturnType<typeof vi.fn>;
  let getEmployees: ReturnType<typeof vi.fn>;

  const office: Office = {
    officeId: 'office-1',
    name: 'Head office',
    city: 'Cluj',
    country: 'Romania',
    employeeCount: 1,
    totalGrossSalary: 6000,
  };

  const employee: EmployeeSummary = {
    employeeId: 'employee-1',
    firstName: 'Ana',
    lastName: 'Pop',
    email: 'ana@example.com',
    status: EmployeeStatus.Active,
  };

  const render = async (officeId: string | undefined) => {
    TestBed.configureTestingModule({
      providers: [
        provideRouter([]),
        {
          provide: OfficeService,
          useValue: { getOffice, getEmployees },
        },
      ],
    });
    const fixture = TestBed.createComponent(OfficeDetailsComponent);
    fixture.componentRef.setInput('officeId', officeId);
    await fixture.whenStable();
    return fixture;
  };

  beforeEach(() => {
    getOffice = vi.fn().mockReturnValue(of(office));
    getEmployees = vi.fn().mockReturnValue(of([employee]));
  });

  it('loads the office and its employees by the id from the route', async () => {
    const fixture = await render('office-1');
    const component = fixture.componentInstance;

    expect(getOffice).toHaveBeenCalledWith('office-1');
    expect(getEmployees).toHaveBeenCalledWith('office-1');
    expect(component.office()).toEqual(office);
    expect(component.employees()).toEqual([employee]);
    expect(component.loadError()).toBeNull();
    expect(fixture.nativeElement.textContent).toContain('Ana');
  });

  it('says so, without calling the API, when no id is given', async () => {
    const fixture = await render(undefined);

    expect(fixture.componentInstance.loadError()).toBe('No office specified.');
    expect(getOffice).not.toHaveBeenCalled();
    expect(getEmployees).not.toHaveBeenCalled();
  });

  it("shows the API's reason when the office cannot be loaded", async () => {
    getOffice.mockReturnValue(
      throwError(
        () =>
          new HttpErrorResponse({
            status: 404,
            error: { status: 404, detail: 'Office not found.' },
          }),
      ),
    );

    const fixture = await render('office-1');

    expect(fixture.componentInstance.loadError()).toBe('Office not found.');
    expect(fixture.nativeElement.textContent).toContain('Office not found.');
  });

  it('keeps the office visible when only its employees fail to load', async () => {
    getEmployees.mockReturnValue(
      throwError(() => new HttpErrorResponse({ status: 500 })),
    );

    const fixture = await render('office-1');
    const component = fixture.componentInstance;

    expect(component.office()).toEqual(office);
    expect(component.employeesError()).toBe(
      'Failed to load employees (500). Please try again.',
    );
  });
});
