import { EmployeeStatus, Gender } from '../../interfaces/employee';
import {
  EmployeeProfile,
  SalaryPoint,
} from '../../interfaces/employee-insights';
import { IsoDate } from '../../interfaces/iso-date';
import { employeeStatusLabel } from '../../utils/employee-status-label';
import {
  AGE_BANDS,
  ChartPoint,
  countByMonth,
  countIntoBands,
  fractionalYearsBetween,
  LabelValue,
  median,
  MonthlyCount,
  rankTotals,
  TENURE_BANDS,
  totalsByLabel,
  wholeYearsBetween,
  yearlyToPoints,
} from '../../utils/chart-stats';
import { ScatterPoint } from './scatter-chart/scatter-chart.component';

export const UNASSIGNED = 'Unassigned';

const STATUS_ORDER = [
  EmployeeStatus.Active,
  EmployeeStatus.Test,
  EmployeeStatus.Deactivated,
];

const GENDER_LABELS: readonly [Gender, string][] = [
  [Gender.Female, 'Female'],
  [Gender.Male, 'Male'],
  [Gender.NotDeclared, 'Not declared'],
];

// Growth over less than a year says little and annualises to wild numbers.
const MIN_GROWTH_YEARS = 1;

type Salaried = EmployeeProfile & { currentGrossSalary: number };

export interface Raise {
  effectiveDate: IsoDate;
  percent: number;
}

export function currentWorkforce(
  employees: EmployeeProfile[],
): EmployeeProfile[] {
  return employees.filter((e) => e.status !== EmployeeStatus.Deactivated);
}

export function salaried(employees: EmployeeProfile[]): Salaried[] {
  return employees.filter((e): e is Salaried => e.currentGrossSalary !== null);
}

export function statusSlices(employees: EmployeeProfile[]): LabelValue[] {
  return STATUS_ORDER.map((status) => ({
    label: employeeStatusLabel(status),
    value: employees.filter((e) => e.status === status).length,
  }));
}

export function genderSlices(employees: EmployeeProfile[]): LabelValue[] {
  return GENDER_LABELS.map(([gender, label]) => ({
    label,
    value: employees.filter((e) => e.gender === gender).length,
  }));
}

export function ageDistribution(
  employees: EmployeeProfile[],
  today: Date,
): ChartPoint[] {
  const ages = employees
    .filter((e) => e.birthDate !== null)
    .map((e) => wholeYearsBetween(e.birthDate!, today));
  return countIntoBands(ages, AGE_BANDS);
}

export function tenureYears(
  employees: EmployeeProfile[],
  today: Date,
): number[] {
  return employees
    .filter((e) => e.hireDate !== null)
    .map((e) => fractionalYearsBetween(e.hireDate!, today));
}

export function tenureDistribution(
  employees: EmployeeProfile[],
  today: Date,
): ChartPoint[] {
  const years = employees
    .filter((e) => e.hireDate !== null)
    .map((e) => wholeYearsBetween(e.hireDate!, today));
  return countIntoBands(years, TENURE_BANDS);
}

export function hireCounts(employees: EmployeeProfile[]): MonthlyCount[] {
  return countByMonth(
    employees.filter((e) => e.hireDate !== null).map((e) => e.hireDate!),
  );
}

export function headcountBy(
  employees: EmployeeProfile[],
  labelFn: (employee: EmployeeProfile) => string | null,
  limit: number,
  otherNoun: string,
): LabelValue[] {
  return rankTotals(
    totalsByLabel(employees, (e) => labelFn(e) ?? UNASSIGNED),
    limit,
    otherNoun,
  );
}

export function payrollBy(
  employees: EmployeeProfile[],
  labelFn: (employee: EmployeeProfile) => string | null,
  limit: number,
  otherNoun: string,
): LabelValue[] {
  return rankTotals(
    totalsByLabel(
      salaried(employees),
      (e) => labelFn(e) ?? UNASSIGNED,
      (e) => e.currentGrossSalary,
    ),
    limit,
    otherNoun,
  );
}

