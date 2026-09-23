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
    private const decimal MaxBruttoSalary = 9_999_999_999.99m;

    public async Task<ResponseModel<object>> AddSalaryFunction(SalaryHistoryEntry request, string employerId,
        CancellationToken cancellationToken = default)
    {
        if (request.EmployeeGuid is not { } employeeGuid || employeeGuid == Guid.Empty)
            return new ResponseModel<object>(400, "A valid employee GUID is required.");

        if (request.BruttoSalary is not { } bruttoSalary || bruttoSalary <= 0)
            return new ResponseModel<object>(400, "Brutto salary must be a positive amount.");

        if (bruttoSalary > MaxBruttoSalary)
            return new ResponseModel<object>(400, $"Brutto salary can't exceed {MaxBruttoSalary:N2}.");

        if (decimal.Round(bruttoSalary, 2) != bruttoSalary)
            return new ResponseModel<object>(400, "Brutto salary can have at most 2 decimal places.");

        if (request.EffectiveDate is not { } effectiveDate)
            return new ResponseModel<object>(400, "Effective date is required.");

        // Same reasoning as every other GUID in this app: always server-generated.
        request.SalaryGuid = SequentialGuid.NewGuid();

        var response = await dbUtils.AddEmployeeSalary(request, cancellationToken);

        if (response.Status == 200)
            await auditLogger.Log(employeeGuid, employerId, "Salary changed",
                string.Create(CultureInfo.InvariantCulture,
                    $"Brutto salary set to {bruttoSalary} effective {effectiveDate:yyyy-MM-dd}"));

        return response;
    }

    public async Task<ResponseModel<object>> GetSalaryHistoryFunction(Guid employeeGuid,
        CancellationToken cancellationToken = default)
    {
        if (employeeGuid == Guid.Empty)
            return new ResponseModel<object>(400, "A valid employee GUID is required.");

        return await dbUtils.GetEmployeeSalaryHistory(employeeGuid, cancellationToken);
    }
}
