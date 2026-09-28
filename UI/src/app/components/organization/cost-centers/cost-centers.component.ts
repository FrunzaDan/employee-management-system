import { Component, OnInit, inject, signal } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { RouterLink } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { CostCenterService } from '../../../services/cost-center.service';
import { ConfirmDialogService } from '../../../services/confirm-dialog.service';
import { CostCenter } from '../../../interfaces/cost-center';
import { extractErrorMessage } from '../../../utils/extract-error-message';
import { RonPipe } from '../../../pipes/ron.pipe';
import { EmployeeListComponent } from '../../employee-list/employee-list.component';

interface CostCenterDraft {
  costCenterId: string | null;
  code: string;
  name: string;
}

@Component({
  selector: 'app-cost-centers',
  templateUrl: './cost-centers.component.html',
  styleUrl: './cost-centers.component.css',
  imports: [RonPipe, RouterLink, EmployeeListComponent],
})
export class CostCentersComponent implements OnInit {
  private readonly costCenterService = inject(CostCenterService);
  private readonly confirmDialogService = inject(ConfirmDialogService);

  readonly costCenters = this.costCenterService.costCenters;
  readonly loading = this.costCenterService.loading;
  readonly loadError = this.costCenterService.error;

  readonly draft = signal<CostCenterDraft | null>(null);
  readonly saving = signal(false);
  readonly saveError = signal<string | null>(null);
  readonly deleteError = signal<string | null>(null);

  readonly expandedCostCenterId = signal<string | null>(null);

  ngOnInit(): void {
    this.costCenterService.loadCostCenters();
  }

  startAdd(): void {
    this.saveError.set(null);
    this.draft.set({ costCenterId: null, code: '', name: '' });
  }

  startEdit(costCenter: CostCenter): void {
    this.saveError.set(null);
    this.draft.set({
      costCenterId: costCenter.costCenterId,
      code: costCenter.code,
      name: costCenter.name ?? '',
    });
  }

  cancel(): void {
    this.draft.set(null);
    this.saveError.set(null);
  }

  updateDraft(
    field: keyof Omit<CostCenterDraft, 'costCenterId'>,
    value: string,
  ): void {
    this.draft.update((d) => (d ? { ...d, [field]: value } : d));
  }

  async save(): Promise<void> {
    const draft = this.draft();
    if (!draft || !draft.code.trim()) {
      this.saveError.set('Cost center code is required.');
      return;
    }

    this.saving.set(true);
    this.saveError.set(null);

    try {
      const payload = {
        code: draft.code,
        name: draft.name,
      };
      await firstValueFrom(
        draft.costCenterId
          ? this.costCenterService.updateCostCenter({
              costCenterId: draft.costCenterId,
              ...payload,
            })
          : this.costCenterService.createCostCenter(payload),
      );
      this.draft.set(null);
    } catch (error) {
      this.saveError.set(
        extractErrorMessage(
          error as HttpErrorResponse,
          'Failed to save cost center',
        ),
      );
    } finally {
      this.saving.set(false);
    }
  }

  async deleteCostCenter(costCenter: CostCenter): Promise<void> {
    const confirmed = await this.confirmDialogService.confirm(
      `Delete cost center "${costCenter.code}"? This cannot be undone.`,
      {
        title: 'Delete cost center?',
        confirmLabel: 'Delete',
        variant: 'danger',
      },
    );
    if (!confirmed) return;

    this.deleteError.set(null);
    try {
      await firstValueFrom(
        this.costCenterService.deleteCostCenter(costCenter.costCenterId),
      );
    } catch (error) {
      this.deleteError.set(
        extractErrorMessage(
          error as HttpErrorResponse,
          'Failed to delete cost center',
        ),
      );
    }
  }

  toggleEmployees(costCenter: CostCenter): void {
    this.expandedCostCenterId.update((id) =>
      id === costCenter.costCenterId ? null : costCenter.costCenterId,
    );
  }
}
