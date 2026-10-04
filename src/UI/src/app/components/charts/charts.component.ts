import { Component, computed, inject, signal } from '@angular/core';
import { EmployeeInsightsService } from '../../services/employee-insights.service';
import { RankedBarChartComponent } from './ranked-bar-chart/ranked-bar-chart.component';
import { TimeSeriesChartComponent } from './time-series-chart/time-series-chart.component';
import { DonutChartComponent } from './donut-chart/donut-chart.component';
import { KpiTileComponent } from './kpi-tile/kpi-tile.component';
import { ScatterChartComponent } from './scatter-chart/scatter-chart.component';
import { RonPipe } from '../../pipes/ron.pipe';
import {
  average,
  ChartPoint,
  cumulativeYearlyPoints,
  histogram,
  median,
  monthlyToPoints,
  wholeYearsBetween,
  yearlyToPoints,
} from '../../utils/chart-stats';
import {
  ageDistribution,
  averagePayBy,
  currentWorkforce,
  genderLabel,
  genderSlices,
  headcountBy,
  hireCounts,
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

const TOP_GROUP_COUNT = 8;
const PAYROLL_SLICE_COUNT = 5;
const GROWTH_LOOKBACK_YEARS = 5;

export type TimeRange = 'monthly' | 'yearly';

function peakOf(points: ChartPoint[]): ChartPoint | null {
  return points.reduce<ChartPoint | null>(
    (best, point) =>
      point.value > 0 && (!best || point.value > best.value) ? point : best,
    null,
  );
}

@Component({
  selector: 'app-charts',
  templateUrl: './charts.component.html',
  styleUrl: './charts.component.css',
  imports: [
    DonutChartComponent,
    KpiTileComponent,
    RankedBarChartComponent,
    ScatterChartComponent,
    TimeSeriesChartComponent,
  ],
  providers: [RonPipe],
})
export class ChartsComponent {
  private readonly insightsService = inject(EmployeeInsightsService);
  private readonly ron = inject(RonPipe);
  private readonly today = new Date();

  readonly loading = this.insightsService.loading;
  readonly loadError = this.insightsService.error;

  readonly employees = computed(
    () => this.insightsService.insights().employees,
  );
  readonly workforce = computed(() => currentWorkforce(this.employees()));
  private readonly salaries = computed(() =>
    salaried(this.workforce()).map((e) => e.currentGrossSalary),
  );

  // KPIs
  readonly headcount = computed(() => this.workforce().length);
  readonly headcountCaption = computed(() => {
    const left = this.employees().length - this.headcount();
    return left > 0 ? `${left} deactivated not counted` : 'current employees';
  });
  readonly monthlyPayroll = computed(() =>
    this.salaries().reduce((sum, salary) => sum + salary, 0),
  );
  readonly payrollCaption = computed(() => {
    const missing = this.headcount() - this.salaries().length;
    return missing > 0
      ? `gross; ${missing} without a salary`
      : 'gross, current salaries';
  });
  readonly medianSalary = computed(() => median(this.salaries()));
  readonly averageSalaryCaption = computed(() => {
    const mean = average(this.salaries());
    return mean === null ? null : `average ${this.formatRon(mean)}`;
  });
  readonly averageTenure = computed(() =>
    average(tenureYears(this.workforce(), this.today)),
  );
  readonly tenureCaption = computed(() => {
    const middle = median(tenureYears(this.workforce(), this.today));
    return middle === null ? null : `median ${middle.toFixed(1)} yrs`;
  });
  readonly averageAge = computed(() =>
    average(
      this.workforce()
        .filter((e) => e.birthDate !== null)
        .map((e) => wholeYearsBetween(e.birthDate!, this.today)),
    ),
  );

  readonly headcountTrend = computed(() =>
    this.headcountGrowthPoints().map((p) => p.value),
  );

  // Workforce
  private readonly hires = computed(() => hireCounts(this.employees()));

  readonly hiresRange = signal<TimeRange>('yearly');
  readonly hiresPoints = computed(() =>
    this.hiresRange() === 'monthly'
      ? monthlyToPoints(this.hires())
      : yearlyToPoints(this.hires()),
  );
  readonly busiestHiringYear = computed(() =>
    peakOf(yearlyToPoints(this.hires())),
  );
  readonly headcountGrowthPoints = computed(() =>
    cumulativeYearlyPoints(hireCounts(this.workforce())),
  );

  readonly statusSlices = computed(() => statusSlices(this.employees()));
  readonly genderSlices = computed(() => genderSlices(this.workforce()));
  readonly ageDistribution = computed(() =>
    ageDistribution(this.workforce(), this.today),
  );
  readonly largestAgeGroup = computed(() => peakOf(this.ageDistribution()));
  readonly tenureDistribution = computed(() =>
    tenureDistribution(this.workforce(), this.today),
  );
  readonly veteranShare = computed(() => {
    const withHireDate = this.tenureDistribution().reduce(
      (sum, band) => sum + band.value,
      0,
    );
    if (withHireDate === 0) return null;
    const veterans = this.tenureDistribution()
      .slice(3)
      .reduce((sum, band) => sum + band.value, 0);
    return Math.round((veterans / withHireDate) * 100);
  });
  readonly headcountByDepartment = computed(() =>
    headcountBy(
      this.workforce(),
      (e) => e.departmentName,
      TOP_GROUP_COUNT,
      'departments',
    ),
  );
  readonly headcountByOffice = computed(() =>
    headcountBy(
      this.workforce(),
      (e) => e.officeName,
      TOP_GROUP_COUNT,
      'offices',
    ),
  );

  // Pay
  readonly salaryDistribution = computed(() => histogram(this.salaries()));
  readonly payrollByDepartment = computed(() =>
    payrollBy(
      this.workforce(),
      (e) => e.departmentName,
      PAYROLL_SLICE_COUNT,
      'departments',
    ),
  );
  readonly averagePayByDepartment = computed(() =>
    averagePayBy(this.workforce(), (e) => e.departmentName ?? 'Unassigned'),
  );
  readonly averagePayByGender = computed(() =>
    averagePayBy(this.workforce(), (e) => genderLabel(e.gender)),
  );
  readonly genderPayGap = computed(() => {
    const byGender = new Map(
      this.averagePayByGender().map((row) => [row.label, row.value]),
    );
    const female = byGender.get('Female');
    const male = byGender.get('Male');
    if (!female || !male) return null;
    const gap = Math.round(((male - female) / male) * 100);
    if (gap === 0) return 'Women and men are paid the same on average.';
    return gap > 0
      ? `Women earn ${gap}% less than men on average.`
      : `Women earn ${-gap}% more than men on average.`;
  });
  readonly payVsTenure = computed(() =>
    payVsTenure(this.workforce(), this.today),
  );

  // Salary growth
  readonly medianSalaryByYear = computed(() =>
    medianSalaryByYear(this.workforce(), this.today),
  );
  readonly medianSalaryTrend = computed(() =>
    this.medianSalaryByYear().map((p) => p.value),
  );
  readonly medianSalaryGrowth = computed(() => {
    const points = this.medianSalaryByYear();
    if (points.length < 2) return null;
    const from = points[Math.max(points.length - 1 - GROWTH_LOOKBACK_YEARS, 0)];
    const to = points[points.length - 1];
    if (from.value === 0) return null;
    const change = Math.round((to.value / from.value - 1) * 100);
    return { change, since: from.label, up: change >= 0 };
  });
  private readonly allRaises = computed(() => raises(this.employees()));
  readonly medianRaise = computed(() =>
    median(this.allRaises().map((raise) => raise.percent)),
  );
  readonly medianRaiseCaption = computed(() => {
    const yearAgo = new Date(this.today);
    yearAgo.setFullYear(yearAgo.getFullYear() - 1);
    const since = yearAgo.toISOString().slice(0, 10);
    const recent = this.allRaises().filter(
      (raise) => raise.effectiveDate > since,
    ).length;
    return `${recent} in the past year`;
  });
  readonly raisesPerYear = computed(() => raisesPerYear(this.employees()));
  readonly busiestRaiseYear = computed(() => peakOf(this.raisesPerYear()));
  readonly payGrowthByDepartment = computed(() =>
    payGrowthBy(
      this.workforce(),
      (e) => e.departmentName ?? 'Unassigned',
      this.today,
    ),
  );

  readonly formatRon = (value: number): string =>
    this.ron.transform(value, '1.0-0');
  readonly formatYears = (value: number): string => `${value.toFixed(1)} yrs`;
  readonly formatAge = (value: number): string => `${Math.round(value)}`;
  readonly formatPercent = (value: number): string => `${value.toFixed(1)}%`;

  constructor() {
    this.insightsService.loadInsights();
  }

  setHiresRange(range: TimeRange): void {
    this.hiresRange.set(range);
  }
}
