using System.Globalization;
using EmployeeManagementSystem.DataAccess.DBConnection;
using EmployeeManagementSystem.Domain.Models;

namespace EmployeeManagementSystem.BusinessLogic.EmployeeFunctions;

public class EmployeeSalary(IDbUtils dbUtils, IEmployeeAuditLogger auditLogger)
{
    private const decimal MaxGrossSalary = 9_999_999_999.99m;

    public async Task<ResponseModel<object>> CreateEmployeeSalaryAsync(CreateSalaryRequest request, string performedBy,
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

        var response = await dbUtils.CreateEmployeeSalaryAsync(request, cancellationToken);

        if (response.Status == 200)
            await auditLogger.LogAsync(request.EmployeeId, performedBy, AuditAction.SalaryChanged,
                string.Create(CultureInfo.InvariantCulture,
                    $"Gross salary set to {grossSalary} effective {effectiveDate:yyyy-MM-dd}"));

        return response;
    }

    public async Task<ResponseModel<IReadOnlyList<SalaryModel>>> GetEmployeeSalaryHistoryAsync(Guid employeeId,
        CancellationToken cancellationToken = default)
    {
        if (employeeId == Guid.Empty)
            return new ResponseModel<IReadOnlyList<SalaryModel>>(400, "A valid employee ID is required.");

        return await dbUtils.GetEmployeeSalaryHistoryAsync(employeeId, cancellationToken);
    }
}
