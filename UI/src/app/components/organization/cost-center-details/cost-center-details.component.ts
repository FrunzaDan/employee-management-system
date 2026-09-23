import {
  Component,
  effect,
  inject,
  input,
  signal,
  untracked,
} from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { RouterLink } from '@angular/router';
import { CostCenterService } from '../../../services/cost-center.service';
import { CostCenter } from '../../../interfaces/cost-center-response';
import { EmployeeSummary } from '../../../interfaces/employee-summary-response';
import { extractErrorMessage } from '../../../utils/extract-error-message';
import { employeeStatusLabel } from '../../../utils/employee-status-label';

// See OfficeDetailsComponent for why this page exists alongside the cost centers
// list's inline "quickly view" expansion.
@Component({
  selector: 'app-cost-center-details',
  templateUrl: './cost-center-details.component.html',
  styleUrl: './cost-center-details.component.css',
  imports: [RouterLink],
})
export class CostCenterDetailsComponent {
  private readonly costCenterService = inject(CostCenterService);

  readonly costCenterId = input<string>();

  readonly costCenter = signal<CostCenter | null>(null);
  readonly loading = signal(true);
  readonly error = signal<string | null>(null);

  readonly employees = signal<EmployeeSummary[]>([]);
  readonly employeesLoading = signal(true);
  readonly employeesError = signal<string | null>(null);

  readonly employeeStatusLabel = employeeStatusLabel;

  constructor() {
    effect(() => {
      const id = this.costCenterId();
      untracked(() => {
        if (!id) {
          this.error.set('No cost center specified.');
          this.loading.set(false);
          this.employeesLoading.set(false);
          return;
        }

        this.costCenterService.getCostCenter(id).subscribe({
          next: (costCenter) => {
            this.costCenter.set(costCenter);
            this.loading.set(false);
          },
          error: (error: HttpErrorResponse) => {
            this.error.set(
              extractErrorMessage(error, 'Failed to load cost center'),
            );
            this.loading.set(false);
          },
        });

        this.costCenterService.getEmployees(id).subscribe({
          next: (employees) => {
            this.employees.set(employees);
            this.employeesLoading.set(false);
          },
          error: (error: HttpErrorResponse) => {
            this.employeesError.set(
              extractErrorMessage(error, 'Failed to load employees'),
            );
            this.employeesLoading.set(false);
          },
        });
      });
    });
  }
}
