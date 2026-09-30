import { signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { EmployeeService } from '../../services/employee.service';
import { EmployeeListComponent } from '../employee-list/employee-list.component';
import { HomeComponent } from './home.component';

describe('HomeComponent', () => {
  // The list provides its own EmployeeService, and that override is compiled
  // once, so each test swaps the stub behind a factory instead.
  let service: Record<string, unknown>;

  const setup = async (
    state: { loading?: boolean; error?: string | null } = {},
  ) => {
    service = {
      employees: signal([]),
      loading: signal(state.loading ?? false),
      error: signal(state.error ?? null),
      totalItems: signal(42),
      bindEmployees: vi.fn(),
      activationLoading: signal(false),
      activationError: signal(null),
      exportLoading: signal(false),
      exportError: signal(null),
    };
    TestBed.configureTestingModule({
      providers: [provideRouter([])],
    });
    TestBed.overrideComponent(EmployeeListComponent, {
      set: {
        providers: [{ provide: EmployeeService, useFactory: () => service }],
      },
    });
    const fixture = TestBed.createComponent(HomeComponent);
    await fixture.whenStable();
    return (fixture.nativeElement as HTMLElement)
      .querySelector('h1')!
      .textContent!.replace(/\s+/g, ' ')
      .trim();
  };

  it('shows the number of employees next to the title', async () => {
    expect(await setup()).toBe('Employees (42)');
  });

  it('leaves the count off while the list is loading', async () => {
    expect(await setup({ loading: true })).toBe('Employees');
  });

  it('leaves the count off when the list fails to load', async () => {
    expect(await setup({ error: 'Failed to load employees' })).toBe(
      'Employees',
    );
  });
});
