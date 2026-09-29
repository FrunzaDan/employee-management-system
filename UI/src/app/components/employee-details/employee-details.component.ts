import {
  Component,
  computed,
  inject,
  input,
  linkedSignal,
  signal,
} from '@angular/core';
import { DatePipe } from '@angular/common';
import { rxResource } from '@angular/core/rxjs-interop';
import {
  FormField,
  FormRoot,
  form,
  min,
  required,
} from '@angular/forms/signals';
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

const AUDIT_LOG_PREVIEW_SIZE = 10;

interface SalaryFormModel {
  grossSalary: number | null;
  effectiveDate: string;
}

const emptySalaryForm = (): SalaryFormModel => ({
  grossSalary: null,
  effectiveDate: '',
});

const GENDER_LABELS = new Map<Gender, string>([
  [Gender.NotDeclared, 'not declared'],
  [Gender.Male, 'male'],
  [Gender.Female, 'female'],
]);

@Component({
  selector: 'app-employee-details',
  templateUrl: './employee-details.component.html',
  styleUrl: './employee-details.component.css',
  imports: [DatePipe, FormField, FormRoot, RonPipe, RouterLink],
})
export class EmployeeDetailsComponent {
  private readonly employeeService = inject(EmployeeService);
  private readonly confirmDialogService = inject(ConfirmDialogService);
  private readonly auditLogService = inject(AuditLogService);
  private readonly salaryHistoryService = inject(SalaryHistoryService);
  private readonly router = inject(Router);

  readonly employeeId = input<string>();

  private readonly employeeResource = rxResource({
    params: () => this.employeeId(),
    stream: ({ params: employeeId }) =>
      this.employeeService.getEmployee(employeeId),
  });
  readonly employee = computed(() =>
    this.employeeResource.hasValue() ? this.employeeResource.value() : null,
  );
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

  readonly activationLoading = this.employeeService.activationLoading;
  readonly activationError = this.employeeService.activationError;
  readonly deleting = signal(false);
  readonly deleteError = signal<string | null>(null);

  readonly auditLog = this.auditLogService.entries;
  readonly auditLogLoading = this.auditLogService.loading;
  readonly auditLogError = this.auditLogService.error;
  // Collapsed again whenever another employee is shown.
  readonly showAllAuditLog = linkedSignal({
    source: this.employeeId,
    computation: () => false,
  });
  readonly visibleAuditLog = computed(() =>
    this.showAllAuditLog()
      ? this.auditLog()
      : this.auditLog().slice(0, AUDIT_LOG_PREVIEW_SIZE),
  );
  readonly hiddenAuditLogCount = computed(
    () => this.auditLog().length - this.visibleAuditLog().length,
  );

  readonly salaryHistory = this.salaryHistoryService.entries;
  readonly salaryHistoryLoading = this.salaryHistoryService.loading;
  readonly salaryHistoryError = this.salaryHistoryService.error;

  private readonly salaryModel = signal<SalaryFormModel>(emptySalaryForm());
  readonly createSalaryError = signal<string | null>(null);
  readonly salaryForm = form(
    this.salaryModel,
    (p) => {
      required(p.grossSalary, { message: 'Gross salary is required.' });
      min(p.grossSalary, 0.01, {
        message: 'Gross salary must be greater than zero.',
      });
      required(p.effectiveDate, { message: 'Effective date is required.' });
    },
    {
      submission: {
        action: () => this.createSalary(),
        onInvalid: (field) =>
          field().errorSummary()[0]?.fieldTree().focusBoundControl(),
      },
    },
  );

  readonly genderLabel = computed(() => {
    const employee = this.employee();
    return employee ? GENDER_LABELS.get(employee.gender) : undefined;
  });

  readonly statusLabel = computed(() => {
    const employee = this.employee();
    return employee ? employeeStatusLabel(employee.status) : undefined;
  });

  readonly canDelete = computed(() => {
    const status = this.employee()?.status;
    return (
      status === EmployeeStatus.Deactivated || status === EmployeeStatus.Test
    );
  });

  constructor() {
    this.auditLogService.bindAuditLog(this.employeeId);
    this.salaryHistoryService.bindSalaryHistory(this.employeeId);
  }

  async deactivateEmployee(): Promise<void> {
    const employeeId = this.employee()?.employeeId;
    if (!employeeId) return;
    const confirmed = await this.confirmDialogService.confirm(
      'Are you sure you want to deactivate this employee?',
      { title: 'Deactivate employee?', confirmLabel: 'Deactivate' },
    );
    if (!confirmed) return;
    if (await this.employeeService.deactivateEmployee(employeeId))
      this.refreshAfterStatusChange();
  }

  async reactivateEmployee(): Promise<void> {
    const employeeId = this.employee()?.employeeId;
    if (!employeeId) return;
    if (await this.employeeService.reactivateEmployee(employeeId))
      this.refreshAfterStatusChange();
  }

  private refreshAfterStatusChange(): void {
    this.employeeResource.reload();
    this.auditLogService.reloadAuditLog();
  }

  private async createSalary(): Promise<void> {
    const employeeId = this.employee()?.employeeId;
    if (!employeeId) return;

    this.createSalaryError.set(null);
    const { grossSalary, effectiveDate } = this.salaryModel();

    try {
      await firstValueFrom(
        this.salaryHistoryService.createSalary({
          employeeId,
          grossSalary: grossSalary!,
          effectiveDate,
        }),
      );
      this.salaryForm().reset(emptySalaryForm());
      this.employeeResource.reload();
      this.auditLogService.reloadAuditLog();
    } catch (error) {
      this.createSalaryError.set(
        extractErrorMessage(
          error as HttpErrorResponse,
          'Failed to add salary entry',
        ),
      );
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
