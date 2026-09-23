namespace EmployeeManagementSystem.Domain.Constants;

// Mirrors the column lengths declared in DB/.../Tables/*.sql. Lives in Domain (not
// BusinessLogic) because both layers need it: BusinessLogic rejects an over-length value with
// a clean 400 instead of an opaque SQL truncation error, and DataAccess sizes its
// SqlParameters to match the proc parameters.
public static class FieldLengthConstants
{
    // Mirrors Employee.sql / EmployeeAddress.sql.
    public const int FirstName = 100;
    public const int LastName = 100;
    public const int Email = 254;
    public const int PhoneNumber = 15;

    public const int Country = 100;
    public const int County = 100;
    public const int City = 100;
    public const int PostalCode = 20;
    public const int Street = 100;
    public const int StreetNumber = 50;

    // Mirrors Office.sql / Department.sql / CostCenter.sql.
    public const int OfficeName = 100;
    public const int DepartmentName = 100;
    public const int CostCenterCode = 50;
    public const int CostCenterName = 100;

    // Mirrors Employer.sql / EmployeeAuditLog.sql.
    public const int Username = 50;
    public const int AuditAction = 20;
    public const int AuditDetails = 500;

    // Mirrors Employee_List's parameters.
    public const int SearchTerm = 254;
    public const int SortColumn = 20;
    public const int SortDirection = 4;
}
