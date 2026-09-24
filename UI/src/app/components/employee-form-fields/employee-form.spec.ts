import { emptyEmployeeForm, isEmployeeFormDirty } from './employee-form';
import { environment } from '../../../environments/environment';

describe('isEmployeeFormDirty', () => {
  it('is false when nothing differs from the baseline', () => {
    expect(isEmployeeFormDirty(emptyEmployeeForm(), emptyEmployeeForm())).toBe(
      false,
    );
  });

  it('is true when any single field differs', () => {
    const changed = { ...emptyEmployeeForm(), postalCode: '400000' };

    expect(isEmployeeFormDirty(changed, emptyEmployeeForm())).toBe(true);
  });

  it('is false again once a changed value is put back', () => {
    const baseline = { ...emptyEmployeeForm(), firstName: 'Dan' };
    const edited = { ...baseline, firstName: 'Daniel' };
    const reverted = { ...edited, firstName: 'Dan' };

    expect(isEmployeeFormDirty(edited, baseline)).toBe(true);
    expect(isEmployeeFormDirty(reverted, baseline)).toBe(false);
  });
});

describe('environment.emailRegex', () => {
  const emailRegex = new RegExp(environment.emailRegex);

  it('accepts a plain address', () => {
    expect(emailRegex.test('ana.pop@example.com')).toBe(true);
  });

  it.each(['a@b@c.com', 'ana pop@example.com', 'ana@example', '@example.com'])(
    'rejects %s',
    (email) => {
      expect(emailRegex.test(email)).toBe(false);
    },
  );
});
