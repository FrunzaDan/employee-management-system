import { Component, OnInit, inject, signal } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { RouterLink } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { CostCenterService } from '../../../services/cost-center.service';
import { ConfirmDialogService } from '../../../services/confirm-dialog.service';
import { CostCenter } from '../../../interfaces/cost-center-response';
import { EmployeeSummary } from '../../../interfaces/employee-summary-response';
import { extractErrorMessage } from '../../../utils/extract-error-message';
import { employeeStatusLabel } from '../../../utils/employee-status-label';

interface CostCenterDraft {
  guid: string | null;
  costCenterCode: string;
  costCenterName: string;
}

@Component({
  selector: 'app-cost-centers',
  templateUrl: './cost-centers.component.html',
  styleUrls: ['./cost-centers.component.css'],
  imports: [RouterLink],
})
export class CostCentersComponent implements OnInit {
  private readonly costCenterService = inject(CostCenterService);
  private readonly confirmDialogService = inject(ConfirmDialogService);

  readonly costCenters = this.costCenterService.costCentersSignal;
  readonly loading = this.costCenterService.loadingSignal;
  readonly error = this.costCenterService.errorSignal;

  readonly draft = signal<CostCenterDraft | null>(null);
  readonly saving = signal(false);
  readonly saveError = signal<string | null>(null);
  readonly deleteError = signal<string | null>(null);

  readonly expandedCostCenterGuid = signal<string | null>(null);
  readonly expandedEmployees = signal<EmployeeSummary[]>([]);
  readonly expandedEmployeesLoading = signal(false);
  readonly expandedEmployeesError = signal<string | null>(null);

  readonly employeeStatusLabel = employeeStatusLabel;

  ngOnInit(): void {
    this.costCenterService.loadCostCenters();
  }

  startAdd(): void {
    this.saveError.set(null);
    this.draft.set({ guid: null, costCenterCode: '', costCenterName: '' });
  }

  startEdit(costCenter: CostCenter): void {
    this.saveError.set(null);
    this.draft.set({
      guid: costCenter.guid,
      costCenterCode: costCenter.costCenterCode,
      costCenterName: costCenter.costCenterName ?? '',
    });
  }

  cancel(): void {
    this.draft.set(null);
    this.saveError.set(null);
  }

  updateDraft(field: keyof Omit<CostCenterDraft, 'guid'>, value: string): void {
    this.draft.update((d) => (d ? { ...d, [field]: value } : d));
  }

  async save(): Promise<void> {
    const draft = this.draft();
    if (!draft || !draft.costCenterCode.trim()) {
      this.saveError.set('Cost center code is required.');
      return;
    }

    this.saving.set(true);
    this.saveError.set(null);

    try {
      const payload = {
        costCenterCode: draft.costCenterCode,
        costCenterName: draft.costCenterName,
      };
      await firstValueFrom(
        draft.guid
          ? this.costCenterService.editCostCenter({ guid: draft.guid, ...payload })
          : this.costCenterService.createCostCenter(payload),
      );
      this.draft.set(null);
    } catch (error) {
      this.saveError.set(
        extractErrorMessage(error as HttpErrorResponse, 'Failed to save cost center'),
      );
    } finally {
      this.saving.set(false);
    }
  }

  async deleteCostCenter(costCenter: CostCenter): Promise<void> {
    const confirmed = await this.confirmDialogService.confirm(
      `Delete cost center "${costCenter.costCenterCode}"? This cannot be undone.`,
    );
    if (!confirmed) return;

    this.deleteError.set(null);
    try {
      await firstValueFrom(this.costCenterService.deleteCostCenter(costCenter.guid));
    } catch (error) {
      this.deleteError.set(
        extractErrorMessage(error as HttpErrorResponse, 'Failed to delete cost center'),
      );
    }
  }

  toggleEmployees(costCenter: CostCenter): void {
    if (this.expandedCostCenterGuid() === costCenter.guid) {
      this.expandedCostCenterGuid.set(null);
      return;
    }

    this.expandedCostCenterGuid.set(costCenter.guid);
    this.expandedEmployees.set([]);
    this.expandedEmployeesError.set(null);
    this.expandedEmployeesLoading.set(true);

    this.costCenterService.getEmployees(costCenter.guid).subscribe({
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
