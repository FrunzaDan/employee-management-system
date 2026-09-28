import { EmployeeStatus, Gender } from '../../interfaces/employee';
import { EmployeeProfile } from '../../interfaces/employee-insights';
import { employeeStatusLabel } from '../../utils/employee-status-label';
import {
  AGE_BANDS,
  ChartPoint,
  countByMonth,
  countIntoBands,
  fractionalYearsBetween,
  LabelValue,
  MonthlyCount,
  rankTotals,
  TENURE_BANDS,
  totalsByLabel,
  wholeYearsBetween,
} from '../../utils/chart-stats';
import { ScatterPoint } from './scatter-chart/scatter-chart.component';

export const UNASSIGNED = 'Unassigned';

const STATUS_ORDER = [
  EmployeeStatus.Active,
  EmployeeStatus.Test,
  EmployeeStatus.Deactivated,
];

const GENDER_LABELS: ReadonlyArray<[Gender, string]> = [
  [Gender.Female, 'Female'],
  [Gender.Male, 'Male'],
  [Gender.NotDeclared, 'Not declared'],
];

type Salaried = EmployeeProfile & { currentGrossSalary: number };

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
