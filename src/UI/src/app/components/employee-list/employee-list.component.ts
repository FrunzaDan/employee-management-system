import {
  Component,
  computed,
  inject,
  input,
  linkedSignal,
  signal,
} from '@angular/core';
import { FormField, debounce, form } from '@angular/forms/signals';
import { RouterLink } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { catchError, concatMap, from, map, of, toArray } from 'rxjs';
import { EmployeeService } from '../../services/employee.service';
import { ConfirmDialogService } from '../../services/confirm-dialog.service';
import { NotificationService } from '../../services/notification.service';
import { EmployeeStatus } from '../../interfaces/employee';
import { extractErrorMessage } from '../../utils/extract-error-message';
import { employeeStatusLabel } from '../../utils/employee-status-label';

type EmployeeSortColumn = 'name' | 'email' | 'phoneNumber';

const SEARCH_DEBOUNCE_MS = 300;

const SORT_LABELS: Record<EmployeeSortColumn, string> = {
  name: 'name',
  email: 'email',
  phoneNumber: 'phone number',
};

@Component({
  selector: 'app-employee-list',
  templateUrl: './employee-list.component.html',
  styleUrl: './employee-list.component.css',
  imports: [FormField, RouterLink],
  // Each list keeps its own page, so a filtered list on an org page never
  // shares state with the main employee list.
  providers: [EmployeeService],
})
export class EmployeeListComponent {
  readonly officeId = input<string>();
  readonly departmentId = input<string>();
  readonly costCenterId = input<string>();
  readonly emptyMessage = input('No employees yet.');

  private readonly employeeService = inject(EmployeeService);
  private readonly confirmDialogService = inject(ConfirmDialogService);
  private readonly notificationService = inject(NotificationService);

  readonly employees = this.employeeService.employees;
  readonly loading = this.employeeService.loading;
  readonly loadError = this.employeeService.error;
  readonly activationLoading = this.employeeService.activationLoading;
  readonly activationError = this.employeeService.activationError;

  readonly deleting = signal(false);
  readonly deleteError = signal<string | null>(null);

  readonly bulkActionInProgress = signal(false);

  readonly allSelected = computed(
    () =>
      this.employees().length > 0 &&
      this.employees().every((c) =>
        this.selectedEmployeeIds().has(c.employeeId),
      ),
  );

  readonly exportLoading = this.employeeService.exportLoading;
  readonly exportError = this.employeeService.exportError;

  readonly EmployeeStatus = EmployeeStatus;

  readonly employeeStatusLabel = employeeStatusLabel;

  readonly searchForm = form(signal({ term: '' }), (p) => {
    debounce(p.term, SEARCH_DEBOUNCE_MS);
  });
  readonly searchTerm = computed(() => this.searchForm.term().value().trim());
  readonly sortColumn = signal<EmployeeSortColumn>('name');
  readonly sortDirection = signal<'asc' | 'desc'>('asc');

  readonly pageSize = 50;
  // Back to page 1 whenever the search or the sort changes.
  readonly currentPage = linkedSignal({
    source: () => [this.searchTerm(), this.sortColumn(), this.sortDirection()],
    computation: () => 1,
  });

  readonly listParams = computed(() => ({
    pageNumber: this.currentPage(),
    pageSize: this.pageSize,
    searchTerm: this.searchTerm() || undefined,
    sortColumn: this.sortColumn(),
    sortDirection: this.sortDirection(),
    ...this.orgFilter(),
  }));

  // A new page, search or sort starts with nothing selected.
  readonly selectedEmployeeIds = linkedSignal<unknown, ReadonlySet<string>>({
    source: this.listParams,
    computation: () => new Set(),
  });

  readonly totalItems = this.employeeService.totalItems;
  readonly totalPages = computed(() =>
    Math.max(1, Math.ceil(this.totalItems() / this.pageSize)),
  );

  readonly resultsAnnouncement = computed(() => {
    if (this.loading()) return 'Loading employees';
    const total = this.totalItems();
    return `${total} ${total === 1 ? 'employee' : 'employees'} found`;
  });

  readonly tableCaption = computed(
    () =>
      `Employees, page ${this.currentPage()} of ${this.totalPages()}, sorted by ${SORT_LABELS[this.sortColumn()]} ${this.sortDirection() === 'asc' ? 'ascending' : 'descending'}`,
  );

  constructor() {
    this.employeeService.bindEmployees(this.listParams);
  }

  goToPage(page: number): void {
    const target = Math.min(Math.max(page, 1), this.totalPages());
    if (target === this.currentPage()) return;
    this.currentPage.set(target);
  }

  ariaSort(column: EmployeeSortColumn): 'ascending' | 'descending' | 'none' {
    if (this.sortColumn() !== column) return 'none';
    return this.sortDirection() === 'asc' ? 'ascending' : 'descending';
  }

  setSort(column: EmployeeSortColumn): void {
    if (this.sortColumn() === column) {
      this.sortDirection.set(this.sortDirection() === 'asc' ? 'desc' : 'asc');
    } else {
      this.sortColumn.set(column);
      this.sortDirection.set('asc');
    }
  }

