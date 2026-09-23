namespace EmployeeManagementSystem.Domain.Constants;

// Mirrors the (N)VARCHAR lengths declared in DB/.../Tables/*.sql. Lives in Domain so both
// sides use one source: BusinessLogic rejects an over-length value with a clean 400 instead
// of an opaque SQL truncation error, and DataAccess sizes each SqlParameter to its column.
public static class FieldLengthConstants
{
    public const int FirstName = 50;
    public const int LastName = 50;
    public const int Email = 254;
    public const int Msisdn = 15;

    public const int Country = 100;
    public const int County = 100;
    public const int Town = 100;
    public const int Zip = 20;
    public const int Street = 100;
    public const int Number = 50;

    public const int OfficeName = 100;
    public const int City = 100;

    public const int DepartmentName = 100;

    public const int CostCenterCode = 50;
    public const int CostCenterName = 100;

    public const int EmployerId = 50;
    public const int AuditAction = 50;
    public const int AuditDetails = 500;
    public const int SearchTerm = 200;
}
