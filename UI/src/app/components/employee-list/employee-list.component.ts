// employee-list.component.ts
import { Component, OnInit, computed, effect, signal, Signal, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { catchError, concatMap, from, map, of, toArray } from 'rxjs';
import { GetEmployeeService } from '../../services/get-employee.service';
import { ActivateEmployeeService } from '../../services/activate-employee.service';
import { ConfirmDialogService } from '../../services/confirm-dialog.service';
import { DeleteEmployeeService } from '../../services/delete-employee.service';
import { ExportEmployeeService } from '../../services/export-employee.service';
import { NotificationService } from '../../services/notification.service';
import {
  Employee,
  EmployeeActivationStatus,
} from '../../interfaces/employee-response';
import { extractErrorMessage } from '../../utils/extract-error-message';

@Component({
  selector: 'app-employee-list',
  templateUrl: './employee-list.component.html',
  styleUrls: ['./employee-list.component.css'],
  imports: [RouterLink],
})
export class EmployeeListComponent implements OnInit {
  // Use dependency injection with inject()
  private readonly getEmployeeService = inject(GetEmployeeService);
  private readonly activateEmployeeService = inject(ActivateEmployeeService);
  private readonly confirmDialogService = inject(ConfirmDialogService);
  private readonly deleteEmployeeService = inject(DeleteEmployeeService);
  private readonly exportEmployeeService = inject(ExportEmployeeService);
  private readonly notificationService = inject(NotificationService);

  // Public signals for template
  readonly employees = this.getEmployeeService.employeesSignal;
  readonly isLoading = this.getEmployeeService.loadingSignal;
  readonly errorMessage = this.getEmployeeService.errorSignal;
  readonly activationLoading = this.activateEmployeeService.loadingSignal;
  readonly activationError = this.activateEmployeeService.errorSignal;

  // Delete is a separate action from deactivate/reactivate, so it gets its own
  // in-flight/error state rather than being folded into activationLoading/Error.
  readonly deleting = signal(false);
  readonly deleteError = signal<string | null>(null);

  // Bulk-delete selection is scoped to the current page only — the checkboxes
  // reference rows that actually exist in the browser, and selection is reset
  // on every fetchEmployees() (page/search/sort change, or after the bulk
  // action itself refreshes the page).
  readonly selectedGuids = signal<ReadonlySet<string>>(new Set());
  readonly bulkActionInProgress = signal(false);

  readonly allOnPageSelected = computed(
    () =>
      this.employees().length > 0 &&
      this.employees().every((c) => this.selectedGuids().has(c.guid)),
  );

  // CSV export exports whatever the list is currently searching/sorted by,
  // not just the current page — see ExportEmployeeService.
  readonly exportLoading = this.exportEmployeeService.loadingSignal;
  readonly exportError = this.exportEmployeeService.errorSignal;

  // Add EmployeeStatus enum for better type checking
  readonly EmployeeStatus = EmployeeActivationStatus;

  readonly statusLabels = new Map<Employee['employeeStatus'], string>([
    [EmployeeActivationStatus.Active, 'Active'],
    [EmployeeActivationStatus.Deactivated, 'Deactivated'],
    [EmployeeActivationStatus.Test, 'Test'],
  ]);

  // Search, sorting, and pagination are all server-side now: every change to
  // any of these re-fetches just the relevant page from the API rather than
  // filtering/sorting an already-loaded full list in memory (see
  // GetEmployeeService.loadEmployees and usp_getEmployees).
  readonly searchTerm = signal('');
  readonly sortColumn = signal<'name' | 'email' | 'msisdn'>('name');
  readonly sortDirection = signal<'asc' | 'desc'>('asc');

  readonly pageSize = 50;
  readonly currentPage = signal(1);

  readonly totalItems = this.getEmployeeService.totalItemsSignal;
  readonly totalPages = computed(() =>
    Math.max(1, Math.ceil(this.totalItems() / this.pageSize)),
  );

  // Spoken by the polite live region so a screen-reader user hears the outcome
  // of a search / page change without hunting for it.
  readonly resultsAnnouncement = computed(() => {
    if (this.isLoading()) return 'Loading employees';
    const total = this.totalItems();
    return `${total} ${total === 1 ? 'employee' : 'employees'} found`;
  });

  readonly tableCaption = computed(
    () =>
      `Employees, page ${this.currentPage()} of ${this.totalPages()}, sorted by ${this.sortColumn()} ${this.sortDirection() === 'asc' ? 'ascending' : 'descending'}`,
  );

  // Debounced so typing doesn't fire an API call per keystroke — the search
  // used to be a synchronous in-memory filter, but now it's a network call.
  private searchDebounceTimer: ReturnType<typeof setTimeout> | undefined;
  private static readonly SEARCH_DEBOUNCE_MS = 300;

  // Computed signal for duplicate GUIDs
  readonly duplicateGuids = computed(() => {
    const employees = this.employees();
    const guidCount = new Map<string, number>();

    employees.forEach((employee) => {
      const count = guidCount.get(employee.guid) ?? 0;
      guidCount.set(employee.guid, count + 1);
    });

    return Array.from(guidCount.entries())
      .filter(([_, count]) => count > 1)
      .map(([guid]) => guid);
  });

  constructor() {
    // duplicateGuids() is a computed signal, so re-run this check whenever
    // it actually changes instead of only once, synchronously, right after
    // the (async) loadEmployees() call in ngOnInit.
    effect(() => {
      const duplicates = this.duplicateGuids();
      if (duplicates.length > 0) {
        console.warn('Duplicate GUIDs found:', duplicates);
      }
    });

  }

  onSearchInput(value: string): void {
    this.searchTerm.set(value);

    clearTimeout(this.searchDebounceTimer);
    this.searchDebounceTimer = setTimeout(() => {
      // A narrower search can make the current page go out of range (e.g.
      // you're on page 3, then a search narrows results to one page) —
      // snap back to page 1 on every new search term.
      this.currentPage.set(1);
      this.fetchEmployees();
    }, EmployeeListComponent.SEARCH_DEBOUNCE_MS);
  }

  goToPage(page: number): void {
    const target = Math.min(Math.max(page, 1), this.totalPages());
    if (target === this.currentPage()) return;
    this.currentPage.set(target);
    this.fetchEmployees();
  }

  // Exposed on the <th> so assistive tech announces the current sort.
  ariaSort(column: 'name' | 'email' | 'msisdn'): 'ascending' | 'descending' | 'none' {
    if (this.sortColumn() !== column) return 'none';
    return this.sortDirection() === 'asc' ? 'ascending' : 'descending';
  }

  setSort(column: 'name' | 'email' | 'msisdn'): void {
    if (this.sortColumn() === column) {
      this.sortDirection.set(this.sortDirection() === 'asc' ? 'desc' : 'asc');
    } else {
      this.sortColumn.set(column);
      this.sortDirection.set('asc');
    }
    this.currentPage.set(1);
    this.fetchEmployees();
  }

  ngOnInit(): void {
    this.fetchEmployees();
  }

  exportCsv(): void {
    this.exportEmployeeService.exportEmployees({
      searchTerm: this.searchTerm().trim() || undefined,
      sortColumn: this.sortColumn(),
      sortDirection: this.sortDirection(),
    });
  }

  private fetchEmployees(): void {
    this.selectedGuids.set(new Set());
    this.getEmployeeService.loadEmployees({
      pageNumber: this.currentPage(),
      pageSize: this.pageSize,
      searchTerm: this.searchTerm().trim() || undefined,
      sortColumn: this.sortColumn(),
      sortDirection: this.sortDirection(),
    });
  }

  // Employee action methods
  async deactivateEmployee(guid: string): Promise<void> {
    const confirmed = await this.confirmDialogService.confirm(
      'Are you sure you want to deactivate this employee?',
    );
    if (!confirmed) return;
    this.activateEmployeeService.deactivateEmployee(guid);
  }

  reactivateEmployee(guid: string): void {
    this.activateEmployeeService.reactivateEmployee(guid);
  }

  async deleteEmployee(guid: string): Promise<void> {
    const confirmed = await this.confirmDialogService.confirm(
      'Are you sure you want to permanently delete this employee? This cannot be undone.',
    );
    if (!confirmed) return;

    this.deleting.set(true);
    this.deleteError.set(null);

    this.deleteEmployeeService.deleteEmployee(guid).subscribe({
      next: () => {
        this.deleting.set(false);
        // removeEmployeeLocally() (called by DeleteEmployeeService) only
        // drops the row from the in-memory page — totalItems/page count
        // would go stale without a real re-fetch of the current page.
        this.fetchEmployees();
      },
      error: (error: HttpErrorResponse) => {
        this.deleting.set(false);
        this.deleteError.set(extractErrorMessage(error));
      },
    });
  }

  isSelected(guid: string): boolean {
    return this.selectedGuids().has(guid);
  }

  toggleSelection(guid: string, checked: boolean): void {
    const next = new Set(this.selectedGuids());
    if (checked) {
      next.add(guid);
    } else {
      next.delete(guid);
    }
    this.selectedGuids.set(next);
  }

  toggleSelectAllOnPage(checked: boolean): void {
    const next = new Set(this.selectedGuids());
    for (const employee of this.employees()) {
      if (checked) {
        next.add(employee.guid);
      } else {
        next.delete(employee.guid);
      }
    }
    this.selectedGuids.set(next);
  }

  // A employee must be Deactivated (or Test, which is exempt from that rule —
  // see usp_deleteEmployee) to be deleted directly; an Active one is only
  // deactivated as part of this action, not deleted, same as the single-row
  // buttons would require.
  async bulkDeleteSelected(): Promise<void> {
    const guids = this.selectedGuids();
    const selected = this.employees().filter((c) => guids.has(c.guid));
    if (selected.length === 0) return;

    const toDeactivate = selected.filter(
      (c) => c.employeeStatus === EmployeeActivationStatus.Active,
    );
    const toDelete = selected.filter(
      (c) => c.employeeStatus !== EmployeeActivationStatus.Active,
    );

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

    const confirmed = await this.confirmDialogService.confirm(lines.join('\n'));
    if (!confirmed) return;

    this.bulkActionInProgress.set(true);

    const operations = [
      ...toDeactivate.map((c) =>
        this.activateEmployeeService.deactivateEmployeeSilently(c.guid).pipe(
          map(() => true),
          catchError(() => of(false)),
        ),
      ),
      ...toDelete.map((c) =>
        this.deleteEmployeeService.deleteEmployeeSilently(c.guid).pipe(
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
        this.fetchEmployees();
      });
  }
}
