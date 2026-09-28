import { TestBed } from '@angular/core/testing';
import {
  axisScale,
  linearTrend,
  ScatterChartComponent,
  ScatterPoint,
} from './scatter-chart.component';

describe('axisScale', () => {
  it('ends at the first round step above the largest value', () => {
    expect(axisScale(21)).toEqual({ max: 25, ticks: [0, 5, 10, 15, 20, 25] });
    expect(axisScale(12800).max).toBe(15000);
  });

  it('still draws an axis when every value is zero', () => {
    expect(axisScale(0)).toEqual({ max: 1, ticks: [0, 1] });
  });
});

describe('linearTrend', () => {
  it('fits a straight line through the points', () => {
    const trend = linearTrend([
      { x: 0, y: 1000 },
      { x: 1, y: 1200 },
      { x: 2, y: 1400 },
    ]);

    expect(trend!.slope).toBeCloseTo(200);
    expect(trend!.intercept).toBeCloseTo(1000);
  });

  it('returns null when there is no spread in x', () => {
    expect(linearTrend([{ x: 1, y: 1 }])).toBeNull();
    expect(
      linearTrend([
        { x: 3, y: 1 },
        { x: 3, y: 9 },
      ]),
    ).toBeNull();
  });
});

describe('ScatterChartComponent', () => {
  const points: ScatterPoint[] = [
    { x: 1, y: 4000, group: 'Sales' },
    { x: 5, y: 8000, group: 'Engineering' },
    { x: 9, y: 12000, group: 'Engineering' },
  ];

  const create = () => {
    const fixture = TestBed.createComponent(ScatterChartComponent);
    fixture.componentRef.setInput('points', points);
    fixture.detectChanges();
    return fixture;
  };

  it('orders the legend by group size and draws one dot per point', () => {
    const fixture = create();

    expect(fixture.componentInstance.groups().map((g) => g.name)).toEqual([
      'Engineering',
      'Sales',
    ]);
    expect(fixture.nativeElement.querySelectorAll('circle.dot')).toHaveLength(
      3,
    );
  });

  it('hides a group when its legend chip is toggled, and shows it again', () => {
    const fixture = create();
    const component = fixture.componentInstance;

    component.toggleGroup('Engineering');
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelectorAll('circle.dot')).toHaveLength(
      1,
    );
    expect(component.trendLine()).toBeNull();

    component.toggleGroup('Engineering');
    fixture.detectChanges();
    expect(fixture.nativeElement.querySelectorAll('circle.dot')).toHaveLength(
      3,
    );
  });

  it('summarises the trend in words', () => {
    const component = create().componentInstance;

    expect(component.trendSummary()).toBe(
      'Each extra year of service adds about 1,000 on average.',
    );
  });
});
