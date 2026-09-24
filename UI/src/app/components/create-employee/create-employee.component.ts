import { Component, computed, inject, signal } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { FormRoot, form } from '@angular/forms/signals';
import { Router, RouterLink } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { EmployeeService } from '../../services/employee.service';
import { extractErrorMessage } from '../../utils/extract-error-message';
import {
  EmployeeFormModel,
  employeeFormSchema,
  emptyEmployeeForm,
  isEmployeeFormDirty,
  toCreateEmployeeRequest,
} from '../employee-form-fields/employee-form';
import { EmployeeFormFieldsComponent } from '../employee-form-fields/employee-form-fields.component';

@Component({
  selector: 'app-add-employee',
  templateUrl: './create-employee.component.html',
  styleUrl: './create-employee.component.css',
  imports: [EmployeeFormFieldsComponent, FormRoot, RouterLink],
  host: { '(window:beforeunload)': 'onBeforeUnload($event)' },
})
export class CreateEmployeeComponent {
  private readonly router = inject(Router);
  private readonly employeeService = inject(EmployeeService);

  readonly model = signal<EmployeeFormModel>(emptyEmployeeForm());
  private readonly saved = signal(false);

  readonly hasUnsavedChanges = computed(
    () =>
      !this.saved() && isEmployeeFormDirty(this.model(), emptyEmployeeForm()),
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
    this.saveError.set(null);
    this.invalidSummary.set(null);

    try {
      await firstValueFrom(
        this.employeeService.createEmployee(
          toCreateEmployeeRequest(this.model()),
        ),
      );
      this.saved.set(true);
      await this.router.navigate(['/employees']);
    } catch (error) {
      this.saveError.set(
        extractErrorMessage(
          error as HttpErrorResponse,
          'Failed to add employee',
        ),
      );
    }
  }

  onBeforeUnload(event: BeforeUnloadEvent): void {
    if (this.hasUnsavedChanges()) event.preventDefault();
  }
}
