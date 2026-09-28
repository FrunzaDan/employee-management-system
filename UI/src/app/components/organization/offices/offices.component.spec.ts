import { HttpErrorResponse } from '@angular/common/http';
import { signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { of, Subject, throwError } from 'rxjs';
import { Office } from '../../../interfaces/office';
import { EmployeeSummary } from '../../../interfaces/employee-summary';
import { EmployeeStatus } from '../../../interfaces/employee';
import { OfficeService } from '../../../services/office.service';
import { ConfirmDialogService } from '../../../services/confirm-dialog.service';
import { OfficesComponent } from './offices.component';

describe('OfficesComponent', () => {
  let loadOffices: ReturnType<typeof vi.fn>;
  let createOffice: ReturnType<typeof vi.fn>;
  let updateOffice: ReturnType<typeof vi.fn>;
  let deleteOffice: ReturnType<typeof vi.fn>;
  let getEmployees: ReturnType<typeof vi.fn>;
  let confirm: ReturnType<typeof vi.fn>;

  const office: Office = {
    officeId: 'office-1',
    name: 'Head office',
    city: null,
    country: 'Romania',
    employeeCount: 2,
    totalGrossSalary: 12000,
  };

  const employee: EmployeeSummary = {
    employeeId: 'employee-1',
    firstName: 'Ana',
    lastName: 'Pop',
    email: 'ana@example.com',
    status: EmployeeStatus.Active,
  };

  const conflict = new HttpErrorResponse({
    status: 409,
    error: { status: 409, detail: 'Office still has employees.' },
  });

  const createComponent = () => {
    loadOffices = vi.fn();
    createOffice = vi
      .fn()
      .mockReturnValue(of({ status: 200, responseMessage: 'ok' }));
    updateOffice = vi
      .fn()
      .mockReturnValue(of({ status: 200, responseMessage: 'ok' }));
    deleteOffice = vi
      .fn()
      .mockReturnValue(of({ status: 200, responseMessage: 'ok' }));
    getEmployees = vi.fn().mockReturnValue(of([employee]));
    confirm = vi.fn().mockResolvedValue(true);

    TestBed.configureTestingModule({
      providers: [
        {
          provide: OfficeService,
          useValue: {
            offices: signal([office]),
            loading: signal(false),
            error: signal(null),
            loadOffices,
            createOffice,
            updateOffice,
            deleteOffice,
            getEmployees,
          },
        },
        { provide: ConfirmDialogService, useValue: { confirm } },
      ],
    });
    return TestBed.runInInjectionContext(() => new OfficesComponent());
  };

  it('loads the offices on init', () => {
    const component = createComponent();

    component.ngOnInit();

    expect(loadOffices).toHaveBeenCalledTimes(1);
  });

  describe('editing', () => {
    it('starts an add with an empty draft', () => {
      const component = createComponent();

      component.startAdd();

      expect(component.draft()).toEqual({
        officeId: null,
        name: '',
        city: '',
        country: '',
      });
    });

    it('starts an edit from the office, turning missing fields into empty text', () => {
      const component = createComponent();

      component.startEdit(office);

      expect(component.draft()).toEqual({
        officeId: 'office-1',
        name: 'Head office',
        city: '',
        country: 'Romania',
      });
    });

    it('updates one field of the draft at a time', () => {
      const component = createComponent();
      component.startAdd();

      component.updateDraft('city', 'Cluj');

      expect(component.draft()?.city).toBe('Cluj');
      expect(component.draft()?.name).toBe('');
    });

    it('cancel throws the draft and any save error away', async () => {
      const component = createComponent();
      component.startAdd();
      await component.save();

      component.cancel();

      expect(component.draft()).toBeNull();
      expect(component.saveError()).toBeNull();
    });
  });

  describe('save', () => {
    it('refuses a blank name without calling the API', async () => {
      const component = createComponent();
      component.startAdd();
      component.updateDraft('name', '   ');

      await component.save();

      expect(component.saveError()).toBe('Office name is required.');
      expect(createOffice).not.toHaveBeenCalled();
    });

    it('creates a new office and closes the draft', async () => {
      const component = createComponent();
      component.startAdd();
      component.updateDraft('name', 'Branch');
      component.updateDraft('city', 'Iasi');

      await component.save();

      expect(createOffice).toHaveBeenCalledWith({
        name: 'Branch',
        city: 'Iasi',
        country: '',
      });
      expect(updateOffice).not.toHaveBeenCalled();
      expect(component.draft()).toBeNull();
      expect(component.saving()).toBe(false);
    });

    it('updates an existing office by its id', async () => {
      const component = createComponent();
      component.startEdit(office);
      component.updateDraft('name', 'Renamed');

      await component.save();

      expect(updateOffice).toHaveBeenCalledWith({
        officeId: 'office-1',
        name: 'Renamed',
        city: '',
        country: 'Romania',
      });
      expect(createOffice).not.toHaveBeenCalled();
    });

    it('keeps the draft open and shows the error when the save fails', async () => {
      const component = createComponent();
      updateOffice.mockReturnValue(throwError(() => conflict));
      component.startEdit(office);

      await component.save();

      expect(component.saveError()).toBe('Office still has employees.');
      expect(component.draft()).not.toBeNull();
      expect(component.saving()).toBe(false);
    });
  });

  describe('delete', () => {
    it('asks first and names the office', async () => {
      const component = createComponent();

      await component.deleteOffice(office);

      expect(confirm).toHaveBeenCalledWith(
        'Delete office "Head office"? This cannot be undone.',
        expect.objectContaining({ variant: 'danger' }),
      );
      expect(deleteOffice).toHaveBeenCalledWith('office-1');
    });

    it('does nothing when the user cancels', async () => {
      const component = createComponent();
      confirm.mockResolvedValue(false);

      await component.deleteOffice(office);

      expect(deleteOffice).not.toHaveBeenCalled();
    });

    it('shows why the delete failed', async () => {
      const component = createComponent();
      deleteOffice.mockReturnValue(throwError(() => conflict));

      await component.deleteOffice(office);

      expect(component.deleteError()).toBe('Office still has employees.');
    });
  });

  describe('toggleEmployees', () => {
    it('expands the office and lists its employees', () => {
      const component = createComponent();

      component.toggleEmployees(office);

      expect(getEmployees).toHaveBeenCalledWith('office-1');
      expect(component.expandedOfficeId()).toBe('office-1');
      expect(component.expandedEmployees()).toEqual([employee]);
      expect(component.expandedEmployeesLoading()).toBe(false);
    });

    it('shows the loading state until the employees arrive', () => {
      const component = createComponent();
      const employees$ = new Subject<EmployeeSummary[]>();
      getEmployees.mockReturnValue(employees$);

      component.toggleEmployees(office);
      expect(component.expandedEmployeesLoading()).toBe(true);

      employees$.next([employee]);
      expect(component.expandedEmployeesLoading()).toBe(false);
    });

    it('collapses on a second click without asking the API again', () => {
      const component = createComponent();
      component.toggleEmployees(office);

      component.toggleEmployees(office);

      expect(component.expandedOfficeId()).toBeNull();
      expect(getEmployees).toHaveBeenCalledTimes(1);
    });

    it('shows an error when the employees cannot be loaded', () => {
      const component = createComponent();
      getEmployees.mockReturnValue(
        throwError(() => new HttpErrorResponse({ status: 503 })),
      );

      component.toggleEmployees(office);

      expect(component.expandedEmployeesError()).toBe(
        'Failed to load employees (503). Please try again.',
      );
      expect(component.expandedEmployeesLoading()).toBe(false);
    });
  });
});
