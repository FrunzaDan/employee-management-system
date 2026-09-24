import { Routes } from '@angular/router';
import { authGuard } from './services/auth.guard';
import { unsavedChangesGuard } from './services/unsaved-changes.guard';

export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () =>
      import('./components/user-login/user-login.component').then(
        (m) => m.UserLoginComponent,
      ),
    title: 'Sign in',
  },
  {
    path: '',
    redirectTo: 'employees',
    pathMatch: 'full',
  },
  {
    path: 'employees',
    loadComponent: () =>
      import('./components/home/home.component').then((m) => m.HomeComponent),
    canActivate: [authGuard],
    title: 'Employees',
  },
  {
    path: 'create-employee',
    loadComponent: () =>
      import('./components/create-employee/create-employee.component').then(
        (m) => m.CreateEmployeeComponent,
      ),
    canActivate: [authGuard],
    canDeactivate: [unsavedChangesGuard],
    title: 'Add employee',
  },
  {
    path: 'employees/update/:employeeId',
    loadComponent: () =>
      import('./components/update-employee/update-employee.component').then(
        (m) => m.UpdateEmployeeComponent,
      ),
    canActivate: [authGuard],
    canDeactivate: [unsavedChangesGuard],
    title: 'Edit employee',
  },
  {
    path: 'about',
    loadComponent: () =>
      import('./components/about/about.component').then(
        (m) => m.AboutComponent,
      ),
    canActivate: [authGuard],
    title: 'About',
  },
  {
    path: 'employees/:employeeId',
    loadComponent: () =>
      import('./components/employee-details/employee-details.component').then(
        (m) => m.EmployeeDetailsComponent,
      ),
    canActivate: [authGuard],
    title: 'Employee details',
  },
  {
    path: 'audit-log',
    loadComponent: () =>
      import('./components/global-audit-log/global-audit-log.component').then(
        (m) => m.GlobalAuditLogComponent,
      ),
    canActivate: [authGuard],
    title: 'Audit log',
  },
  {
    path: 'offices',
    loadComponent: () =>
      import('./components/organization/offices/offices.component').then(
        (m) => m.OfficesComponent,
      ),
    canActivate: [authGuard],
    title: 'Offices',
  },
  {
    path: 'offices/:officeId',
    loadComponent: () =>
      import('./components/organization/office-details/office-details.component').then(
        (m) => m.OfficeDetailsComponent,
      ),
    canActivate: [authGuard],
    title: 'Office details',
  },
  {
    path: 'departments',
    loadComponent: () =>
      import('./components/organization/departments/departments.component').then(
        (m) => m.DepartmentsComponent,
      ),
    canActivate: [authGuard],
    title: 'Departments',
  },
  {
    path: 'departments/:departmentId',
    loadComponent: () =>
      import('./components/organization/department-details/department-details.component').then(
        (m) => m.DepartmentDetailsComponent,
      ),
    canActivate: [authGuard],
    title: 'Department details',
  },
  {
    path: 'cost-centers',
    loadComponent: () =>
      import('./components/organization/cost-centers/cost-centers.component').then(
        (m) => m.CostCentersComponent,
      ),
    canActivate: [authGuard],
    title: 'Cost centers',
  },
  {
    path: 'cost-centers/:costCenterId',
    loadComponent: () =>
      import('./components/organization/cost-center-details/cost-center-details.component').then(
        (m) => m.CostCenterDetailsComponent,
      ),
    canActivate: [authGuard],
    title: 'Cost center details',
  },
  {
    path: '**',
    loadComponent: () =>
      import('./components/page-not-found/page-not-found.component').then(
        (m) => m.PageNotFoundComponent,
      ),
    title: 'Page not found',
  },
];
