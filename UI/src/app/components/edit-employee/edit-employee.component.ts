import {
  Component,
  computed,
  effect,
  inject,
  input,
  linkedSignal,
  signal,
  untracked,
} from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { FormRoot, form } from '@angular/forms/signals';
import { Router, RouterLink } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { EditEmployeeService } from '../../services/edit-employee.service';
import { GetEmployeeService } from '../../services/get-employee.service';
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
  selector: 'app-edit-employee',
  templateUrl: './edit-employee.component.html',
  styleUrl: './edit-employee.component.css',
  imports: [EmployeeFormFieldsComponent, FormRoot, RouterLink],
  // Refresh / closing the tab isn't a router navigation, so guard it here too.
  host: { '(window:beforeunload)': 'onBeforeUnload($event)' },
})
export class EditEmployeeComponent {
  private readonly router = inject(Router);
  private readonly getEmployeeService = inject(GetEmployeeService);
  private readonly editEmployeeService = inject(EditEmployeeService);

  // Bound straight from `?id=` by withComponentInputBinding() in app.config.ts.
  readonly id = input<string>();

  readonly employee = this.getEmployeeService.selectedEmployeeSignal;
  readonly isLoading = this.getEmployeeService.loadingSignal;
  readonly errorMessage = this.getEmployeeService.errorSignal;

  // The form model *is* the loaded employee, mapped: it re-derives whenever
  // employee() changes and stays writable for the user's edits — no effect +
  // patchValue copy step.
  private readonly baseline = computed(() => {
    const employee = this.employee();
    return employee ? toFormModel(employee) : emptyEmployeeForm();
  });
  readonly model = linkedSignal(() => this.baseline());

  private readonly saved = signal(false);

  // Read by unsavedChangesGuard: edits that differ from the loaded employee and
  // haven't been saved. Putting a value back to the original clears it.
  readonly hasUnsavedChanges = computed(
    () => !this.saved() && isEmployeeFormDirty(this.model(), this.baseline()),
  );

  // Distinct from isLoading/errorMessage above, which reflect fetching the
  // employee being edited — these track the save (PATCH) request itself.
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

  constructor() {
    effect(() => {
      const id = this.id();
      if (id) untracked(() => this.getEmployeeService.getEmployee(id));
    });
  }

  private async save(): Promise<void> {
    const current = this.employee();
    if (!current) return;

    this.saveError.set(null);
    this.invalidSummary.set(null);

    try {
      await firstValueFrom(
        this.editEmployeeService.editEmployee(applyFormModel(this.model(), current)),
      );
      // Saved — leaving now must not trigger the unsaved-changes prompt.
      this.saved.set(true);
      await this.router.navigate(['/employees']);
    } catch (error) {
      this.saveError.set(
        extractErrorMessage(error as HttpErrorResponse, 'Failed to save changes'),
      );
    }
  }

  onBeforeUnload(event: BeforeUnloadEvent): void {
    if (this.hasUnsavedChanges()) event.preventDefault();
  }
}
