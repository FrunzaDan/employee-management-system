import { Component, computed, input, signal } from '@angular/core';
import { formatTick } from '../../../utils/chart-scale';
import { niceStep } from '../../../utils/chart-stats';
import { paletteColor } from '../donut-chart/donut-chart.component';

export interface ScatterPoint {
  x: number;
  y: number;
  group: string;
}

interface Dot {
  key: string;
  cx: number;
  cy: number;
  color: string;
  group: string;
  xText: string;
  yText: string;
}

interface LegendGroup {
  name: string;
  count: number;
  color: string;
  hidden: boolean;
}

const VIEW_WIDTH = 900;
const VIEW_HEIGHT = 340;
const TOP_PADDING = 20;
const RIGHT_PADDING = 20;
const BOTTOM_PADDING = 44;
const LEFT_PADDING = 52;
const PLOT_WIDTH = VIEW_WIDTH - LEFT_PADDING - RIGHT_PADDING;
const PLOT_HEIGHT = VIEW_HEIGHT - TOP_PADDING - BOTTOM_PADDING;
const TARGET_TICKS = 4;
const TOOLTIP_HEIGHT = 56;

// Axis from zero to the first round step at or above the largest value, so the
// dots use the whole plot instead of stopping halfway along a power of ten.
export function axisScale(maxValue: number): { max: number; ticks: number[] } {
  const step = niceStep(maxValue / TARGET_TICKS);
  const max = Math.max(Math.ceil(maxValue / step), 1) * step;
  const ticks: number[] = [];
  for (let tick = 0; tick <= max + step / 2; tick += step) ticks.push(tick);
  return { max, ticks };
}

// Least-squares line through the points; null when x never varies.
export function linearTrend(
  points: readonly { x: number; y: number }[],
): { slope: number; intercept: number } | null {
  const n = points.length;
  if (n < 2) return null;
  const meanX = points.reduce((s, p) => s + p.x, 0) / n;
  const meanY = points.reduce((s, p) => s + p.y, 0) / n;
  let covariance = 0;
  let variance = 0;
  for (const p of points) {
    covariance += (p.x - meanX) * (p.y - meanY);
    variance += (p.x - meanX) ** 2;
  }
  if (variance === 0) return null;
  const slope = covariance / variance;
  return { slope, intercept: meanY - slope * meanX };
}

@Component({
  selector: 'app-scatter-chart',
  templateUrl: './scatter-chart.component.html',
  styleUrl: './scatter-chart.component.css',
})
export class ScatterChartComponent {
  readonly points = input.required<ScatterPoint[]>();
  readonly xFormatter = input<(value: number) => string>((value) =>
    value.toLocaleString(),
  );
  readonly yFormatter = input<(value: number) => string>((value) =>
    value.toLocaleString(),
  );
  readonly xLabel = input('');
  readonly yLabel = input('');
  readonly ariaLabel = input('Scatter chart');
  readonly emptyMessage = input('No data yet.');

  readonly viewWidth = VIEW_WIDTH;
  readonly viewHeight = VIEW_HEIGHT;
  readonly leftPadding = LEFT_PADDING;
  readonly rightEdge = VIEW_WIDTH - RIGHT_PADDING;
  readonly topPadding = TOP_PADDING;
  readonly axisY = TOP_PADDING + PLOT_HEIGHT;
  readonly tooltipHeight = TOOLTIP_HEIGHT;

  readonly hiddenGroups = signal<ReadonlySet<string>>(new Set());
  readonly activeKey = signal<string | null>(null);

  readonly groups = computed<LegendGroup[]>(() => {
    const counts = new Map<string, number>();
    for (const point of this.points()) {
      counts.set(point.group, (counts.get(point.group) ?? 0) + 1);
    }
    const hidden = this.hiddenGroups();
    return [...counts.entries()]
      .sort((a, b) => b[1] - a[1] || a[0].localeCompare(b[0]))
      .map(([name, count], index) => ({
        name,
        count,
        color: paletteColor(index),
        hidden: hidden.has(name),
      }));
  });

  private readonly visiblePoints = computed(() => {
    const hidden = this.hiddenGroups();
    return this.points().filter((p) => !hidden.has(p.group));
  });

  private readonly xScale = computed(() =>
    axisScale(Math.max(0, ...this.points().map((p) => p.x))),
  );
  private readonly yScale = computed(() =>
    axisScale(Math.max(0, ...this.points().map((p) => p.y))),
  );

  private toX(value: number): number {
    return LEFT_PADDING + (value / this.xScale().max) * PLOT_WIDTH;
  }

  private toY(value: number): number {
    return this.axisY - (value / this.yScale().max) * PLOT_HEIGHT;
  }

  readonly xTicks = computed(() =>
    this.xScale().ticks.map((tick) => ({
      x: this.toX(tick),
      label: formatTick(tick),
    })),
  );

  readonly yTicks = computed(() =>
    this.yScale().ticks.map((tick) => ({
      y: this.toY(tick),
      label: formatTick(tick),
    })),
  );

  readonly dots = computed<Dot[]>(() => {
    const colors = new Map(this.groups().map((g) => [g.name, g.color]));
    const xFormat = this.xFormatter();
    const yFormat = this.yFormatter();
    return this.visiblePoints().map((point, index) => ({
      key: `${point.group}-${index}`,
      cx: this.toX(point.x),
      cy: this.toY(point.y),
      color: colors.get(point.group)!,
      group: point.group,
      xText: xFormat(point.x),
      yText: yFormat(point.y),
    }));
  });

  readonly trendLine = computed(() => {
    const trend = linearTrend(this.visiblePoints());
    if (!trend) return null;
    const xs = this.visiblePoints().map((p) => p.x);
    const from = Math.min(...xs);
    const to = Math.max(...xs);
    return {
      x1: this.toX(from),
      y1: this.toY(Math.max(trend.intercept + trend.slope * from, 0)),
      x2: this.toX(to),
      y2: this.toY(Math.max(trend.intercept + trend.slope * to, 0)),
      slope: trend.slope,
    };
  });

  readonly trendSummary = computed(() => {
    const trend = this.trendLine();
    if (!trend) return null;
    const perYear = this.yFormatter()(Math.abs(Math.round(trend.slope)));
    return trend.slope >= 0
      ? `Each extra year of service adds about ${perYear} on average.`
      : `Pay drops by about ${perYear} per year of service on average.`;
  });

  readonly tooltip = computed(() => {
    const key = this.activeKey();
    const dot = key ? this.dots().find((d) => d.key === key) : undefined;
    if (!dot) return null;
    const width =
      Math.max(dot.group.length, dot.yText.length, dot.xText.length) * 7 + 24;
    const x = Math.min(
      Math.max(dot.cx - width / 2, LEFT_PADDING),
      VIEW_WIDTH - RIGHT_PADDING - width,
    );
    const y =
      dot.cy - TOOLTIP_HEIGHT - 12 < 0
        ? dot.cy + 12
        : dot.cy - TOOLTIP_HEIGHT - 12;
    return { x, y, width, dot };
  });

  toggleGroup(name: string): void {
    this.hiddenGroups.update((hidden) => {
      const next = new Set(hidden);
      if (next.has(name)) next.delete(name);
      else next.add(name);
      return next;
    });
  }

  setActive(key: string | null): void {
    this.activeKey.set(key);
  }
}
