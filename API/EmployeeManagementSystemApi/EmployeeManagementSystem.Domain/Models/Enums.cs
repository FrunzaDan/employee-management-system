using System.Text.Json.Serialization;

namespace EmployeeManagementSystem.Domain.Models;

// Employee.Gender (TINYINT, CK_Employee_Gender). Serialized as its number.
public enum Gender : byte
{
    NotDeclared = 0,
    Male = 1,
    Female = 2
}

// Employee.StatusCode (SMALLINT, CK_Employee_StatusCode) — see
// ai_docs/database.md. Serialized as its number, the documented status code.
public enum EmployeeStatus : short
{
    Active = 1901,
    Deactivated = 1903,
    Test = 1904
}

// Employer.RoleCode (SMALLINT). The number is also the JWT role claim value
// that [Authorize(Roles = "1801")] checks.
public enum EmployerRole : short
{
    Employer = 1801
}

// EmployeeAuditLog.ActionType (VARCHAR, CK_EmployeeAuditLog_ActionType). Stored and
// serialized by name, so the DB rows and the JSON stay human-readable.
[JsonConverter(typeof(JsonStringEnumConverter<AuditAction>))]
public enum AuditAction
{
    Created,
    Edited,
    Deactivated,
    Reactivated,
    Deleted,
    SalaryChanged
}

// Employee_List's @SortColumn/@SortDirection. Bound from the query string by name,
// case-insensitively ("name", "Email", ...).
public enum EmployeeSortColumn
{
    Name,
    Email,
    PhoneNumber
}

public enum SortDirection
{
    Asc,
    Desc
}
