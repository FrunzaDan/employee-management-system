import { emptyEmployeeForm, isEmployeeFormDirty } from './employee-form';

describe('isEmployeeFormDirty', () => {
  it('is false when nothing differs from the baseline', () => {
    expect(isEmployeeFormDirty(emptyEmployeeForm(), emptyEmployeeForm())).toBe(false);
  });

  it('is true when any single field differs', () => {
    const changed = { ...emptyEmployeeForm(), zip: '400000' };

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