  exportCsv(): void {
    this.employeeService.exportEmployees({
      searchTerm: this.searchTerm() || undefined,
      sortColumn: this.sortColumn(),
      sortDirection: this.sortDirection(),
      ...this.orgFilter(),
    });
  }

  private orgFilter() {
    return {
      officeId: this.officeId(),
      departmentId: this.departmentId(),
      costCenterId: this.costCenterId(),
    };
  }

  // After rows are removed, step back a page if this one is now empty;
  // otherwise reload it so it fills up again from the next page.
  private refreshAfterRemoval(): void {
    if (this.employees().length === 0 && this.currentPage() > 1) {
      this.currentPage.update((page) => page - 1);
    } else {
      this.selectedEmployeeIds.set(new Set());
      this.employeeService.reloadEmployees();
    }
  }

  async deactivateEmployee(employeeId: string): Promise<void> {
    const confirmed = await this.confirmDialogService.confirm(
      'Are you sure you want to deactivate this employee?',
      { title: 'Deactivate employee?', confirmLabel: 'Deactivate' },
    );
    if (!confirmed) return;
    await this.employeeService.deactivateEmployee(employeeId);
  }

  async reactivateEmployee(employeeId: string): Promise<void> {
    await this.employeeService.reactivateEmployee(employeeId);
  }

  async deleteEmployee(employeeId: string): Promise<void> {
    const confirmed = await this.confirmDialogService.confirm(
      'Are you sure you want to permanently delete this employee? This cannot be undone.',
      { title: 'Delete employee?', confirmLabel: 'Delete', variant: 'danger' },
    );
    if (!confirmed) return;

    this.deleting.set(true);
    this.deleteError.set(null);

    this.employeeService.deleteEmployee(employeeId).subscribe({
      next: () => {
        this.deleting.set(false);
        this.refreshAfterRemoval();
      },
      error: (error: HttpErrorResponse) => {
        this.deleting.set(false);
        this.deleteError.set(extractErrorMessage(error));
      },
    });
  }

  isSelected(employeeId: string): boolean {
    return this.selectedEmployeeIds().has(employeeId);
  }

  toggleSelection(employeeId: string, checked: boolean): void {
    const next = new Set(this.selectedEmployeeIds());
    if (checked) {
      next.add(employeeId);
    } else {
      next.delete(employeeId);
    }
    this.selectedEmployeeIds.set(next);
  }

  toggleSelectAll(checked: boolean): void {
    const next = new Set(this.selectedEmployeeIds());
    for (const employee of this.employees()) {
      if (checked) {
        next.add(employee.employeeId);
      } else {
        next.delete(employee.employeeId);
      }
    }
    this.selectedEmployeeIds.set(next);
  }

  async bulkDeleteSelected(): Promise<void> {
    const employeeIds = this.selectedEmployeeIds();
    const selected = this.employees().filter((c) =>
      employeeIds.has(c.employeeId),
    );
    if (selected.length === 0) return;

    const toDeactivate = selected.filter(
      (c) => c.status === EmployeeStatus.Active,
    );
    const toDelete = selected.filter((c) => c.status !== EmployeeStatus.Active);

    const lines = [`Of the ${selected.length} selected employees:`];
    if (toDeactivate.length > 0) {
      lines.push(
        `- ${toDeactivate.length} ${toDeactivate.length === 1 ? 'is' : 'are'} active and will only be deactivated (a employee must be deactivated before it can be deleted).`,
      );
    }
    if (toDelete.length > 0) {
      lines.push(
        `- ${toDelete.length} ${toDelete.length === 1 ? 'is' : 'are'} already deactivated or test employees and will be permanently deleted.`,
      );
    }
    lines.push('Continue?');

    const confirmed = await this.confirmDialogService.confirm(
      lines.join('\n'),
      {
        title: 'Apply bulk action?',
        confirmLabel: 'Apply',
        variant: toDelete.length > 0 ? 'danger' : 'default',
      },
    );
    if (!confirmed) return;

    this.bulkActionInProgress.set(true);

    const operations = [
      ...toDeactivate.map((c) =>
        this.employeeService.deactivateEmployeeSilently(c.employeeId).pipe(
          map(() => true),
          catchError(() => of(false)),
        ),
      ),
      ...toDelete.map((c) =>
        this.employeeService.deleteEmployeeSilently(c.employeeId).pipe(
          map(() => true),
          catchError(() => of(false)),
        ),
      ),
    ];

    from(operations)
      .pipe(
        concatMap((operation) => operation),
        toArray(),
      )
      .subscribe((results) => {
        this.bulkActionInProgress.set(false);
        const succeeded = results.filter(Boolean).length;
        const failed = results.length - succeeded;
        this.notificationService.show(
          failed === 0
            ? `Bulk action completed: ${toDeactivate.length} deactivated, ${toDelete.length} deleted.`
            : `Bulk action completed with ${failed} failure(s) (${succeeded} succeeded).`,
          failed === 0 ? 'success' : 'error',
        );
        this.refreshAfterRemoval();
      });
  }
}
