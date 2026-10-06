using EmployeeManagementSystem.BusinessLogic.Abstractions;
using EmployeeManagementSystem.BusinessLogic.Contracts;
using EmployeeManagementSystem.BusinessLogic.Features.AuditLog;
using EmployeeManagementSystem.BusinessLogic.Features.Salaries;
using EmployeeManagementSystem.Domain.Models;
using Moq;

namespace EmployeeManagementSystem.Tests.Features.Salaries;

public class CreateEmployeeSalaryHandlerTests
{
    private static readonly Guid EmployeeId = Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6");

    private const string PerformedBy = "TestEmployer";

    private static CreateSalaryRequest ValidRequest() => new()
    {
        EmployeeId = EmployeeId,
        GrossSalary = 5000.50m,
        EffectiveDate = new DateOnly(2026, 9, 1)
    };

    public static TheoryData<CreateSalaryRequest, string> InvalidRequests => new()
    {
        { new CreateSalaryRequest { EmployeeId = Guid.Empty, GrossSalary = 1, EffectiveDate = new DateOnly(2026, 1, 1) }, "employee ID" },
        { new CreateSalaryRequest { EmployeeId = EmployeeId, GrossSalary = null, EffectiveDate = new DateOnly(2026, 1, 1) }, "positive" },
        { new CreateSalaryRequest { EmployeeId = EmployeeId, GrossSalary = 0, EffectiveDate = new DateOnly(2026, 1, 1) }, "positive" },
        { new CreateSalaryRequest { EmployeeId = EmployeeId, GrossSalary = 10_000_000_000m, EffectiveDate = new DateOnly(2026, 1, 1) }, "exceed" },
        { new CreateSalaryRequest { EmployeeId = EmployeeId, GrossSalary = 1.005m, EffectiveDate = new DateOnly(2026, 1, 1) }, "2 decimal places" },
        { new CreateSalaryRequest { EmployeeId = EmployeeId, GrossSalary = 1, EffectiveDate = null }, "Effective date" }
    };

    [Fact]
    public async Task AddSalaryFunction_RecordsTheEntry_AndLogsASalaryChangedAuditEntry()
    {
        var salaries = new Mock<ISalaryRepository>();
        var auditLogger = new Mock<IEmployeeAuditLogger>();
        var expected = new ResponseModel<object>(200, "Salary entry added successfully.");
        salaries.Setup(d => d.CreateEmployeeSalaryAsync(It.IsAny<CreateSalaryRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);
        var handler = new CreateEmployeeSalaryHandler(salaries.Object, auditLogger.Object);

        var result = await handler.HandleAsync(ValidRequest(), PerformedBy, TestContext.Current.CancellationToken);

        Assert.Same(expected, result);
        auditLogger.Verify(a => a.LogAsync(EmployeeId, PerformedBy, AuditAction.SalaryChanged,
            "Gross salary set to 5000.50 effective 2026-09-01", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AddSalaryFunction_DoesNotLogAnAuditEntry_WhenTheDbLayerRejectsIt()
    {
        var salaries = new Mock<ISalaryRepository>();
        var auditLogger = new Mock<IEmployeeAuditLogger>();
        salaries.Setup(d => d.CreateEmployeeSalaryAsync(It.IsAny<CreateSalaryRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ResponseModel<object>(404, "Employee not found."));
        var handler = new CreateEmployeeSalaryHandler(salaries.Object, auditLogger.Object);

        var result = await handler.HandleAsync(ValidRequest(), PerformedBy, TestContext.Current.CancellationToken);

        Assert.Equal(404, result.Status);
        auditLogger.Verify(a => a.LogAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<AuditAction>(),
            It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [MemberData(nameof(InvalidRequests))]
    public async Task AddSalaryFunction_RejectsAnInvalidRequest_WithoutTouchingTheDb(CreateSalaryRequest request,
        string expectedMessagePart)
    {
        var salaries = new Mock<ISalaryRepository>();
        var handler = new CreateEmployeeSalaryHandler(salaries.Object, Mock.Of<IEmployeeAuditLogger>());

        var result = await handler.HandleAsync(request, PerformedBy, TestContext.Current.CancellationToken);

        Assert.Equal(400, result.Status);
        Assert.Contains(expectedMessagePart, result.ResponseMessage);
        salaries.Verify(d => d.CreateEmployeeSalaryAsync(It.IsAny<CreateSalaryRequest>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
