import {
  Component,
  computed,
  effect,
  inject,
  input,
  signal,
  untracked,
} from '@angular/core';
import { DatePipe } from '@angular/common';
import { rxResource } from '@angular/core/rxjs-interop';
import { RonPipe } from '../../pipes/ron.pipe';
import { HttpErrorResponse } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
import { EmployeeService } from '../../services/employee.service';
import { AuditLogService } from '../../services/audit-log.service';
import { ConfirmDialogService } from '../../services/confirm-dialog.service';
import { SalaryHistoryService } from '../../services/salary-history.service';
import { Employee, EmployeeStatus, Gender } from '../../interfaces/employee';
import { Router, RouterLink } from '@angular/router';
import { extractErrorMessage } from '../../utils/extract-error-message';
import { auditActionLabel } from '../../utils/audit-action-label';
import { employeeStatusLabel } from '../../utils/employee-status-label';

const GENDER_LABELS = new Map<Gender, string>([
  [Gender.NotDeclared, 'not declared'],
  [Gender.Male, 'male'],
  [Gender.Female, 'female'],
]);

@Component({
  selector: 'app-employee-details',
  templateUrl: './employee-details.component.html',
  styleUrl: './employee-details.component.css',
  imports: [DatePipe, RonPipe, RouterLink],
})
export class EmployeeDetailsComponent {
  private readonly employeeService = inject(EmployeeService);
  private readonly confirmDialogService = inject(ConfirmDialogService);
  private readonly auditLogService = inject(AuditLogService);
  private readonly salaryHistoryService = inject(SalaryHistoryService);
  private readonly router = inject(Router);

  // Bound from the `:employeeId` route param by withComponentInputBinding() in app.config.ts.
  readonly employeeId = input<string>();

  // Keyed on the route's id, like Imalo's ScholarDetailsComponent: a new id
  // cancels whatever is still in flight. hasValue() guards the read, since
  // value() throws while the resource is in error.
  private readonly employeeResource = rxResource({
    params: () => this.employeeId(),
    stream: ({ params: employeeId }) =>
      this.employeeService.getEmployee(employeeId),
  });
  readonly employee = computed(() =>
    this.employeeResource.hasValue() ? this.employeeResource.value() : null,
  );
  // The first load only: a reload (after a status change) keeps the page on
  // screen until the fresh copy arrives.
  readonly loading = computed(
    () => this.employeeResource.status() === 'loading',
  );
  readonly loadError = computed(() => {
    const error = this.employeeResource.error();
    return error
      ? extractErrorMessage(
          error as HttpErrorResponse,
          'Failed to load the employee',
        )
      : null;
  });

  readonly EmployeeStatus = EmployeeStatus;
  readonly auditActionLabel = auditActionLabel;
  readonly Gender = Gender;

  // Deactivate/reactivate share EmployeeService's loading/error state (it's
  // providedIn: 'root', same instance the employee list uses); delete gets its own,
  // same split as employee-list.component.ts.
  readonly activationLoading = this.employeeService.activationLoading;
  readonly activationError = this.employeeService.activationError;
  readonly deleting = signal(false);
  readonly deleteError = signal<string | null>(null);

  readonly auditLog = this.auditLogService.entries;
  readonly auditLogLoading = this.auditLogService.loading;
  readonly auditLogError = this.auditLogService.error;
  private wasActivationLoading = false;

  readonly salaryHistory = this.salaryHistoryService.entries;
  readonly salaryHistoryLoading = this.salaryHistoryService.loading;
  readonly salaryHistoryError = this.salaryHistoryService.error;

  // Draft state for the inline "add salary entry" form — deliberately not a
  // signal-forms FieldTree like employee-form.ts: two plain fields, no shared
  // schema/validation needed across pages.
  readonly newSalaryAmount = signal('');
  readonly newSalaryEffectiveDate = signal('');
  readonly addingSalary = signal(false);
  readonly createSalaryError = signal<string | null>(null);

  readonly genderLabel = computed(() => {
    const employee = this.employee();
    return employee ? GENDER_LABELS.get(employee.gender) : undefined;
  });

