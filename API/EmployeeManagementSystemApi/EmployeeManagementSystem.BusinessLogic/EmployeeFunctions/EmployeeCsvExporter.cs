using System.Globalization;
using System.Text;
using EmployeeManagementSystem.Domain.Models;

namespace EmployeeManagementSystem.BusinessLogic.EmployeeFunctions;

public static class EmployeeCsvExporter
{
    private const char Utf8Bom = (char)0xFEFF;

    private static readonly string[] Header =
    [
        "Employee ID", "First Name", "Last Name", "Email", "Phone Number", "Gender", "Birth Date", "Status",
        "Created At", "Last Interaction At", "Country", "County", "City", "Postal Code", "Street", "Street Number"
    ];

    public static string ToCsv(IEnumerable<EmployeeModel> employees)
    {
        var builder = new StringBuilder();
        builder.Append(Utf8Bom);
        builder.AppendJoin(',', Header.Select(EscapeField)).Append("\r\n");

        foreach (var employee in employees)
        {
            var fields = new[]
            {
                employee.EmployeeId.ToString(),
                employee.FirstName,
                employee.LastName,
                employee.Email,
                employee.PhoneNumber,
                GenderLabel(employee.Gender),
                employee.BirthDate?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                StatusLabel(employee.Status),
                employee.CreatedAt.ToString("u", CultureInfo.InvariantCulture),
                employee.LastInteractionAt.ToString("u", CultureInfo.InvariantCulture),
                employee.Address.Country,
                employee.Address.County,
                employee.Address.City,
                employee.Address.PostalCode,
                employee.Address.Street,
                employee.Address.StreetNumber
            };

            builder.AppendJoin(',', fields.Select(EscapeField)).Append("\r\n");
        }

        return builder.ToString();
    }

    private static string GenderLabel(Gender gender) => gender switch
    {
        Gender.NotDeclared => "not declared",
        Gender.Male => "male",
        Gender.Female => "female",
        _ => string.Empty
    };

    private static string StatusLabel(EmployeeStatus status) => status switch
    {
        EmployeeStatus.Active => "Active",
        EmployeeStatus.Deactivated => "Deactivated",
        EmployeeStatus.Test => "Test",
        _ => string.Empty
    };

    private static string EscapeField(string? value)
    {
        var field = value ?? string.Empty;

        if (field.Length > 0 && (field[0] is '=' or '+' or '-' or '@'))
            field = "'" + field;

        if (field.Contains(',') || field.Contains('"') || field.Contains('\n') || field.Contains('\r'))
            field = "\"" + field.Replace("\"", "\"\"") + "\"";

        return field;
    }
}
