import {
  Component,
  computed,
  inject,
  input,
  linkedSignal,
  signal,
} from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { rxResource } from '@angular/core/rxjs-interop';
import { FormRoot, form } from '@angular/forms/signals';
import { Router, RouterLink } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { EmployeeService } from '../../services/employee.service';
import { extractErrorMessage } from '../../utils/extract-error-message';
import {
  employeeFormSchema,
  emptyEmployeeForm,
  isEmployeeFormDirty,
  applyFormModel,
  toFormModel,
} from '../employee-form-fields/employee-form';
import { EmployeeFormFieldsComponent } from '../employee-form-fields/employee-form-fields.component';

@Component({
  selector: 'app-update-employee',
  templateUrl: './update-employee.component.html',
  styleUrl: './update-employee.component.css',
  imports: [EmployeeFormFieldsComponent, FormRoot, RouterLink],
  host: { '(window:beforeunload)': 'onBeforeUnload($event)' },
})
export class UpdateEmployeeComponent {
  private readonly router = inject(Router);
  private readonly employeeService = inject(EmployeeService);

  readonly employeeId = input<string>();

  private readonly employeeResource = rxResource({
    params: () => this.employeeId(),
    stream: ({ params: employeeId }) =>
      this.employeeService.getEmployee(employeeId),
  });
  readonly employee = computed(() =>
    this.employeeResource.hasValue() ? this.employeeResource.value() : null,
  );
  readonly loading = this.employeeResource.isLoading;
  readonly loadError = computed(() => {
    const error = this.employeeResource.error();
    return error
      ? extractErrorMessage(
          error as HttpErrorResponse,
          'Failed to load the employee',
        )
      : null;
  });

  private readonly baseline = computed(() => {
    const employee = this.employee();
    return employee ? toFormModel(employee) : emptyEmployeeForm();
  });
  readonly model = linkedSignal(() => this.baseline());

  private readonly saved = signal(false);

  readonly hasUnsavedChanges = computed(
    () => !this.saved() && isEmployeeFormDirty(this.model(), this.baseline()),
  );

  readonly saveError = signal<string | null>(null);
  readonly invalidSummary = signal<string | null>(null);

  readonly employeeForm = form(this.model, employeeFormSchema, {
    submission: {
      action: () => this.save(),
      onInvalid: (field) => {
        const errors = field().errorSummary();
        this.invalidSummary.set(
          `The form has ${errors.length} ${errors.length === 1 ? 'error' : 'errors'}. Please correct the highlighted fields.`,
        );
        errors[0]?.fieldTree().focusBoundControl();
      },
    },
  });

  private async save(): Promise<void> {
    const current = this.employee();
    if (!current) return;

    this.saveError.set(null);
    this.invalidSummary.set(null);

    try {
      await firstValueFrom(
        this.employeeService.updateEmployee(
          applyFormModel(this.model(), current),
        ),
      );
      this.saved.set(true);
      await this.router.navigate(['/employees']);
    } catch (error) {
      this.saveError.set(
        extractErrorMessage(
          error as HttpErrorResponse,
          'Failed to save changes',
        ),
      );
    }
  }

  onBeforeUnload(event: BeforeUnloadEvent): void {
    if (this.hasUnsavedChanges()) event.preventDefault();
  }
}
