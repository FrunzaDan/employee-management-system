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
  private wasActivationLoading = false;

  readonly salaryHistory = this.salaryHistoryService.entries;
  readonly salaryHistoryLoading = this.salaryHistoryService.loading;
  readonly salaryHistoryError = this.salaryHistoryService.error;

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

  readonly canDelete = computed(() => {
    const status = this.employee()?.status;
    return (
      status === EmployeeStatus.Deactivated || status === EmployeeStatus.Test
    );
  });

  constructor() {
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
