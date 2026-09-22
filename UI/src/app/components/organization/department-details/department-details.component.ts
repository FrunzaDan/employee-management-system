import { Component, effect, inject, input, signal, untracked } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { RouterLink } from '@angular/router';
import { DepartmentService } from '../../../services/department.service';
import { Department } from '../../../interfaces/department-response';
import { EmployeeSummary } from '../../../interfaces/employee-summary-response';
import { extractErrorMessage } from '../../../utils/extract-error-message';
import { employeeStatusLabel } from '../../../utils/employee-status-label';

// See OfficeDetailsComponent for why this page exists alongside the departments
// list's inline "quickly view" expansion.
@Component({
  selector: 'app-department-details',
  templateUrl: './department-details.component.html',
  styleUrls: ['./department-details.component.css'],
  imports: [RouterLink],
})
export class DepartmentDetailsComponent {
  private readonly departmentService = inject(DepartmentService);

  readonly id = input<string>();

  readonly department = signal<Department | null>(null);
  readonly loading = signal(true);
  readonly error = signal<string | null>(null);

  readonly employees = signal<EmployeeSummary[]>([]);
  readonly employeesLoading = signal(true);
  readonly employeesError = signal<string | null>(null);

  readonly employeeStatusLabel = employeeStatusLabel;

  constructor() {
    effect(() => {
      const id = this.id();
      untracked(() => {
        if (!id) {
          this.error.set('No department specified.');
          this.loading.set(false);
          this.employeesLoading.set(false);
          return;
        }

        this.departmentService.getDepartment(id).subscribe({
          next: (department) => {
            this.department.set(department);
            this.loading.set(false);
          },
          error: (error: HttpErrorResponse) => {
            this.error.set(extractErrorMessage(error, 'Failed to load department'));
            this.loading.set(false);
          },
        });

        this.departmentService.getEmployees(id).subscribe({
          next: (employees) => {
            this.employees.set(employees);
            this.employeesLoading.set(false);
          },
          error: (error: HttpErrorResponse) => {
            this.employeesError.set(extractErrorMessage(error, 'Failed to load employees'));
            this.employeesLoading.set(false);
          },
        });
      });
    });
  }
}
