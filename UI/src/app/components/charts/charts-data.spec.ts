import { EmployeeProfile } from '../../interfaces/employee-insights';
import {
  ageDistribution,
  averagePayBy,
  currentWorkforce,
  genderLabel,
  genderSlices,
  headcountBy,
  hireCounts,
  payrollBy,
  payVsTenure,
  salaried,
  statusSlices,
  tenureDistribution,
  tenureYears,
} from './charts-data';

const TODAY = new Date(2026, 8, 28);

const buildEmployee = (
  overrides: Partial<EmployeeProfile> = {},
): EmployeeProfile => ({
  status: 1901,
  gender: 1,
  birthDate: '1990-01-01',
  hireDate: '2016-09-28',
  departmentName: 'Engineering',
  officeName: 'HQ',
  currentGrossSalary: 8000,
  ...overrides,
});

describe('currentWorkforce', () => {
  it('leaves out deactivated employees', () => {
    const employees = [
      buildEmployee({ status: 1901 }),
      buildEmployee({ status: 1904 }),
      buildEmployee({ status: 1903 }),
    ];

    expect(currentWorkforce(employees).map((e) => e.status)).toEqual([
      1901, 1904,
    ]);
  });
});

describe('salaried', () => {
  it('keeps only employees with a current salary', () => {
    const employees = [
      buildEmployee({ currentGrossSalary: null }),
      buildEmployee({ currentGrossSalary: 5000 }),
    ];

    expect(salaried(employees)).toHaveLength(1);
  });
});

describe('statusSlices and genderSlices', () => {
  it('count every status and gender, keeping zero slices', () => {
    const employees = [
      buildEmployee({ status: 1904, gender: 2 }),
      buildEmployee({ status: 1903, gender: 2 }),
    ];

    expect(statusSlices(employees)).toEqual([
      { label: 'Active', value: 0 },
      { label: 'Test', value: 1 },
      { label: 'Deactivated', value: 1 },
    ]);
    expect(genderSlices(employees)).toEqual([
      { label: 'Female', value: 2 },
      { label: 'Male', value: 0 },
      { label: 'Not declared', value: 0 },
    ]);
  });
});

describe('ageDistribution', () => {
  it('bands current ages and skips missing birth dates', () => {
    const employees = [
      buildEmployee({ birthDate: '2002-09-29' }),
      buildEmployee({ birthDate: '1980-01-01' }),
      buildEmployee({ birthDate: null }),
    ];

    expect(ageDistribution(employees, TODAY).map((p) => p.value)).toEqual([
      1, 0, 0, 1, 0, 0,
    ]);
  });
});

describe('tenure', () => {
  it('uses the hire date and skips employees without one', () => {
    const employees = [
      buildEmployee({ hireDate: '2016-09-28' }),
      buildEmployee({ hireDate: '2026-03-01' }),
      buildEmployee({ hireDate: null }),
    ];

    expect(tenureYears(employees, TODAY)).toHaveLength(2);
    expect(tenureYears(employees, TODAY)[0]).toBeCloseTo(10, 1);
    expect(tenureDistribution(employees, TODAY).map((p) => p.value)).toEqual([
      1, 0, 0, 1, 0, 0,
    ]);
  });
});

describe('hireCounts', () => {
  it('counts hires per month and ignores missing hire dates', () => {
    const employees = [
      buildEmployee({ hireDate: '2020-02-10' }),
      buildEmployee({ hireDate: '2020-02-20' }),
      buildEmployee({ hireDate: null }),
    ];

    expect(hireCounts(employees)).toEqual([{ yearMonth: '2020-02', count: 2 }]);
  });
});

describe('headcountBy and payrollBy', () => {
  const employees = [
    buildEmployee({ departmentName: 'Engineering', currentGrossSalary: 9000 }),
    buildEmployee({ departmentName: 'Engineering', currentGrossSalary: 7000 }),
    buildEmployee({ departmentName: null, currentGrossSalary: 4000 }),
    buildEmployee({ departmentName: 'Sales', currentGrossSalary: null }),
  ];

  it('groups missing departments under "Unassigned"', () => {
    expect(
      headcountBy(employees, (e) => e.departmentName, 5, 'departments'),
    ).toEqual([
      { label: 'Engineering', value: 2 },
      { label: 'Sales', value: 1 },
      { label: 'Unassigned', value: 1 },
    ]);
  });

  it('sums salaries per group, skipping employees without one', () => {
    expect(
      payrollBy(employees, (e) => e.departmentName, 5, 'departments'),
    ).toEqual([
      { label: 'Engineering', value: 16000 },
      { label: 'Unassigned', value: 4000 },
    ]);
  });
});

describe('averagePayBy', () => {
  it('averages salaries per label, highest first', () => {
    const employees = [
      buildEmployee({ gender: 2, currentGrossSalary: 6000 }),
      buildEmployee({ gender: 2, currentGrossSalary: 8000 }),
      buildEmployee({ gender: 1, currentGrossSalary: 9000 }),
      buildEmployee({ gender: 1, currentGrossSalary: null }),
    ];

    expect(averagePayBy(employees, (e) => genderLabel(e.gender))).toEqual([
      { label: 'Male', value: 9000 },
      { label: 'Female', value: 7000 },
    ]);
  });
});

describe('payVsTenure', () => {
  it('plots salaried employees with a hire date, grouped by department', () => {
    const employees = [
      buildEmployee({ departmentName: null, currentGrossSalary: 5000 }),
      buildEmployee({ hireDate: null }),
      buildEmployee({ currentGrossSalary: null }),
    ];

    const points = payVsTenure(employees, TODAY);

    expect(points).toHaveLength(1);
    expect(points[0].y).toBe(5000);
    expect(points[0].group).toBe('Unassigned');
    expect(points[0].x).toBeCloseTo(10, 1);
  });
});
