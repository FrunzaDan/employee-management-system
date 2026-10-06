using EmployeeManagementSystem.BusinessLogic.Abstractions;
using EmployeeManagementSystem.BusinessLogic.Contracts;
using EmployeeManagementSystem.BusinessLogic.Features.Employees;
using EmployeeManagementSystem.Domain.Models;
using Moq;

namespace EmployeeManagementSystem.Tests.Features.Employees;

public class GetEmployeeInsightsHandlerTests
{
    [Fact]
    public async Task GetEmployeeInsightsAsync_ReturnsWhateverTheDbLayerReturns()
    {
        var employees = new Mock<IEmployeeRepository>();
        var expected = new ResponseModel<EmployeeInsightsModel>(200, "Employee insights retrieved.",
            new EmployeeInsightsModel { Employees = [] });
        employees.Setup(d => d.GetEmployeeInsightsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(expected);
        var handler = new GetEmployeeInsightsHandler(employees.Object);

        var result = await handler.HandleAsync(TestContext.Current.CancellationToken);

        Assert.Same(expected, result);
    }
}
