import { inject } from '@angular/core';
import { CanActivateFn, Routes } from '@angular/router';
import { AuthGuardService } from './services/auth-guard.service';
import { unsavedChangesGuard } from './services/unsaved-changes.guard';

const authGuardFn: CanActivateFn = () => {
  const authService = inject(AuthGuardService);
  return authService.canActivate();
};

// Every page is lazy-loaded so the initial bundle only carries the shell;
// `title` feeds AppTitleStrategy (document title = WCAG 2.4.2).
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
    pathMatch: 'full',
    loadComponent: () =>
      import('./components/home/home.component').then((m) => m.HomeComponent),
    canActivate: [authGuardFn],
    title: 'Employees',
  },
  {
    path: 'employees',
    loadComponent: () =>
      import('./components/home/home.component').then((m) => m.HomeComponent),
    canActivate: [authGuardFn],
    title: 'Employees',
  },
  {
    path: 'addEmployee',
    loadComponent: () =>
      import('./components/add-employee/add-employee.component').then(
        (m) => m.AddEmployeeComponent,
      ),
    canActivate: [authGuardFn],
    canDeactivate: [unsavedChangesGuard],
    title: 'Register employee',
  },
  {
    path: 'editEmployee',
    loadComponent: () =>
      import('./components/edit-employee/edit-employee.component').then(
        (m) => m.EditEmployeeComponent,
      ),
    canActivate: [authGuardFn],
    canDeactivate: [unsavedChangesGuard],
    title: 'Edit employee',
  },
  {
    path: 'about',
    loadComponent: () =>
      import('./components/about/about.component').then(
        (m) => m.AboutComponent,
      ),
    canActivate: [authGuardFn],
    title: 'About',
  },
  {
    path: 'employeeDetails',
    loadComponent: () =>
      import('./components/employee-details/employee-details.component').then(
        (m) => m.EmployeeDetailsComponent,
      ),
    canActivate: [authGuardFn],
    title: 'Employee details',
  },
  {
    path: 'auditLog',
    loadComponent: () =>
      import('./components/global-audit-log/global-audit-log.component').then(
        (m) => m.GlobalAuditLogComponent,
      ),
    canActivate: [authGuardFn],
    title: 'Audit log',
  },
  {
    path: 'offices',
    loadComponent: () =>
      import('./components/organization/offices/offices.component').then(
        (m) => m.OfficesComponent,
      ),
    canActivate: [authGuardFn],
    title: 'Offices',
  },
  {
    path: 'officeDetails',
    loadComponent: () =>
      import('./components/organization/office-details/office-details.component').then(
        (m) => m.OfficeDetailsComponent,
      ),
    canActivate: [authGuardFn],
    title: 'Office details',
  },
  {
    path: 'departments',
    loadComponent: () =>
      import('./components/organization/departments/departments.component').then(
        (m) => m.DepartmentsComponent,
      ),
    canActivate: [authGuardFn],
    title: 'Departments',
  },
  {
    path: 'departmentDetails',
    loadComponent: () =>
      import('./components/organization/department-details/department-details.component').then(
        (m) => m.DepartmentDetailsComponent,
      ),
    canActivate: [authGuardFn],
    title: 'Department details',
  },
  {
    path: 'costCenters',
    loadComponent: () =>
      import('./components/organization/cost-centers/cost-centers.component').then(
        (m) => m.CostCentersComponent,
      ),
    canActivate: [authGuardFn],
    title: 'Cost centers',
  },
  {
    path: 'costCenterDetails',
    loadComponent: () =>
      import('./components/organization/cost-center-details/cost-center-details.component').then(
        (m) => m.CostCenterDetailsComponent,
      ),
    canActivate: [authGuardFn],
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
