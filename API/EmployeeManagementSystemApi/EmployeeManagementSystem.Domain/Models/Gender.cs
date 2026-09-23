namespace EmployeeManagementSystem.Domain.Models;

// Employee.Gender (TINYINT, CHECK-constrained to these values). Serialized as its
// number, so the JSON contract is unchanged.
public enum Gender : byte
{
    NotDeclared = 0,
    Male = 1,
    Female = 2
}
