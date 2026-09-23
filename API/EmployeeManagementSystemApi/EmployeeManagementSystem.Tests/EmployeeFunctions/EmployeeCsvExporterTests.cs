using EmployeeManagementSystem.BusinessLogic.EmployeeFunctions;
using EmployeeManagementSystem.Domain.Models;

namespace EmployeeManagementSystem.Tests.EmployeeFunctions;

public class EmployeeCsvExporterTests
{
    private static EmployeeModel MakeEmployee(Action<EmployeeModel>? configure = null)
    {
        var employee = new EmployeeModel
        {
            Guid = System.Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6"),
            FirstName = "Dan",
            LastName = "Frunza",
            Email = "dan@example.com",
            Msisdn = "123456789",
            Gender = Gender.Male,
            Birthdate = new DateOnly(1990, 1, 1),
            EmployeeStatus = EmployeeStatus.Active,
            CreationDate = new DateTime(2026, 1, 1, 9, 30, 0, DateTimeKind.Utc),
            InteractionDate = new DateTime(2026, 1, 2, 0, 0, 0, DateTimeKind.Utc),
            Address = new AddressModel
            {
                Country = "Romania",
                County = "Cluj",
                Town = "Cluj-Napoca",
                Zip = "400001",
                Street = "Main",
                Number = "1"
            }
        };
        configure?.Invoke(employee);
        return employee;
    }

    [Fact]
    public void ToCsv_StartsWithAUtf8BomFollowedByTheHeaderRow()
    {
        var csv = EmployeeCsvExporter.ToCsv([]);

        Assert.Equal((char)0xFEFF, csv[0]);
        Assert.StartsWith("Guid,First Name,Last Name,Email,MSISDN,Gender,Birthdate,Status,Creation Date,Interaction Date,Country,County,Town,Zip,Street,Number\r\n", csv[1..]);
    }

    [Fact]
    public void ToCsv_MapsGenderAndStatusCodesToLabels()
    {
        var csv = EmployeeCsvExporter.ToCsv([MakeEmployee(c =>
        {
            c.Gender = Gender.Female;
            c.EmployeeStatus = EmployeeStatus.Deactivated;
        })]);

        Assert.Contains(",female,", csv);
        Assert.Contains(",Deactivated,", csv);
    }

    [Fact]
    public void ToCsv_MapsTheTestStatusCodeToItsLabel()
    {
        var csv = EmployeeCsvExporter.ToCsv([MakeEmployee(c => c.EmployeeStatus = EmployeeStatus.Test)]);

        Assert.Contains(",Test,", csv);
    }

    [Theory]
    [InlineData((byte)3)]
    [InlineData(null)]
    public void ToCsv_LeavesGenderBlankForAnUnrecognizedOrMissingCode(byte? gender)
    {
        var csv = EmployeeCsvExporter.ToCsv([MakeEmployee(c => c.Gender = (Gender?)gender)]);

        var dataRow = csv.Split("\r\n")[1];
        Assert.Equal(string.Empty, dataRow.Split(',')[5]);
    }

    [Fact]
    public void ToCsv_WritesDatesAsIsoAndTimestampsAsIsoUtc()
    {
        var csv = EmployeeCsvExporter.ToCsv([MakeEmployee()]);

        var fields = csv.Split("\r\n")[1].Split(',');
        Assert.Equal("1990-01-01", fields[6]);
        Assert.Equal("2026-01-01T09:30:00Z", fields[8]);
        Assert.Equal("2026-01-02T00:00:00Z", fields[9]);
    }

    [Fact]
    public void ToCsv_QuotesAndEscapesAFieldContainingACommaOrQuote()
    {
        var csv = EmployeeCsvExporter.ToCsv([MakeEmployee(c => c.LastName = "Frunza, \"Dan\"")]);

        Assert.Contains("\"Frunza, \"\"Dan\"\"\"", csv);
    }

    [Fact]
    public void ToCsv_PrefixesAFieldStartingWithAFormulaCharacterToPreventSpreadsheetInjection()
    {
        var csv = EmployeeCsvExporter.ToCsv([MakeEmployee(c => c.LastName = "=cmd|'/c calc'!A0")]);

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
