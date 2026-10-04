using System.Text.Json.Serialization;

namespace EmployeeManagementSystem.Domain.Models;

public enum Gender : byte
{
    NotDeclared = 0,
    Male = 1,
    Female = 2
}

public enum EmployeeStatus : short
{
    Active = 1901,
    Deactivated = 1903,
    Test = 1904
}

public enum EmployerRole : short
{
    Employer = 1801
}

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
