import { RenderMode, ServerRoute } from '@angular/ssr';

// Routes with an id in the path can't be prerendered at build time (there's no list of
// ids to render), so they render per request; everything else is prerendered as before.
export const serverRoutes: ServerRoute[] = [
  { path: 'employees/:employeeId', renderMode: RenderMode.Server },
  { path: 'employees/update/:employeeId', renderMode: RenderMode.Server },
  { path: 'offices/:officeId', renderMode: RenderMode.Server },
  { path: 'departments/:departmentId', renderMode: RenderMode.Server },
  { path: 'cost-centers/:costCenterId', renderMode: RenderMode.Server },
  { path: '**', renderMode: RenderMode.Prerender },
];
