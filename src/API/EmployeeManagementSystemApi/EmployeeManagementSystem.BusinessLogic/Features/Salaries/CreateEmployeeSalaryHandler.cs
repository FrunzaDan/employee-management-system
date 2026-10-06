using System.Globalization;
using EmployeeManagementSystem.BusinessLogic.Abstractions;
using EmployeeManagementSystem.BusinessLogic.Contracts;
using EmployeeManagementSystem.BusinessLogic.Features.AuditLog;
using EmployeeManagementSystem.Domain.Models;

namespace EmployeeManagementSystem.BusinessLogic.Features.Salaries;

public class CreateEmployeeSalaryHandler(ISalaryRepository salaries, IEmployeeAuditLogger auditLogger)
{
    private const decimal MaxGrossSalary = 9_999_999_999.99m;

    public async Task<ResponseModel<object>> HandleAsync(CreateSalaryRequest request, string performedBy,
        CancellationToken cancellationToken = default)
    {
        if (request.EmployeeId == Guid.Empty)
            return new ResponseModel<object>(400, "A valid employee ID is required.");

        if (request.GrossSalary is not { } grossSalary || grossSalary <= 0)
            return new ResponseModel<object>(400, "Gross salary must be a positive amount.");

        if (grossSalary > MaxGrossSalary)
            return new ResponseModel<object>(400, $"Gross salary can't exceed {MaxGrossSalary:N2}.");

        if (decimal.Round(grossSalary, 2) != grossSalary)
            return new ResponseModel<object>(400, "Gross salary can have at most 2 decimal places.");

        if (request.EffectiveDate is not { } effectiveDate)
            return new ResponseModel<object>(400, "Effective date is required.");

        var response = await salaries.CreateEmployeeSalaryAsync(request, cancellationToken);

        if (response.Status == 200)
            await auditLogger.LogAsync(request.EmployeeId, performedBy, AuditAction.SalaryChanged,
                string.Create(CultureInfo.InvariantCulture,
                    $"Gross salary set to {grossSalary} effective {effectiveDate:yyyy-MM-dd}"), cancellationToken: CancellationToken.None);

        return response;
    }
}
