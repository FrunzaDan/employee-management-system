import { EmployeeProfile } from '../../interfaces/employee-insights';
import {
  ageDistribution,
  averagePayBy,
  currentWorkforce,
  genderLabel,
  genderSlices,
  headcountBy,
  hireCounts,
  annualPayGrowth,
  medianSalaryByYear,
  payGrowthBy,
  payrollBy,
  payVsTenure,
  raises,
  raisesPerYear,
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
  salaryHistory: [],
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

const history = (...entries: [string, number][]) =>
  entries.map(([effectiveDate, grossSalary]) => ({
    effectiveDate,
    grossSalary,
  }));

describe('raises and raisesPerYear', () => {
  it('counts each salary increase once, by the date it took effect', () => {
    const employees = [
      buildEmployee({
        salaryHistory: history(
          ['2020-01-01', 4000],
          ['2021-03-01', 5000],
          ['2023-06-01', 4500],
          ['2023-09-01', 4950],
        ),
      }),
      buildEmployee({ salaryHistory: history(['2021-05-01', 6000]) }),
    ];

    expect(raises(employees)).toEqual([
      { effectiveDate: '2021-03-01', percent: 25 },
      { effectiveDate: '2023-09-01', percent: expect.closeTo(10, 5) },
    ]);
    expect(raisesPerYear(employees).map((p) => [p.label, p.value])).toEqual([
      ['2021', 1],
      ['2022', 0],
      ['2023', 1],
    ]);
  });
});

describe('medianSalaryByYear', () => {
  it('takes the median of the salaries in effect at the end of each year', () => {
    const employees = [
      buildEmployee({
        salaryHistory: history(['2024-02-01', 4000], ['2025-12-31', 5000]),
      }),
      buildEmployee({ salaryHistory: history(['2025-06-01', 7000]) }),
      buildEmployee({ salaryHistory: [] }),
    ];

    expect(
      medianSalaryByYear(employees, TODAY).map((p) => [p.label, p.value]),
    ).toEqual([
      ['2024', 4000],
      ['2025', 6000],
      ['2026', 6000],
    ]);
  });

  it('is empty without any salary history', () => {
    expect(medianSalaryByYear([buildEmployee()], TODAY)).toEqual([]);
  });
});

describe('annualPayGrowth and payGrowthBy', () => {
  it('compounds growth from the first salary to today', () => {
    expect(
      annualPayGrowth(
        history(['2024-09-28', 4000], ['2025-01-01', 4840]),
        TODAY,
      ),
    ).toBeCloseTo(10, 1);
  });

  it('counts a never-raised employee as 0% and skips anyone here under a year', () => {
    expect(annualPayGrowth(history(['2020-01-01', 5000]), TODAY)).toBe(0);
    expect(annualPayGrowth(history(['2026-01-01', 5000]), TODAY)).toBeNull();
    expect(annualPayGrowth([], TODAY)).toBeNull();
  });

  it('averages the growth per group, fastest first', () => {
    const employees = [
      buildEmployee({
        departmentName: 'Sales',
        salaryHistory: history(['2024-09-28', 4000], ['2025-01-01', 4840]),
      }),
      buildEmployee({
        departmentName: 'Sales',
        salaryHistory: history(['2020-01-01', 5000]),
      }),
      buildEmployee({
        departmentName: 'Engineering',
        salaryHistory: history(['2024-09-28', 4000], ['2025-01-01', 4400]),
      }),
      buildEmployee({
        departmentName: 'HR',
        salaryHistory: history(['2026-06-01', 4000]),
      }),
    ];

    expect(payGrowthBy(employees, (e) => e.departmentName!, TODAY)).toEqual([
      { label: 'Sales', value: 5 },
      { label: 'Engineering', value: 4.9 },
    ]);
  });
});
