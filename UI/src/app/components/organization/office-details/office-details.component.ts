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
import { OfficeService } from '../../../services/office.service';
import { Office } from '../../../interfaces/office';
import { EmployeeSummary } from '../../../interfaces/employee-summary';
import { extractErrorMessage } from '../../../utils/extract-error-message';
import { employeeStatusLabel } from '../../../utils/employee-status-label';

// A dedicated page for one office (reached from the offices list, or directly by
// URL) — the full employees table, as opposed to that list's inline "quickly view"
// expansion, which only shows a compact name/email/status summary.
@Component({
  selector: 'app-office-details',
  templateUrl: './office-details.component.html',
  styleUrl: './office-details.component.css',
  imports: [RouterLink],
})
export class OfficeDetailsComponent {
  private readonly officeService = inject(OfficeService);

  // Bound from the `:officeId` route param by withComponentInputBinding() in
  // app.config.ts, same as EmployeeDetailsComponent.employeeId.
  readonly officeId = input<string>();

  readonly office = signal<Office | null>(null);
  readonly loading = signal(true);
  readonly error = signal<string | null>(null);

  readonly employees = signal<EmployeeSummary[]>([]);
  readonly employeesLoading = signal(true);
  readonly employeesError = signal<string | null>(null);

  readonly employeeStatusLabel = employeeStatusLabel;

  constructor() {
    effect(() => {
      const id = this.officeId();
      untracked(() => {
        if (!id) {
          this.error.set('No office specified.');
          this.loading.set(false);
          this.employeesLoading.set(false);
          return;
        }

        this.officeService.getOffice(id).subscribe({
          next: (office) => {
            this.office.set(office);
            this.loading.set(false);
          },
          error: (error: HttpErrorResponse) => {
            this.error.set(extractErrorMessage(error, 'Failed to load office'));
            this.loading.set(false);
          },
        });

        this.officeService.getEmployees(id).subscribe({
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
