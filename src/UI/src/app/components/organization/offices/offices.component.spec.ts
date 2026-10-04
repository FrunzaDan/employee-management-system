import { HttpErrorResponse } from '@angular/common/http';
import { signal } from '@angular/core';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { submit } from '@angular/forms/signals';
import { provideRouter } from '@angular/router';
import { of, throwError } from 'rxjs';
import { Office } from '../../../interfaces/office';
import { OfficeService } from '../../../services/office.service';
import { ConfirmDialogService } from '../../../services/confirm-dialog.service';
import { OfficesComponent } from './offices.component';

describe('OfficesComponent', () => {
  let loadOffices: ReturnType<typeof vi.fn>;
  let createOffice: ReturnType<typeof vi.fn>;
  let updateOffice: ReturnType<typeof vi.fn>;
  let deleteOffice: ReturnType<typeof vi.fn>;
  let confirm: ReturnType<typeof vi.fn>;

  const office: Office = {
    officeId: 'office-1',
    name: 'Head office',
    city: null,
    country: 'Romania',
    employeeCount: 2,
    totalGrossSalary: 12000,
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

      expect(component.officeForm().value()).toEqual({
        officeId: null,
        name: '',
        city: '',
        country: '',
      });
    });

    it('starts an edit from the office, turning missing fields into empty text', () => {
      const component = createComponent();

      component.startEdit(office);

      expect(component.officeForm().value()).toEqual({
        officeId: 'office-1',
        name: 'Head office',
        city: '',
        country: 'Romania',
      });
    });

    it('updates one field of the draft at a time', () => {
      const component = createComponent();
      component.startAdd();

      component.officeForm.city().value.set('Cluj');

      expect(component.officeForm.city().value()).toBe('Cluj');
      expect(component.officeForm.name().value()).toBe('');
    });

    it('cancel throws the draft and any save error away', async () => {
      const component = createComponent();
      component.startAdd();
      await submit(component.officeForm);

      component.cancel();

      expect(component.editorOpen()).toBe(false);
      expect(component.saveError()).toBeNull();
    });
  });

  describe('save', () => {
    it('refuses a blank name without calling the API', async () => {
      const component = createComponent();
      component.startAdd();
      component.officeForm.name().value.set('   ');

      await submit(component.officeForm);

      expect(component.officeForm.name().errors()[0].message).toBe(
        'Office name is required.',
      );
      expect(component.saveError()).toBeNull();
      expect(createOffice).not.toHaveBeenCalled();
    });

    it('creates a new office and closes the draft', async () => {
      const component = createComponent();
      component.startAdd();
      component.officeForm.name().value.set('Branch');
      component.officeForm.city().value.set('Iasi');

      await submit(component.officeForm);

      expect(createOffice).toHaveBeenCalledWith({
        name: 'Branch',
        city: 'Iasi',
        country: '',
      });
      expect(updateOffice).not.toHaveBeenCalled();
      expect(component.editorOpen()).toBe(false);
      expect(component.officeForm().submitting()).toBe(false);
    });

    it('updates an existing office by its id', async () => {
      const component = createComponent();
      component.startEdit(office);
      component.officeForm.name().value.set('Renamed');

      await submit(component.officeForm);

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

      await submit(component.officeForm);

      expect(component.saveError()).toBe('Office still has employees.');
      expect(component.editorOpen()).toBe(true);
      expect(component.officeForm().submitting()).toBe(false);
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
    it('expands the office to show its employees', () => {
      const component = createComponent();

      component.toggleEmployees(office);

      expect(component.expandedOfficeId()).toBe('office-1');
    });

    it('collapses on a second click', () => {
      const component = createComponent();
      component.toggleEmployees(office);

      component.toggleEmployees(office);

      expect(component.expandedOfficeId()).toBeNull();
    });
  });

  describe('template', () => {
    let fixture: ComponentFixture<OfficesComponent>;

    const el = <T extends HTMLElement>(selector: string): T =>
      fixture.nativeElement.querySelector(selector);
    const buttonByText = (text: string): HTMLButtonElement =>
      Array.from<HTMLButtonElement>(
        fixture.nativeElement.querySelectorAll('button'),
      ).find((b) => b.textContent?.trim() === text)!;
    const type = (selector: string, value: string) => {
      const input = el<HTMLInputElement>(selector);
      input.value = value;
      input.dispatchEvent(new Event('input'));
    };
    const settle = () => new Promise((resolve) => setTimeout(resolve));

    beforeEach(async () => {
      TestBed.configureTestingModule({ providers: [provideRouter([])] });
      createComponent();
      fixture = TestBed.createComponent(OfficesComponent);
      await fixture.whenStable();
    });

    it('loads and renders each office, with a dash for a missing city', () => {
      const row = el('tbody tr');

      expect(loadOffices).toHaveBeenCalledTimes(1);
      expect(row.textContent).toContain('Head office');
      expect(row.textContent).toContain('—');
      expect(row.textContent).toContain('Romania');
      expect(row.textContent).toContain('12.000,00 RON');
    });

    it('creates an office from the Add office form', async () => {
      buttonByText('Add office').click();
      await fixture.whenStable();
      expect(el('form h2').textContent).toContain('New office');

      type('#name', 'Branch');
      type('#officeCity', 'Iasi');
      buttonByText('Save').click();
      await settle();

      expect(createOffice).toHaveBeenCalledWith({
        name: 'Branch',
        city: 'Iasi',
        country: '',
      });
    });

    it('shows the error under the name and does not save a blank office', async () => {
      buttonByText('Add office').click();
      await fixture.whenStable();

      buttonByText('Save').click();
      await settle();
      await fixture.whenStable();

      expect(el('#name-error').textContent).toContain(
        'Office name is required.',
      );
      expect(createOffice).not.toHaveBeenCalled();
    });

    it("edits the row's office from its pre-filled Edit form", async () => {
      buttonByText('Edit').click();
      await fixture.whenStable();
      expect(el('form h2').textContent).toContain('Edit office');
      expect(el<HTMLInputElement>('#name').value).toBe('Head office');

      type('#name', 'Renamed');
      buttonByText('Save').click();
      await settle();

      expect(updateOffice).toHaveBeenCalledWith(
        expect.objectContaining({ officeId: 'office-1', name: 'Renamed' }),
      );
      expect(createOffice).not.toHaveBeenCalled();
    });

    it("deletes the row's office from its Delete button", async () => {
      buttonByText('Delete').click();
      await settle();

      expect(deleteOffice).toHaveBeenCalledWith('office-1');
    });
  });
});
