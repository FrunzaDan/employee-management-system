import { Component, computed, inject, input } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { HttpErrorResponse } from '@angular/common/http';
import { RouterLink } from '@angular/router';
import { CostCenterService } from '../../../services/cost-center.service';
import { extractErrorMessage } from '../../../utils/extract-error-message';
import { employeeStatusLabel } from '../../../utils/employee-status-label';

// See OfficeDetailsComponent for why this page exists alongside the cost centers
// list's inline "quickly view" expansion, and for how it loads.
@Component({
  selector: 'app-cost-center-details',
  templateUrl: './cost-center-details.component.html',
  styleUrl: './cost-center-details.component.css',
  imports: [RouterLink],
})
export class CostCenterDetailsComponent {
  private readonly costCenterService = inject(CostCenterService);

  readonly costCenterId = input<string>();

  private readonly costCenterResource = rxResource({
    params: () => this.costCenterId(),
    stream: ({ params: costCenterId }) =>
      this.costCenterService.getCostCenter(costCenterId),
  });
  readonly costCenter = computed(() =>
    this.costCenterResource.hasValue() ? this.costCenterResource.value() : null,
  );
  readonly loading = this.costCenterResource.isLoading;
  readonly loadError = computed(() => {
    if (!this.costCenterId()) return 'No cost center specified.';
    const error = this.costCenterResource.error();
    return error
      ? extractErrorMessage(
          error as HttpErrorResponse,
          'Failed to load cost center',
        )
      : null;
  });

  private readonly employeesResource = rxResource({
    params: () => this.costCenterId(),
    stream: ({ params: costCenterId }) =>
      this.costCenterService.getEmployees(costCenterId),
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