export function averagePayBy(
  employees: EmployeeProfile[],
  labelFn: (employee: EmployeeProfile) => string,
): LabelValue[] {
  const paid = salaried(employees);
  const totals = totalsByLabel(paid, labelFn, (e) => e.currentGrossSalary);
  const counts = totalsByLabel(paid, labelFn);
  return [...totals.entries()]
    .map(([label, total]) => ({
      label,
      value: Math.round(total / counts.get(label)!),
    }))
    .sort((a, b) => b.value - a.value || a.label.localeCompare(b.label));
}

export function genderLabel(gender: Gender): string {
  return GENDER_LABELS.find(([g]) => g === gender)?.[1] ?? 'Not declared';
}

export function payVsTenure(
  employees: EmployeeProfile[],
  today: Date,
): ScatterPoint[] {
  return salaried(employees)
    .filter((e) => e.hireDate !== null)
    .map((e) => ({
      x: fractionalYearsBetween(e.hireDate!, today),
      y: e.currentGrossSalary,
      group: e.departmentName ?? UNASSIGNED,
    }));
}

export function raises(employees: EmployeeProfile[]): Raise[] {
  return employees.flatMap(({ salaryHistory }) =>
    salaryHistory
      .slice(1)
      .map((entry, index) => ({
        effectiveDate: entry.effectiveDate,
        percent:
          (entry.grossSalary / salaryHistory[index].grossSalary - 1) * 100,
      }))
      .filter((raise) => raise.percent > 0),
  );
}

export function raisesPerYear(employees: EmployeeProfile[]): ChartPoint[] {
  return yearlyToPoints(
    countByMonth(raises(employees).map((raise) => raise.effectiveDate)),
  );
}

function salaryOn(history: SalaryPoint[], isoDate: IsoDate): number | null {
  let salary: number | null = null;
  for (const entry of history) {
    if (entry.effectiveDate > isoDate) break;
    salary = entry.grossSalary;
  }
  return salary;
}

// The median of the salaries in effect at the end of each year, from the
// first salary on record to this year.
export function medianSalaryByYear(
  employees: EmployeeProfile[],
  today: Date,
): ChartPoint[] {
  const firstDates = employees
    .filter((e) => e.salaryHistory.length > 0)
    .map((e) => e.salaryHistory[0].effectiveDate);
  if (firstDates.length === 0) return [];

  const firstYear = Math.min(...firstDates.map((d) => Number(d.slice(0, 4))));
  const points: ChartPoint[] = [];
  for (let year = firstYear; year <= today.getFullYear(); year++) {
    const salaries = employees
      .map((e) => salaryOn(e.salaryHistory, `${year}-12-31`))
      .filter((salary) => salary !== null);
    const key = year.toString();
    points.push({ key, label: key, value: Math.round(median(salaries) ?? 0) });
  }
  return points;
}

// Compound yearly growth from the first salary to today's, so a raise long
// ago counts for less than a recent one. Employees never raised count as 0%.
export function annualPayGrowth(
  history: SalaryPoint[],
  today: Date,
): number | null {
  if (history.length === 0) return null;
  const years = fractionalYearsBetween(history[0].effectiveDate, today);
  if (years < MIN_GROWTH_YEARS) return null;
  const ratio =
    history[history.length - 1].grossSalary / history[0].grossSalary;
  return (Math.pow(ratio, 1 / years) - 1) * 100;
}

export function payGrowthBy(
  employees: EmployeeProfile[],
  labelFn: (employee: EmployeeProfile) => string,
  today: Date,
): LabelValue[] {
  const growth = employees
    .map((e) => ({ e, growth: annualPayGrowth(e.salaryHistory, today) }))
    .filter(
      (row): row is { e: EmployeeProfile; growth: number } =>
        row.growth !== null,
    );
  const totals = totalsByLabel(
    growth,
    (row) => labelFn(row.e),
    (row) => row.growth,
  );
  const counts = totalsByLabel(growth, (row) => labelFn(row.e));
  return [...totals.entries()]
    .map(([label, total]) => ({
      label,
      value: Math.round((total / counts.get(label)!) * 10) / 10,
    }))
    .sort((a, b) => b.value - a.value || a.label.localeCompare(b.label));
}
