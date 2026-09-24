import { Component, computed, inject, input } from '@angular/core';
import { rxResource } from '@angular/core/rxjs-interop';
import { HttpErrorResponse } from '@angular/common/http';
import { RouterLink } from '@angular/router';
import { OfficeService } from '../../../services/office.service';
import { extractErrorMessage } from '../../../utils/extract-error-message';
import { employeeStatusLabel } from '../../../utils/employee-status-label';

// A dedicated page for one office (reached from the offices list, or directly by
// URL) — the full employees table, as opposed to that list's inline "quickly view"
// expansion, which only shows a compact name/email/status summary. Both reads are
// rxResources keyed on the route's id (like Imalo's ScholarDetailsComponent): a new
// id cancels whatever is still in flight, and reading value() is guarded by
// hasValue(), since it throws while a resource is in error.
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

  private readonly officeResource = rxResource({
    params: () => this.officeId(),
    stream: ({ params: officeId }) => this.officeService.getOffice(officeId),
  });
  readonly office = computed(() =>
    this.officeResource.hasValue() ? this.officeResource.value() : null,
  );
  readonly loading = this.officeResource.isLoading;
  readonly loadError = computed(() => {
    if (!this.officeId()) return 'No office specified.';
    const error = this.officeResource.error();
    return error
      ? extractErrorMessage(error as HttpErrorResponse, 'Failed to load office')
      : null;
  });

  private readonly employeesResource = rxResource({
    params: () => this.officeId(),
    stream: ({ params: officeId }) => this.officeService.getEmployees(officeId),
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
