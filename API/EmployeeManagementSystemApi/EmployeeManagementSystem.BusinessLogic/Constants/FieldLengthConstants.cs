namespace EmployeeManagementSystem.BusinessLogic.Constants;

// Mirrors the NVARCHAR lengths declared in DB/.../Tables/tbl_employees.sql and
// tbl_addresses.sql, so an over-length value is rejected here with a clean 400
// instead of surfacing as an opaque SQL truncation/conversion error.
public static class FieldLengthConstants
{
    public const int FirstName = 50;
    public const int LastName = 50;
    public const int Email = 50;
    public const int Msisdn = 50;
    public const int Birthdate = 50;

    public const int Country = 100;
    public const int County = 100;
    public const int Town = 50;
    public const int Zip = 50;
    public const int Street = 100;
    public const int Number = 50;

    public const int HireDate = 50;

    public const int OfficeName = 100;
    public const int City = 50;

    public const int DepartmentName = 100;

    public const int CostCenterCode = 50;
    public const int CostCenterName = 100;
}
