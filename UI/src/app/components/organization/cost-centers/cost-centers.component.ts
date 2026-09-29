import { Component, OnInit, computed, inject, signal } from '@angular/core';
import {
  FormField,
  FormRoot,
  form,
  pattern,
  required,
} from '@angular/forms/signals';
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

const emptyCostCenterDraft = (): CostCenterDraft => ({
  costCenterId: null,
  code: '',
  name: '',
});

const NOT_BLANK = /\S/;

@Component({
  selector: 'app-cost-centers',
  templateUrl: './cost-centers.component.html',
  styleUrl: './cost-centers.component.css',
  imports: [EmployeeListComponent, FormField, FormRoot, RonPipe, RouterLink],
})
export class CostCentersComponent implements OnInit {
  private readonly costCenterService = inject(CostCenterService);
  private readonly confirmDialogService = inject(ConfirmDialogService);

  readonly costCenters = this.costCenterService.costCenters;
  readonly loading = this.costCenterService.loading;
  readonly loadError = this.costCenterService.error;

  readonly editorOpen = signal(false);
  private readonly draft = signal<CostCenterDraft>(emptyCostCenterDraft());
  readonly isEdit = computed(() => this.draft().costCenterId !== null);
  readonly saveError = signal<string | null>(null);
  readonly deleteError = signal<string | null>(null);

  readonly costCenterForm = form(
    this.draft,
    (p) => {
      required(p.code, { message: 'Cost center code is required.' });
      pattern(p.code, NOT_BLANK, { message: 'Cost center code is required.' });
    },
    {
      submission: {
        action: () => this.save(),
        onInvalid: (field) =>
          field().errorSummary()[0]?.fieldTree().focusBoundControl(),
      },
    },
  );

  readonly expandedCostCenterId = signal<string | null>(null);

  ngOnInit(): void {
    this.costCenterService.loadCostCenters();
  }

  startAdd(): void {
    this.openEditor(emptyCostCenterDraft());
  }

  startEdit(costCenter: CostCenter): void {
    this.openEditor({
      costCenterId: costCenter.costCenterId,
      code: costCenter.code,
      name: costCenter.name ?? '',
    });
  }

  cancel(): void {
    this.editorOpen.set(false);
    this.saveError.set(null);
  }

  private openEditor(draft: CostCenterDraft): void {
    this.saveError.set(null);
    this.costCenterForm().reset(draft);
    this.editorOpen.set(true);
  }

  private async save(): Promise<void> {
    const draft = this.draft();
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
      this.editorOpen.set(false);
    } catch (error) {
      this.saveError.set(
        extractErrorMessage(
          error as HttpErrorResponse,
          'Failed to save cost center',
        ),
      );
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
