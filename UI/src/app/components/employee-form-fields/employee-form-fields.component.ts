import { Component, inject, input, OnInit } from '@angular/core';
import { FieldTree, FormField } from '@angular/forms/signals';
import { EmployeeFormModel } from './employee-form';
import { OfficeService } from '../../services/office.service';
import { DepartmentService } from '../../services/department.service';
import { CostCenterService } from '../../services/cost-center.service';

// The three form cards (personal details + address + job information) shared by
// add-employee and edit-employee. The parent owns the form and its submission;
// this only renders.
@Component({
  selector: 'app-employee-form-fields',
  templateUrl: './employee-form-fields.component.html',
  imports: [FormField],
})
export class EmployeeFormFieldsComponent implements OnInit {
  readonly form = input.required<FieldTree<EmployeeFormModel>>();

  private readonly officeService = inject(OfficeService);
  private readonly departmentService = inject(DepartmentService);
  private readonly costCenterService = inject(CostCenterService);

  protected readonly offices = this.officeService.officesSignal;
  protected readonly departments = this.departmentService.departmentsSignal;
  protected readonly costCenters = this.costCenterService.costCentersSignal;

  ngOnInit(): void {
    // Small reference lists (see OfficeService et al.) — loaded once per form
    // instance, not paginated/searched like the employee list is.
    this.officeService.loadOffices();
    this.departmentService.loadDepartments();
    this.costCenterService.loadCostCenters();
  }

  protected readonly basicFields = [
    { key: 'firstName', label: 'First Name' },
    { key: 'lastName', label: 'Last Name' },
    { key: 'email', label: 'Email' },
    { key: 'phoneNumber', label: 'Phone' },
  ] as const;

  protected readonly addressFields = [
    { key: 'country', label: 'Country' },
    { key: 'county', label: 'County' },
    { key: 'city', label: 'City' },
    { key: 'street', label: 'Street' },
    { key: 'streetNumber', label: 'Street number' },
    { key: 'postalCode', label: 'Postal code' },
  ] as const;
}
