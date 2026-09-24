using System.Globalization;
using EmployeeManagementSystem.DataAccess.DBConnection;
using EmployeeManagementSystem.Domain.Models;

namespace EmployeeManagementSystem.BusinessLogic.EmployeeFunctions;

// Salary is append-only history (see database.md/EmployeeSalary) — there is
// deliberately no EditSalary/DeleteSalary; a correction is just a new entry with a later
// effective date, same as how a real payroll change is recorded.
public class EmployeeSalary(IDbUtils dbUtils, IEmployeeAuditLogger auditLogger)
{
    // EmployeeSalary.GrossSalary is DECIMAL(12, 2): at most 10 integer digits
    // and 2 decimals. Checked here so an out-of-range amount is a clean 400, not an arithmetic
    // overflow 500 — and so a third decimal isn't silently rounded away by SQL Server.
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

        // Not forwarding cancellationToken: the entry was already recorded, so the audit write
        // should still be attempted even if the client has since disconnected.
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
