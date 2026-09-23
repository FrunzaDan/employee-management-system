import { Component, OnInit, inject, signal } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { RouterLink } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { DepartmentService } from '../../../services/department.service';
import { ConfirmDialogService } from '../../../services/confirm-dialog.service';
import { Department } from '../../../interfaces/department';
import { EmployeeSummary } from '../../../interfaces/employee-summary';
import { extractErrorMessage } from '../../../utils/extract-error-message';
import { employeeStatusLabel } from '../../../utils/employee-status-label';
import { RonPipe } from '../../../pipes/ron.pipe';

interface DepartmentDraft {
  departmentId: string | null;
  name: string;
}

@Component({
  selector: 'app-departments',
  templateUrl: './departments.component.html',
  styleUrl: './departments.component.css',
  imports: [RonPipe, RouterLink],
})
export class DepartmentsComponent implements OnInit {
  private readonly departmentService = inject(DepartmentService);
  private readonly confirmDialogService = inject(ConfirmDialogService);

  readonly departments = this.departmentService.departments;
  readonly loading = this.departmentService.loading;
  readonly error = this.departmentService.error;

  readonly draft = signal<DepartmentDraft | null>(null);
  readonly saving = signal(false);
  readonly saveError = signal<string | null>(null);
  readonly deleteError = signal<string | null>(null);

  readonly expandedDepartmentId = signal<string | null>(null);
  readonly expandedEmployees = signal<EmployeeSummary[]>([]);
  readonly expandedEmployeesLoading = signal(false);
  readonly expandedEmployeesError = signal<string | null>(null);

  readonly employeeStatusLabel = employeeStatusLabel;

  ngOnInit(): void {
    this.departmentService.loadDepartments();
  }

  startAdd(): void {
    this.saveError.set(null);
    this.draft.set({ departmentId: null, name: '' });
  }

  startEdit(department: Department): void {
    this.saveError.set(null);
    this.draft.set({
      departmentId: department.departmentId,
      name: department.name,
    });
  }

  cancel(): void {
    this.draft.set(null);
    this.saveError.set(null);
  }

  updateDraft(value: string): void {
    this.draft.update((d) => (d ? { ...d, name: value } : d));
  }

  async save(): Promise<void> {
    const draft = this.draft();
    if (!draft || !draft.name.trim()) {
      this.saveError.set('Department name is required.');
      return;
    }

    this.saving.set(true);
    this.saveError.set(null);

    try {
      await firstValueFrom(
        draft.departmentId
          ? this.departmentService.updateDepartment({
              departmentId: draft.departmentId,
              name: draft.name,
            })
          : this.departmentService.createDepartment({ name: draft.name }),
      );
      this.draft.set(null);
    } catch (error) {
      this.saveError.set(
        extractErrorMessage(
          error as HttpErrorResponse,
          'Failed to save department',
        ),
      );
    } finally {
      this.saving.set(false);
    }
  }

  async deleteDepartment(department: Department): Promise<void> {
    const confirmed = await this.confirmDialogService.confirm(
      `Delete department "${department.name}"? This cannot be undone.`,
      {
        title: 'Delete department?',
        confirmLabel: 'Delete',
        variant: 'danger',
      },
    );
    if (!confirmed) return;

    this.deleteError.set(null);
    try {
      await firstValueFrom(
        this.departmentService.deleteDepartment(department.departmentId),
      );
    } catch (error) {
      this.deleteError.set(
        extractErrorMessage(
          error as HttpErrorResponse,
          'Failed to delete department',
        ),
      );
    }
  }

  toggleEmployees(department: Department): void {
    if (this.expandedDepartmentId() === department.departmentId) {
      this.expandedDepartmentId.set(null);
      return;
    }

    this.expandedDepartmentId.set(department.departmentId);
    this.expandedEmployees.set([]);
    this.expandedEmployeesError.set(null);
    this.expandedEmployeesLoading.set(true);

    this.departmentService.getEmployees(department.departmentId).subscribe({
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
