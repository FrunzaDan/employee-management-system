import { HttpErrorResponse } from '@angular/common/http';
import { signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { submit } from '@angular/forms/signals';
import { of, throwError } from 'rxjs';
import { CostCenter } from '../../../interfaces/cost-center';
import { CostCenterService } from '../../../services/cost-center.service';
import { ConfirmDialogService } from '../../../services/confirm-dialog.service';
import { CostCentersComponent } from './cost-centers.component';

describe('CostCentersComponent', () => {
  let loadCostCenters: ReturnType<typeof vi.fn>;
  let createCostCenter: ReturnType<typeof vi.fn>;
  let updateCostCenter: ReturnType<typeof vi.fn>;
  let deleteCostCenter: ReturnType<typeof vi.fn>;
  let confirm: ReturnType<typeof vi.fn>;

  const costCenter: CostCenter = {
    costCenterId: 'cost-center-1',
    code: 'CC-100',
    name: null,
    employeeCount: 2,
    totalGrossSalary: 12000,
  };

  const conflict = new HttpErrorResponse({
    status: 409,
    error: { status: 409, detail: 'Cost center still has employees.' },
  });

  const createComponent = () => {
    loadCostCenters = vi.fn();
    createCostCenter = vi
      .fn()
      .mockReturnValue(of({ status: 200, responseMessage: 'ok' }));
    updateCostCenter = vi
      .fn()
      .mockReturnValue(of({ status: 200, responseMessage: 'ok' }));
    deleteCostCenter = vi
      .fn()
      .mockReturnValue(of({ status: 200, responseMessage: 'ok' }));
    confirm = vi.fn().mockResolvedValue(true);

    TestBed.configureTestingModule({
      providers: [
        {
          provide: CostCenterService,
          useValue: {
            costCenters: signal([costCenter]),
            loading: signal(false),
            error: signal(null),
            loadCostCenters,
            createCostCenter,
            updateCostCenter,
            deleteCostCenter,
          },
        },
        { provide: ConfirmDialogService, useValue: { confirm } },
      ],
    });
    return TestBed.runInInjectionContext(() => new CostCentersComponent());
  };

  it('loads the cost centers on init', () => {
    const component = createComponent();

    component.ngOnInit();

    expect(loadCostCenters).toHaveBeenCalledTimes(1);
  });

  it('starts an add with an empty draft and an edit from the cost center', () => {
    const component = createComponent();

    component.startAdd();
    expect(component.costCenterForm().value()).toEqual({
      costCenterId: null,
      code: '',
      name: '',
    });

    component.startEdit(costCenter);
    expect(component.costCenterForm().value()).toEqual({
      costCenterId: 'cost-center-1',
      code: 'CC-100',
      name: '',
    });
  });

  describe('save', () => {
    it('refuses a blank code without calling the API, even when a name is given', async () => {
      const component = createComponent();
      component.startAdd();
      component.costCenterForm.name().value.set('Sales');

      await submit(component.costCenterForm);

      expect(component.costCenterForm.code().errors()[0].message).toBe(
        'Cost center code is required.',
      );
      expect(component.saveError()).toBeNull();
      expect(createCostCenter).not.toHaveBeenCalled();
    });

    it('creates a new cost center and closes the draft', async () => {
      const component = createComponent();
      component.startAdd();
      component.costCenterForm.code().value.set('CC-200');

      await submit(component.costCenterForm);

      expect(createCostCenter).toHaveBeenCalledWith({
        code: 'CC-200',
        name: '',
      });
      expect(component.editorOpen()).toBe(false);
      expect(component.costCenterForm().submitting()).toBe(false);
    });

    it('updates an existing cost center by its id', async () => {
      const component = createComponent();
      component.startEdit(costCenter);
      component.costCenterForm.name().value.set('Engineering');

      await submit(component.costCenterForm);

      expect(updateCostCenter).toHaveBeenCalledWith({
        costCenterId: 'cost-center-1',
        code: 'CC-100',
        name: 'Engineering',
      });
      expect(createCostCenter).not.toHaveBeenCalled();
    });

    it('keeps the draft open and shows the error when the save fails', async () => {
      const component = createComponent();
      updateCostCenter.mockReturnValue(throwError(() => conflict));
      component.startEdit(costCenter);

      await submit(component.costCenterForm);

      expect(component.saveError()).toBe('Cost center still has employees.');
      expect(component.editorOpen()).toBe(true);
      expect(component.costCenterForm().submitting()).toBe(false);
    });
  });

  describe('delete', () => {
    it('asks first, naming the cost center by its code, then deletes it', async () => {
      const component = createComponent();

      await component.deleteCostCenter(costCenter);

      expect(confirm).toHaveBeenCalledWith(
        'Delete cost center "CC-100"? This cannot be undone.',
        expect.objectContaining({ variant: 'danger' }),
      );
      expect(deleteCostCenter).toHaveBeenCalledWith('cost-center-1');
    });

    it('does nothing when the user cancels', async () => {
      const component = createComponent();
      confirm.mockResolvedValue(false);

      await component.deleteCostCenter(costCenter);

      expect(deleteCostCenter).not.toHaveBeenCalled();
    });

    it('shows why the delete failed', async () => {
      const component = createComponent();
      deleteCostCenter.mockReturnValue(throwError(() => conflict));

      await component.deleteCostCenter(costCenter);

      expect(component.deleteError()).toBe('Cost center still has employees.');
    });
  });

  describe('toggleEmployees', () => {
    it('expands the cost center to show its employees', () => {
      const component = createComponent();

      component.toggleEmployees(costCenter);

      expect(component.expandedCostCenterId()).toBe('cost-center-1');
    });

    it('collapses on a second click', () => {
      const component = createComponent();
      component.toggleEmployees(costCenter);

      component.toggleEmployees(costCenter);

      expect(component.expandedCostCenterId()).toBeNull();
    });
  });
});
