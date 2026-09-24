import { Component, computed, inject, input } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { HttpErrorResponse } from '@angular/common/http';
import { RouterLink } from '@angular/router';
import { DepartmentService } from '../../../services/department.service';
import { extractErrorMessage } from '../../../utils/extract-error-message';
import { employeeStatusLabel } from '../../../utils/employee-status-label';

@Component({
  selector: 'app-department-details',
  templateUrl: './department-details.component.html',
  styleUrl: './department-details.component.css',
  imports: [RouterLink],
})
export class DepartmentDetailsComponent {
  private readonly departmentService = inject(DepartmentService);

  readonly departmentId = input<string>();

  private readonly departmentResource = rxResource({
    params: () => this.departmentId(),
    stream: ({ params: departmentId }) =>
      this.departmentService.getDepartment(departmentId),
  });
  readonly department = computed(() =>
    this.departmentResource.hasValue() ? this.departmentResource.value() : null,
  );
  readonly loading = this.departmentResource.isLoading;
  readonly loadError = computed(() => {
    if (!this.departmentId()) return 'No department specified.';
    const error = this.departmentResource.error();
    return error
      ? extractErrorMessage(
          error as HttpErrorResponse,
          'Failed to load department',
        )
      : null;
  });

  private readonly employeesResource = rxResource({
    params: () => this.departmentId(),
    stream: ({ params: departmentId }) =>
      this.departmentService.getEmployees(departmentId),
  });
  readonly employees = computed(() =>
    this.employeesResource.hasValue() ? this.employeesResource.value() : [],
  );
  readonly employeesLoading = this.employeesResource.isLoading;
  readonly employeesError = computed(() => {
    const error = this.employeesResource.error();
    return error
      ? extractErrorMessage(
          error as HttpErrorResponse,
          'Failed to load employees',
        )
      : null;
  });

  readonly employeeStatusLabel = employeeStatusLabel;
}
