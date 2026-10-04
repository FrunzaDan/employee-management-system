import { HttpErrorResponse } from '@angular/common/http';
import { Component, input } from '@angular/core';
import { By } from '@angular/platform-browser';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { of, throwError } from 'rxjs';
import { Department } from '../../../interfaces/department';
import { EmployeeSummary } from '../../../interfaces/employee-summary';
import { EmployeeStatus } from '../../../interfaces/employee';
import { DepartmentService } from '../../../services/department.service';
import { EmployeeListComponent } from '../../employee-list/employee-list.component';
import { DepartmentDetailsComponent } from './department-details.component';

@Component({ selector: 'app-employee-list', template: '' })
class EmployeeListStub {
  readonly departmentId = input<string>();
  readonly emptyMessage = input<string>();
}

describe('DepartmentDetailsComponent', () => {
  let getDepartment: ReturnType<typeof vi.fn>;
  let getEmployees: ReturnType<typeof vi.fn>;

  const department: Department = {
    departmentId: 'department-1',
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

  const render = async (departmentId: string | undefined) => {
    TestBed.configureTestingModule({
      providers: [
        provideRouter([]),
        {
          provide: DepartmentService,
          useValue: { getDepartment, getEmployees },
        },
      ],
    });
    TestBed.overrideComponent(DepartmentDetailsComponent, {
      remove: { imports: [EmployeeListComponent] },
      add: { imports: [EmployeeListStub] },
    });
    const fixture = TestBed.createComponent(DepartmentDetailsComponent);
    fixture.componentRef.setInput('departmentId', departmentId);
    await fixture.whenStable();
    return fixture;
  };

  beforeEach(() => {
    getDepartment = vi.fn().mockReturnValue(of(department));
    getEmployees = vi.fn().mockReturnValue(of([employee]));
  });

  it('loads the department and its employees by the id from the route', async () => {
    const fixture = await render('department-1');
    const component = fixture.componentInstance;

    expect(getDepartment).toHaveBeenCalledWith('department-1');
    expect(getEmployees).toHaveBeenCalledWith('department-1');
    expect(component.department()).toEqual(department);
    expect(component.employees()).toEqual([employee]);
    expect(component.loadError()).toBeNull();
    const list = fixture.debugElement.query(By.directive(EmployeeListStub));
    expect(list.componentInstance.departmentId()).toBe('department-1');
  });

  it('says so, without calling the API, when no id is given', async () => {
    const fixture = await render(undefined);

    expect(fixture.componentInstance.loadError()).toBe(
      'No department specified.',
    );
    expect(getDepartment).not.toHaveBeenCalled();
    expect(getEmployees).not.toHaveBeenCalled();
  });

  it("shows the API's reason when the department cannot be loaded", async () => {
    getDepartment.mockReturnValue(
      throwError(
        () =>
          new HttpErrorResponse({
            status: 404,
            error: { status: 404, detail: 'Department not found.' },
          }),
      ),
    );

    const fixture = await render('department-1');

    expect(fixture.componentInstance.loadError()).toBe('Department not found.');
    expect(fixture.nativeElement.textContent).toContain(
      'Department not found.',
    );
  });

  it('keeps the department visible when only its employees fail to load', async () => {
    getEmployees.mockReturnValue(
      throwError(() => new HttpErrorResponse({ status: 500 })),
    );

    const fixture = await render('department-1');
    const component = fixture.componentInstance;

    expect(component.department()).toEqual(department);
    expect(component.employeesError()).toBe(
      'Failed to load employees (500). Please try again.',
    );
  });
});
