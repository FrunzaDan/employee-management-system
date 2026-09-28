import { Component, OnInit, inject, signal } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { RouterLink } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { OfficeService } from '../../../services/office.service';
import { ConfirmDialogService } from '../../../services/confirm-dialog.service';
import { Office } from '../../../interfaces/office';
import { extractErrorMessage } from '../../../utils/extract-error-message';
import { RonPipe } from '../../../pipes/ron.pipe';
import { EmployeeListComponent } from '../../employee-list/employee-list.component';

interface OfficeDraft {
  officeId: string | null;
  name: string;
  city: string;
  country: string;
}

@Component({
  selector: 'app-offices',
  templateUrl: './offices.component.html',
  styleUrl: './offices.component.css',
  imports: [RonPipe, RouterLink, EmployeeListComponent],
})
export class OfficesComponent implements OnInit {
  private readonly officeService = inject(OfficeService);
  private readonly confirmDialogService = inject(ConfirmDialogService);

  readonly offices = this.officeService.offices;
  readonly loading = this.officeService.loading;
  readonly loadError = this.officeService.error;

  readonly draft = signal<OfficeDraft | null>(null);
  readonly saving = signal(false);
  readonly saveError = signal<string | null>(null);
  readonly deleteError = signal<string | null>(null);

  readonly expandedOfficeId = signal<string | null>(null);

  ngOnInit(): void {
    this.officeService.loadOffices();
  }

  startAdd(): void {
    this.saveError.set(null);
    this.draft.set({ officeId: null, name: '', city: '', country: '' });
  }

  startEdit(office: Office): void {
    this.saveError.set(null);
    this.draft.set({
      officeId: office.officeId,
      name: office.name,
      city: office.city ?? '',
      country: office.country ?? '',
    });
  }

  cancel(): void {
    this.draft.set(null);
    this.saveError.set(null);
  }

  updateDraft(field: keyof Omit<OfficeDraft, 'officeId'>, value: string): void {
    this.draft.update((d) => (d ? { ...d, [field]: value } : d));
  }

  async save(): Promise<void> {
    const draft = this.draft();
    if (!draft || !draft.name.trim()) {
      this.saveError.set('Office name is required.');
      return;
    }

    this.saving.set(true);
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
      this.draft.set(null);
    } catch (error) {
      this.saveError.set(
        extractErrorMessage(
          error as HttpErrorResponse,
          'Failed to save office',
        ),
      );
    } finally {
      this.saving.set(false);
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
