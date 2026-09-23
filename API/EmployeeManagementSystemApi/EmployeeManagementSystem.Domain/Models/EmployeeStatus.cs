namespace EmployeeManagementSystem.Domain.Models;

// tbl_employees.employee_Status (SMALLINT, CHECK-constrained to these values) — see
// ai_docs/database.md. Serialized as its number, so the JSON contract is unchanged.
public enum EmployeeStatus : short
{
    Active = 1901,
    Deactivated = 1903,
    Test = 1904
}
