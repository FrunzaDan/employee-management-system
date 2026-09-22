import { Component, OnInit, inject, signal } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { RouterLink } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { DepartmentService } from '../../../services/department.service';
import { ConfirmDialogService } from '../../../services/confirm-dialog.service';
import { Department } from '../../../interfaces/department-response';
import { EmployeeSummary } from '../../../interfaces/employee-summary-response';
import { extractErrorMessage } from '../../../utils/extract-error-message';
import { employeeStatusLabel } from '../../../utils/employee-status-label';

interface DepartmentDraft {
  guid: string | null;
  departmentName: string;
}

@Component({
  selector: 'app-departments',
  templateUrl: './departments.component.html',
  styleUrls: ['./departments.component.css'],
  imports: [RouterLink],
})
export class DepartmentsComponent implements OnInit {
  private readonly departmentService = inject(DepartmentService);
  private readonly confirmDialogService = inject(ConfirmDialogService);

  readonly departments = this.departmentService.departmentsSignal;
  readonly loading = this.departmentService.loadingSignal;
  readonly error = this.departmentService.errorSignal;

  readonly draft = signal<DepartmentDraft | null>(null);
  readonly saving = signal(false);
  readonly saveError = signal<string | null>(null);
  readonly deleteError = signal<string | null>(null);

  readonly expandedDepartmentGuid = signal<string | null>(null);
  readonly expandedEmployees = signal<EmployeeSummary[]>([]);
  readonly expandedEmployeesLoading = signal(false);
  readonly expandedEmployeesError = signal<string | null>(null);

  readonly employeeStatusLabel = employeeStatusLabel;

  ngOnInit(): void {
    this.departmentService.loadDepartments();
  }

  startAdd(): void {
    this.saveError.set(null);
    this.draft.set({ guid: null, departmentName: '' });
  }

  startEdit(department: Department): void {
    this.saveError.set(null);
    this.draft.set({ guid: department.guid, departmentName: department.departmentName });
  }

  cancel(): void {
    this.draft.set(null);
    this.saveError.set(null);
  }

  updateDraft(value: string): void {
    this.draft.update((d) => (d ? { ...d, departmentName: value } : d));
  }

  async save(): Promise<void> {
    const draft = this.draft();
    if (!draft || !draft.departmentName.trim()) {
      this.saveError.set('Department name is required.');
      return;
    }

    this.saving.set(true);
    this.saveError.set(null);

    try {
      await firstValueFrom(
        draft.guid
          ? this.departmentService.editDepartment({
              guid: draft.guid,
              departmentName: draft.departmentName,
            })
          : this.departmentService.createDepartment({ departmentName: draft.departmentName }),
      );
      this.draft.set(null);
    } catch (error) {
      this.saveError.set(
        extractErrorMessage(error as HttpErrorResponse, 'Failed to save department'),
      );
    } finally {
      this.saving.set(false);
    }
  }

  async deleteDepartment(department: Department): Promise<void> {
    const confirmed = await this.confirmDialogService.confirm(
      `Delete department "${department.departmentName}"? This cannot be undone.`,
    );
    if (!confirmed) return;

    this.deleteError.set(null);
    try {
      await firstValueFrom(this.departmentService.deleteDepartment(department.guid));
    } catch (error) {
      this.deleteError.set(
        extractErrorMessage(error as HttpErrorResponse, 'Failed to delete department'),
      );
    }
  }

  toggleEmployees(department: Department): void {
    if (this.expandedDepartmentGuid() === department.guid) {
      this.expandedDepartmentGuid.set(null);
      return;
    }

    this.expandedDepartmentGuid.set(department.guid);
    this.expandedEmployees.set([]);
    this.expandedEmployeesError.set(null);
    this.expandedEmployeesLoading.set(true);

    this.departmentService.getEmployees(department.guid).subscribe({
      next: (employees) => {
        this.expandedEmployees.set(employees);
        this.expandedEmployeesLoading.set(false);
      },
      error: (error: HttpErrorResponse) => {
        this.expandedEmployeesError.set(
          extractErrorMessage(error, 'Failed to load employees'),
        );
        this.expandedEmployeesLoading.set(false);
      },
    });
  }
}
