import { Component, OnInit, computed, inject, signal } from '@angular/core';
import {
  FormField,
  FormRoot,
  form,
  pattern,
  required,
  TreeValidationResult,
} from '@angular/forms/signals';
import { HttpErrorResponse } from '@angular/common/http';
import { RouterLink } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { DepartmentService } from '../../../services/department.service';
import { ConfirmDialogService } from '../../../services/confirm-dialog.service';
import { Department } from '../../../interfaces/department';
import { extractErrorMessage } from '../../../utils/extract-error-message';
import { toServerErrors } from '../../../utils/server-errors';
import { RonPipe } from '../../../pipes/ron.pipe';
import { EmployeeListComponent } from '../../employee-list/employee-list.component';

interface DepartmentDraft {
  departmentId: string | null;
  name: string;
}

const emptyDepartmentDraft = (): DepartmentDraft => ({
  departmentId: null,
  name: '',
});

const NOT_BLANK = /\S/;

@Component({
  selector: 'app-departments',
  templateUrl: './departments.component.html',
  styleUrl: './departments.component.css',
  imports: [EmployeeListComponent, FormField, FormRoot, RonPipe, RouterLink],
})
export class DepartmentsComponent implements OnInit {
  private readonly departmentService = inject(DepartmentService);
  private readonly confirmDialogService = inject(ConfirmDialogService);

  readonly departments = this.departmentService.departments;
  readonly loading = this.departmentService.loading;
  readonly loadError = this.departmentService.error;

  readonly editorOpen = signal(false);
  private readonly draft = signal<DepartmentDraft>(emptyDepartmentDraft());
  readonly isEdit = computed(() => this.draft().departmentId !== null);
  readonly saveError = signal<string | null>(null);
  readonly deleteError = signal<string | null>(null);

  readonly departmentForm = form(
    this.draft,
    (p) => {
      required(p.name, { message: 'Department name is required.' });
      pattern(p.name, NOT_BLANK, { message: 'Department name is required.' });
    },
    {
      submission: {
        action: () => this.save(),
        onInvalid: (field) =>
          field().errorSummary()[0]?.fieldTree().focusBoundControl(),
      },
    },
  );

  readonly expandedDepartmentId = signal<string | null>(null);

  ngOnInit(): void {
    this.departmentService.loadDepartments();
  }

  startAdd(): void {
    this.openEditor(emptyDepartmentDraft());
  }

  startEdit(department: Department): void {
    this.openEditor({
      departmentId: department.departmentId,
      name: department.name,
    });
  }

  cancel(): void {
    this.editorOpen.set(false);
    this.saveError.set(null);
  }

  private openEditor(draft: DepartmentDraft): void {
    this.saveError.set(null);
    this.departmentForm().reset(draft);
    this.editorOpen.set(true);
  }

  private async save(): Promise<TreeValidationResult> {
    const draft = this.draft();
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
      this.editorOpen.set(false);
    } catch (error) {
      const { fieldErrors, message } = toServerErrors(
        error as HttpErrorResponse,
        this.departmentForm,
        'Failed to save department',
      );
      this.saveError.set(message);
      fieldErrors[0]?.fieldTree().focusBoundControl();
      return fieldErrors;
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
    this.expandedDepartmentId.update((id) =>
      id === department.departmentId ? null : department.departmentId,
    );
  }
}
