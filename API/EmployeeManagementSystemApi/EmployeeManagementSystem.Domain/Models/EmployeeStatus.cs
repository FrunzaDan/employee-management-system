namespace EmployeeManagementSystem.Domain.Models;

// Employee.StatusCode (SMALLINT, CHECK-constrained to these values) — see
// ai_docs/database.md. Serialized as its number, so the JSON contract is unchanged.
public enum EmployeeStatus : short
{
    Active = 1901,
    Deactivated = 1903,
    Test = 1904
}
