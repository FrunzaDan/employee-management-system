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
import { OfficeService } from '../../../services/office.service';
import { ConfirmDialogService } from '../../../services/confirm-dialog.service';
import { Office } from '../../../interfaces/office';
import { extractErrorMessage } from '../../../utils/extract-error-message';
import { toServerErrors } from '../../../utils/server-errors';
import { RonPipe } from '../../../pipes/ron.pipe';
import { EmployeeListComponent } from '../../employee-list/employee-list.component';

interface OfficeDraft {
  officeId: string | null;
  name: string;
  city: string;
  country: string;
}

const emptyOfficeDraft = (): OfficeDraft => ({
  officeId: null,
  name: '',
  city: '',
  country: '',
});

const NOT_BLANK = /\S/;

@Component({
  selector: 'app-offices',
  templateUrl: './offices.component.html',
  styleUrl: './offices.component.css',
  imports: [EmployeeListComponent, FormField, FormRoot, RonPipe, RouterLink],
})
export class OfficesComponent implements OnInit {
  private readonly officeService = inject(OfficeService);
  private readonly confirmDialogService = inject(ConfirmDialogService);

  readonly offices = this.officeService.offices;
  readonly loading = this.officeService.loading;
  readonly loadError = this.officeService.error;

  readonly editorOpen = signal(false);
  private readonly draft = signal<OfficeDraft>(emptyOfficeDraft());
  readonly isEdit = computed(() => this.draft().officeId !== null);
  readonly saveError = signal<string | null>(null);
  readonly deleteError = signal<string | null>(null);

  readonly officeForm = form(
    this.draft,
    (p) => {
      required(p.name, { message: 'Office name is required.' });
      pattern(p.name, NOT_BLANK, { message: 'Office name is required.' });
    },
    {
      submission: {
        action: () => this.save(),
        onInvalid: (field) =>
          field().errorSummary()[0]?.fieldTree().focusBoundControl(),
      },
    },
  );

  readonly expandedOfficeId = signal<string | null>(null);

  ngOnInit(): void {
    this.officeService.loadOffices();
  }

  startAdd(): void {
    this.openEditor(emptyOfficeDraft());
  }

  startEdit(office: Office): void {
    this.openEditor({
      officeId: office.officeId,
      name: office.name,
      city: office.city ?? '',
      country: office.country ?? '',
    });
  }

  cancel(): void {
    this.editorOpen.set(false);
    this.saveError.set(null);
  }

  private openEditor(draft: OfficeDraft): void {
    this.saveError.set(null);
    this.officeForm().reset(draft);
    this.editorOpen.set(true);
  }

  private async save(): Promise<TreeValidationResult> {
    const draft = this.draft();
    this.saveError.set(null);

    try {
      const payload = {
        name: draft.name,
        city: draft.city,
        country: draft.country,
      };
      await firstValueFrom(
        draft.officeId
          ? this.officeService.updateOffice({
              officeId: draft.officeId,
              ...payload,
            })
          : this.officeService.createOffice(payload),
      );
      this.editorOpen.set(false);
    } catch (error) {
      const { fieldErrors, message } = toServerErrors(
        error as HttpErrorResponse,
        this.officeForm,
        'Failed to save office',
      );
      this.saveError.set(message);
      fieldErrors[0]?.fieldTree().focusBoundControl();
      return fieldErrors;
    }
  }

  async deleteOffice(office: Office): Promise<void> {
    const confirmed = await this.confirmDialogService.confirm(
      `Delete office "${office.name}"? This cannot be undone.`,
      { title: 'Delete office?', confirmLabel: 'Delete', variant: 'danger' },
    );
    if (!confirmed) return;

    this.deleteError.set(null);
    try {
      await firstValueFrom(this.officeService.deleteOffice(office.officeId));
    } catch (error) {
      this.deleteError.set(
        extractErrorMessage(
          error as HttpErrorResponse,
          'Failed to delete office',
        ),
      );
    }
  }

  toggleEmployees(office: Office): void {
    this.expandedOfficeId.update((id) =>
      id === office.officeId ? null : office.officeId,
    );
  }
}
