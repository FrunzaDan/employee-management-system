import { HttpErrorResponse } from '@angular/common/http';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { of, throwError } from 'rxjs';
import { CostCenter } from '../../../interfaces/cost-center';
import { EmployeeSummary } from '../../../interfaces/employee-summary';
import { EmployeeStatus } from '../../../interfaces/employee';
import { CostCenterService } from '../../../services/cost-center.service';
import { CostCenterDetailsComponent } from './cost-center-details.component';

describe('CostCenterDetailsComponent', () => {
  let getCostCenter: ReturnType<typeof vi.fn>;
  let getEmployees: ReturnType<typeof vi.fn>;

  const costCenter: CostCenter = {
    costCenterId: 'cost-center-1',
    code: 'CC-100',
    name: 'Engineering',
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

  const render = async (costCenterId: string | undefined) => {
    TestBed.configureTestingModule({
      providers: [
        provideRouter([]),
        {
          provide: CostCenterService,
          useValue: { getCostCenter, getEmployees },
        },
      ],
    });
    const fixture = TestBed.createComponent(CostCenterDetailsComponent);
    fixture.componentRef.setInput('costCenterId', costCenterId);
    await fixture.whenStable();
    return fixture;
  };

  beforeEach(() => {
    getCostCenter = vi.fn().mockReturnValue(of(costCenter));
    getEmployees = vi.fn().mockReturnValue(of([employee]));
  });

  it('loads the cost center and its employees by the id from the route', async () => {
    const fixture = await render('cost-center-1');
    const component = fixture.componentInstance;

    expect(getCostCenter).toHaveBeenCalledWith('cost-center-1');
    expect(getEmployees).toHaveBeenCalledWith('cost-center-1');
    expect(component.costCenter()).toEqual(costCenter);
    expect(component.employees()).toEqual([employee]);
    expect(component.loadError()).toBeNull();
    expect(fixture.nativeElement.textContent).toContain('Ana');
  });

  it('says so, without calling the API, when no id is given', async () => {
    const fixture = await render(undefined);

    expect(fixture.componentInstance.loadError()).toBe(
      'No cost center specified.',
    );
    expect(getCostCenter).not.toHaveBeenCalled();
    expect(getEmployees).not.toHaveBeenCalled();
  });

  it("shows the API's reason when the cost center cannot be loaded", async () => {
    getCostCenter.mockReturnValue(
      throwError(
        () =>
          new HttpErrorResponse({
            status: 404,
            error: { status: 404, detail: 'Cost center not found.' },
          }),
      ),
    );

    const fixture = await render('cost-center-1');

    expect(fixture.componentInstance.loadError()).toBe(
      'Cost center not found.',
    );
    expect(fixture.nativeElement.textContent).toContain(
      'Cost center not found.',
    );
  });

  it('keeps the cost center visible when only its employees fail to load', async () => {
    getEmployees.mockReturnValue(
      throwError(() => new HttpErrorResponse({ status: 500 })),
    );

    const fixture = await render('cost-center-1');
    const component = fixture.componentInstance;

    expect(component.costCenter()).toEqual(costCenter);
    expect(component.employeesError()).toBe(
      'Failed to load employees (500). Please try again.',
    );
  });
});
