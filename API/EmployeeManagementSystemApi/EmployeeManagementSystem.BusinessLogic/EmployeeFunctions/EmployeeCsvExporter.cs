using System.Text;
using EmployeeManagementSystem.Domain.Models;

namespace EmployeeManagementSystem.BusinessLogic.EmployeeFunctions;

public static class EmployeeCsvExporter
{
    // UTF-8 BOM (U+FEFF), written via its code point rather than the invisible
    // literal glyph so it survives round-tripping through editors/encodings intact.
    private const char Utf8Bom = (char)0xFEFF;

    private static readonly string[] Header =
    [
        "Guid", "First Name", "Last Name", "Email", "MSISDN", "Gender", "Birthdate", "Status",
        "Creation Date", "Interaction Date", "Country", "County", "Town", "Zip", "Street", "Number"
    ];

    public static string ToCsv(IEnumerable<EmployeeModel> employees)
    {
        var builder = new StringBuilder();
        // Leading BOM so Excel opens the file as UTF-8 instead of guessing ANSI
        // and mangling non-ASCII names/addresses.
        builder.Append(Utf8Bom);
        builder.AppendJoin(',', Header.Select(EscapeField)).Append("\r\n");

        foreach (var employee in employees)
        {
            var fields = new[]
            {
                employee.Guid,
                employee.FirstName,
                employee.LastName,
                employee.Email,
                employee.Msisdn,
                GenderLabel(employee.Gender),
                employee.Birthdate,
                StatusLabel(employee.EmployeeStatus),
                employee.CreationDate,
                employee.InteractionDate,
                employee.Address?.Country,
                employee.Address?.County,
                employee.Address?.Town,
                employee.Address?.Zip,
                employee.Address?.Street,
                employee.Address?.Number
            };

            builder.AppendJoin(',', fields.Select(EscapeField)).Append("\r\n");
        }

        return builder.ToString();
    }

    private static string GenderLabel(int? gender) => gender switch
    {
        0 => "not declared",
        1 => "male",
        2 => "female",
        _ => string.Empty
    };

    // tbl_employees.employee_Status codes — see ai_docs/database.md.
    private static string StatusLabel(int? status) => status switch
    {
        1901 => "Active",
        1903 => "Deactivated",
        1904 => "Test",
        _ => string.Empty
    };

    // RFC 4180 quoting, plus a leading apostrophe on any field that starts with a
    // formula-trigger character (=, +, -, @) so a spreadsheet app never executes
    // employee-supplied data as a formula when the CSV is opened.
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
