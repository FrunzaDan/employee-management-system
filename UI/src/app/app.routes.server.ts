import { RenderMode, ServerRoute } from '@angular/ssr';

export const serverRoutes: ServerRoute[] = [
  { path: 'employees/:employeeId', renderMode: RenderMode.Server },
  { path: 'employees/update/:employeeId', renderMode: RenderMode.Server },
  { path: 'offices/:officeId', renderMode: RenderMode.Server },
  { path: 'departments/:departmentId', renderMode: RenderMode.Server },
  { path: 'cost-centers/:costCenterId', renderMode: RenderMode.Server },
  { path: '**', renderMode: RenderMode.Prerender },
];