  readonly statusLabel = computed(() => {
    const employee = this.employee();
    return employee ? employeeStatusLabel(employee.status) : undefined;
  });

  // Deactivated employees follow the normal deactivate-then-delete lifecycle;
  // Test employees are fictitious data and are exempt from that guardrail
  // (see Employee_Delete), so they can be deleted straight away too.
  readonly canDelete = computed(() => {
    const status = this.employee()?.status;
    return (
      status === EmployeeStatus.Deactivated || status === EmployeeStatus.Test
    );
  });

  constructor() {
    // (Re)load whenever the id in the URL changes; no id means nothing to show.
    effect(() => {
      const id = this.employeeId();
      untracked(() => {
        if (id) {
          this.auditLogService.loadAuditLog(id);
          this.salaryHistoryService.loadSalaryHistory(id);
        } else {
          this.router.navigate(['/employees']);
        }
      });
    });

    // A deactivate/reactivate changes the status and adds an audit entry, so
    // both are re-fetched once activationLoading() flips back to false.
    effect(() => {
      const loading = this.activationLoading();
      if (this.wasActivationLoading && !loading) {
        const employeeId = this.employee()?.employeeId;
        if (employeeId) {
          this.employeeResource.reload();
          this.auditLogService.loadAuditLog(employeeId);
        }
      }
      this.wasActivationLoading = loading;
    });
  }

  async deactivateEmployee(): Promise<void> {
    const employeeId = this.employee()?.employeeId;
    if (!employeeId) return;
    const confirmed = await this.confirmDialogService.confirm(
      'Are you sure you want to deactivate this employee?',
      { title: 'Deactivate employee?', confirmLabel: 'Deactivate' },
    );
    if (!confirmed) return;
    this.employeeService.deactivateEmployee(employeeId);
  }

  reactivateEmployee(): void {
    const employeeId = this.employee()?.employeeId;
    if (!employeeId) return;
    this.employeeService.reactivateEmployee(employeeId);
  }

  async createSalary(): Promise<void> {
    const employeeId = this.employee()?.employeeId;
    if (!employeeId) return;

    const grossSalary = Number(this.newSalaryAmount());
    if (
      !this.newSalaryAmount() ||
      Number.isNaN(grossSalary) ||
      grossSalary <= 0
    ) {
      this.createSalaryError.set('Enter a valid, positive gross salary.');
      return;
    }
    if (!this.newSalaryEffectiveDate()) {
      this.createSalaryError.set('Enter an effective date.');
      return;
    }

    this.addingSalary.set(true);
    this.createSalaryError.set(null);

    try {
      await firstValueFrom(
        this.salaryHistoryService.createSalary({
          employeeId: employeeId,
          grossSalary,
          effectiveDate: this.newSalaryEffectiveDate(),
        }),
      );
      this.newSalaryAmount.set('');
      this.newSalaryEffectiveDate.set('');
      // The new entry changes the current salary and adds an audit entry.
      this.employeeResource.reload();
      this.auditLogService.loadAuditLog(employeeId);
    } catch (error) {
      this.createSalaryError.set(
        extractErrorMessage(
          error as HttpErrorResponse,
          'Failed to add salary entry',
        ),
      );
    } finally {
      this.addingSalary.set(false);
    }
  }

  async deleteEmployee(): Promise<void> {
    const employeeId = this.employee()?.employeeId;
    if (!employeeId) return;
    const confirmed = await this.confirmDialogService.confirm(
      'Are you sure you want to permanently delete this employee? This cannot be undone.',
      { title: 'Delete employee?', confirmLabel: 'Delete', variant: 'danger' },
    );
    if (!confirmed) return;

    this.deleting.set(true);
    this.deleteError.set(null);

    this.employeeService.deleteEmployee(employeeId).subscribe({
      next: () => this.router.navigate(['/employees']),
      error: (error: HttpErrorResponse) => {
        this.deleting.set(false);
        this.deleteError.set(extractErrorMessage(error));
      },
    });
  }
}
