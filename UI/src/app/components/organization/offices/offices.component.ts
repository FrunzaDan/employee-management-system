import { Component, OnInit, inject, signal } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { RouterLink } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { OfficeService } from '../../../services/office.service';
import { ConfirmDialogService } from '../../../services/confirm-dialog.service';
import { Office } from '../../../interfaces/office-response';
import { EmployeeSummary } from '../../../interfaces/employee-summary-response';
import { extractErrorMessage } from '../../../utils/extract-error-message';
import { employeeStatusLabel } from '../../../utils/employee-status-label';

interface OfficeDraft {
  guid: string | null; // null = creating a new office, not editing an existing one
  officeName: string;
  city: string;
  country: string;
}

// A single page combining a list + one inline form reused for add and edit —
// unlike the employee add/edit split, offices are a 1-3 field lookup entity
// with no status lifecycle, so a separate pair of routes/pages would be excessive.
@Component({
  selector: 'app-offices',
  templateUrl: './offices.component.html',
  styleUrls: ['./offices.component.css'],
  imports: [RouterLink],
})
export class OfficesComponent implements OnInit {
  private readonly officeService = inject(OfficeService);
  private readonly confirmDialogService = inject(ConfirmDialogService);

  readonly offices = this.officeService.officesSignal;
  readonly loading = this.officeService.loadingSignal;
  readonly error = this.officeService.errorSignal;

  readonly draft = signal<OfficeDraft | null>(null);
  readonly saving = signal(false);
  readonly saveError = signal<string | null>(null);
  readonly deleteError = signal<string | null>(null);

  // Which office's employee list is currently expanded (at most one at a
  // time) — fetched on demand rather than eagerly per office, since most
  // rows are never expanded in a given visit.
  readonly expandedOfficeGuid = signal<string | null>(null);
  readonly expandedEmployees = signal<EmployeeSummary[]>([]);
  readonly expandedEmployeesLoading = signal(false);
  readonly expandedEmployeesError = signal<string | null>(null);

  readonly employeeStatusLabel = employeeStatusLabel;

  ngOnInit(): void {
    this.officeService.loadOffices();
  }

  startAdd(): void {
    this.saveError.set(null);
    this.draft.set({ guid: null, officeName: '', city: '', country: '' });
  }

  startEdit(office: Office): void {
    this.saveError.set(null);
    this.draft.set({
      guid: office.guid,
      officeName: office.officeName,
      city: office.city,
      country: office.country,
    });
  }

  cancel(): void {
    this.draft.set(null);
    this.saveError.set(null);
  }

  updateDraft(field: keyof Omit<OfficeDraft, 'guid'>, value: string): void {
    this.draft.update((d) => (d ? { ...d, [field]: value } : d));
  }

  async save(): Promise<void> {
    const draft = this.draft();
    if (!draft || !draft.officeName.trim()) {
      this.saveError.set('Office name is required.');
      return;
    }

    this.saving.set(true);
    this.saveError.set(null);

    try {
      const payload = { officeName: draft.officeName, city: draft.city, country: draft.country };
      await firstValueFrom(
        draft.guid
          ? this.officeService.editOffice({ guid: draft.guid, ...payload })
          : this.officeService.createOffice(payload),
      );
      this.draft.set(null);
    } catch (error) {
      this.saveError.set(extractErrorMessage(error as HttpErrorResponse, 'Failed to save office'));
    } finally {
      this.saving.set(false);
    }
  }

  async deleteOffice(office: Office): Promise<void> {
    const confirmed = await this.confirmDialogService.confirm(
      `Delete office "${office.officeName}"? This cannot be undone.`,
    );
    if (!confirmed) return;

    this.deleteError.set(null);
    try {
      await firstValueFrom(this.officeService.deleteOffice(office.guid));
    } catch (error) {
      // e.g. 409 when the office is still assigned to an employee.
      this.deleteError.set(extractErrorMessage(error as HttpErrorResponse, 'Failed to delete office'));
    }
  }

  toggleEmployees(office: Office): void {
    if (this.expandedOfficeGuid() === office.guid) {
      this.expandedOfficeGuid.set(null);
      return;
    }

    this.expandedOfficeGuid.set(office.guid);
    this.expandedEmployees.set([]);
    this.expandedEmployeesError.set(null);
    this.expandedEmployeesLoading.set(true);

    this.officeService.getEmployees(office.guid).subscribe({
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
