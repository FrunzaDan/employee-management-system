using EmployeeManagementSystem.BusinessLogic.Abstractions;
using EmployeeManagementSystem.BusinessLogic.Contracts;
using EmployeeManagementSystem.BusinessLogic.Features.Employees;
using EmployeeManagementSystem.Domain.Models;
using Moq;

namespace EmployeeManagementSystem.Tests.Features.Employees;

public class GetEmployeeHandlerTests
{
    private static readonly Guid EmployeeId = Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6");

    private static EmployeeModel MakeEmployee() => new()
    {
        EmployeeId = EmployeeId,
        FirstName = "Dan",
        LastName = "Frunza",
        Email = "dan@example.com",
        PhoneNumber = "123456789",
        Gender = Gender.Male,
        Status = EmployeeStatus.Active,
        AccountCreatedAt = DateTime.UtcNow,
        LastInteractionAt = DateTime.UtcNow,
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

    private static async Task<EmployeeLookup?> CaptureLookup(string searchTerm)
    {
        var employees = new Mock<IEmployeeRepository>();
        EmployeeLookup? captured = null;
        employees.Setup(d => d.GetEmployeeAsync(It.IsAny<EmployeeLookup>(), It.IsAny<CancellationToken>()))
            .Callback<EmployeeLookup, CancellationToken>((l, _) => captured = l)
            .ReturnsAsync(new ResponseModel<EmployeeModel>(200, "Employee found.", MakeEmployee()));
        var handler = new GetEmployeeHandler(employees.Object);

        await handler.HandleAsync(searchTerm, TestContext.Current.CancellationToken);

        return captured;
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task GetEmployeeAsync_RejectsAnEmptySearchVariable_WithoutTouchingTheDb(string? searchTerm)
    {
        var employees = new Mock<IEmployeeRepository>();
        var handler = new GetEmployeeHandler(employees.Object);

        var result = await handler.HandleAsync(searchTerm, TestContext.Current.CancellationToken);

        Assert.Equal(400, result.Status);
        employees.Verify(d => d.GetEmployeeAsync(It.IsAny<EmployeeLookup>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetEmployeeAsync_RejectsASearchVariableThatIsNeitherIdPhoneNumberNorEmail()
    {
        var employees = new Mock<IEmployeeRepository>();
        var handler = new GetEmployeeHandler(employees.Object);

        var result = await handler.HandleAsync("not-a-valid-search-term", TestContext.Current.CancellationToken);

        Assert.Equal(400, result.Status);
        employees.Verify(d => d.GetEmployeeAsync(It.IsAny<EmployeeLookup>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData("3fa85f64-5717-4562-b3fc-2c963f66afa6")]
    [InlineData("{3FA85F64-5717-4562-B3FC-2C963F66AFA6}")]
    [InlineData("3fa85f6457174562b3fc2c963f66afa6")]
    public async Task GetEmployeeAsync_LooksUpByEmployeeId_ForAnyGuidSpelling(string searchTerm)
    {
        var captured = await CaptureLookup(searchTerm);

        Assert.Equal(new EmployeeLookup(EmployeeId: EmployeeId), captured);
    }

    [Fact]
    public async Task GetEmployeeAsync_LooksUpByPhoneNumber_ForADigitsOnlySearchTerm()
    {
        Assert.Equal(new EmployeeLookup(PhoneNumber: "123456789"), await CaptureLookup("123456789"));
    }

    [Fact]
    public async Task GetEmployeeAsync_LooksUpByEmail_ForAnEmailShapedSearchVariable()
    {
        Assert.Equal(new EmployeeLookup(Email: "dan@example.com"), await CaptureLookup(" dan@example.com "));
    }
}
