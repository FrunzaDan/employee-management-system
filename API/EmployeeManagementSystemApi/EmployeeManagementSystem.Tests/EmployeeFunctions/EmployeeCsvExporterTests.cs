using EmployeeManagementSystem.BusinessLogic.EmployeeFunctions;
using EmployeeManagementSystem.Domain.Models;

namespace EmployeeManagementSystem.Tests.EmployeeFunctions;

public class EmployeeCsvExporterTests
{
    private static EmployeeModel MakeEmployee(Func<EmployeeModel, EmployeeModel>? configure = null)
    {
        var employee = new EmployeeModel
        {
            EmployeeId = Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6"),
            FirstName = "Dan",
            LastName = "Frunza",
            Email = "dan@example.com",
            PhoneNumber = "123456789",
            Gender = Gender.Male,
            BirthDate = new DateOnly(1990, 1, 1),
            Status = EmployeeStatus.Active,
            CreatedAt = new DateTime(2026, 1, 1, 8, 30, 0, DateTimeKind.Utc),
            LastInteractionAt = new DateTime(2026, 1, 2, 9, 45, 15, DateTimeKind.Utc),
            Address = new AddressModel
            {
                Country = "Romania",
                County = "Cluj",
                City = "Cluj-Napoca",
                PostalCode = "400001",
                Street = "Main",
                StreetNumber = "1"
            }
        };
        return configure is null ? employee : configure(employee);
    }

    [Fact]
    public void ToCsv_StartsWithAUtf8BomFollowedByTheHeaderRow()
    {
        var csv = EmployeeCsvExporter.ToCsv([]);

        Assert.Equal((char)0xFEFF, csv[0]);
        Assert.StartsWith("Employee ID,First Name,Last Name,Email,Phone Number,Gender,Birth Date,Status,Created At,Last Interaction At,Country,County,City,Postal Code,Street,Street Number\r\n", csv[1..]);
    }

    [Fact]
    public void ToCsv_MapsGenderAndStatusCodesToLabels()
    {
        var csv = EmployeeCsvExporter.ToCsv([MakeEmployee(c =>
            c with { Gender = Gender.Female, Status = EmployeeStatus.Deactivated })]);

        Assert.Contains(",female,", csv);
        Assert.Contains(",Deactivated,", csv);
    }

    [Fact]
    public void ToCsv_MapsTheTestStatusCodeToItsLabel()
    {
        var csv = EmployeeCsvExporter.ToCsv([MakeEmployee(c => c with { Status = EmployeeStatus.Test })]);

        Assert.Contains(",Test,", csv);
    }

    [Fact]
    public void ToCsv_LeavesGenderBlankForAnUnrecognizedCode()
    {
        var csv = EmployeeCsvExporter.ToCsv([MakeEmployee(c => c with { Gender = (Gender)3 })]);

        var dataRow = csv.Split("\r\n")[1];
        Assert.Equal(string.Empty, dataRow.Split(',')[5]);
    }

    [Fact]
    public void ToCsv_WritesDatesAsIso8601_WithTimestampsMarkedUtc()
    {
        var dataRow = EmployeeCsvExporter.ToCsv([MakeEmployee()]).Split("\r\n")[1].Split(',');

        Assert.Equal("1990-01-01", dataRow[6]);
        Assert.Equal("2026-01-01 08:30:00Z", dataRow[8]);
        Assert.Equal("2026-01-02 09:45:15Z", dataRow[9]);
    }

    [Fact]
    public void ToCsv_LeavesBirthdateBlank_WhenItIsNotSet()
    {
        var dataRow = EmployeeCsvExporter.ToCsv([MakeEmployee(c => c with { BirthDate = null })]).Split("\r\n")[1];

        Assert.Equal(string.Empty, dataRow.Split(',')[6]);
    }

    [Fact]
    public void ToCsv_QuotesAndEscapesAFieldContainingACommaOrQuote()
    {
        var csv = EmployeeCsvExporter.ToCsv([MakeEmployee(c => c with { LastName = "Frunza, \"Dan\"" })]);

        Assert.Contains("\"Frunza, \"\"Dan\"\"\"", csv);
    }

    [Fact]
    public void ToCsv_PrefixesAFieldStartingWithAFormulaCharacterToPreventSpreadsheetInjection()
    {
        var csv = EmployeeCsvExporter.ToCsv([MakeEmployee(c => c with { LastName = "=cmd|'/c calc'!A0" })]);

        Assert.Contains(",'=cmd|'/c calc'!A0,", csv);
    }

    [Fact]
    public void ToCsv_WritesOneRowPerEmployee()
    {
        var csv = EmployeeCsvExporter.ToCsv([MakeEmployee(), MakeEmployee()]);

        // header + 2 data rows + trailing blank line from the last row's \r\n
        Assert.Equal(4, csv.Split("\r\n").Length);
    }
}
