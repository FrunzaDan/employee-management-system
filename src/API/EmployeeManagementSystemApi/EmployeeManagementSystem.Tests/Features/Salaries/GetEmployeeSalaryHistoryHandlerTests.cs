using EmployeeManagementSystem.BusinessLogic.Abstractions;
using EmployeeManagementSystem.BusinessLogic.Features.Salaries;
using Moq;

namespace EmployeeManagementSystem.Tests.Features.Salaries;

public class GetEmployeeSalaryHistoryHandlerTests
{
    [Fact]
    public async Task GetEmployeeSalaryHistoryAsync_RejectsAnEmptyEmployeeId_WithoutTouchingTheDb()
    {
        var salaries = new Mock<ISalaryRepository>();
        var handler = new GetEmployeeSalaryHistoryHandler(salaries.Object);

        var result = await handler.HandleAsync(Guid.Empty, TestContext.Current.CancellationToken);

        Assert.Equal(400, result.Status);
        salaries.Verify(d => d.GetEmployeeSalaryHistoryAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
