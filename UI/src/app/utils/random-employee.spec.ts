import { Gender } from '../interfaces/employee';
import { buildRandomEmployee, randomGender } from './random-employee';

const yearsBetween = (from: string, to: string) =>
  (new Date(to).getTime() - new Date(from).getTime()) / (365.25 * 86_400_000);

describe('randomGender', () => {
  afterEach(() => vi.restoreAllMocks());

  it('gives 46% male, 46% female and 8% not declared', () => {
    const genderAt = (roll: number) => {
      vi.spyOn(Math, 'random').mockReturnValue(roll);
      return randomGender();
    };

    expect(genderAt(0)).toBe(Gender.Male);
    expect(genderAt(0.459)).toBe(Gender.Male);
    expect(genderAt(0.46)).toBe(Gender.Female);
    expect(genderAt(0.919)).toBe(Gender.Female);
    expect(genderAt(0.92)).toBe(Gender.NotDeclared);
    expect(genderAt(0.999)).toBe(Gender.NotDeclared);
  });

  it('declares a gender far more often than not', () => {
    const genders = Array.from({ length: 2000 }, () => randomGender());
    const count = (gender: Gender) =>
      genders.filter((g) => g === gender).length;

    expect(count(Gender.Male)).toBeGreaterThan(count(Gender.NotDeclared) * 3);
    expect(count(Gender.Female)).toBeGreaterThan(count(Gender.NotDeclared) * 3);
  });
});

describe('buildRandomEmployee', () => {
  const hireFrom = new Date(2005, 0, 1);
  const hireTo = new Date(2025, 11, 1);
  const build = (count: number) => {
    const taken = new Set<string>();
    return Array.from({ length: count }, () =>
      buildRandomEmployee(hireFrom, hireTo, taken),
    );
  };

  it('hires within the range, aged 19 to 56 on the hire date', () => {
    for (const employee of build(200)) {
      expect(employee.hireDate >= '2005-01-01').toBe(true);
      expect(employee.hireDate <= '2025-12-01').toBe(true);
      const age = yearsBetween(employee.birthDate, employee.hireDate);
      expect(age).toBeGreaterThanOrEqual(19 - 0.01);
      expect(age).toBeLessThan(56 + 0.01);
    }
  });

  it('never repeats an email or a phone number within a run', () => {
    const employees = build(200);

    expect(new Set(employees.map((e) => e.email)).size).toBe(200);
    expect(new Set(employees.map((e) => e.phoneNumber)).size).toBe(200);
  });

  it('writes emails and phone numbers the API accepts', () => {
    for (const employee of build(200)) {
      expect(employee.email).toMatch(/^[a-z0-9._-]+@[a-z.]+\.[a-z]+$/);
      expect(employee.phoneNumber).toMatch(/^07[2-9]\d{7}$/);
      expect(employee.address.postalCode).toMatch(/^\d{6}$/);
    }
  });

  it('varies the names instead of reusing a handful', () => {
    const employees = build(50);

    expect(new Set(employees.map((e) => e.firstName)).size).toBeGreaterThan(20);
    expect(new Set(employees.map((e) => e.address.city)).size).toBeGreaterThan(
      5,
    );
  });
});
