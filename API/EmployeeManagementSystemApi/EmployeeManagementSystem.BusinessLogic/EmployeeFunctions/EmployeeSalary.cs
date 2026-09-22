using EmployeeManagementSystem.BusinessLogic.Validations;
using EmployeeManagementSystem.DataAccess.DBConnection;
using EmployeeManagementSystem.Domain.Models;

namespace EmployeeManagementSystem.BusinessLogic.EmployeeFunctions;

// Salary is append-only history (see database.md/tbl_employee_salary_history) — there is
// deliberately no EditSalary/DeleteSalary; a correction is just a new entry with a later
// effective date, same as how a real payroll change is recorded.
public class EmployeeSalary(IDbUtils dbUtils, IEmployeeAuditLogger auditLogger)
{
    public async Task<ResponseModel<object>> AddSalaryFunction(SalaryHistoryEntry request, string employerId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(request.EmployeeGuid) || !GuidValidation.ValidateGuid(request.EmployeeGuid))
            return new ResponseModel<object>(400, "A valid employee GUID is required.");

        if (request.BruttoSalary is null || request.BruttoSalary <= 0)
            return new ResponseModel<object>(400, "Brutto salary must be a positive amount.");

        if (string.IsNullOrEmpty(request.EffectiveDate) || !DateOnly.TryParse(request.EffectiveDate, out _))
            return new ResponseModel<object>(400, "Invalid effective date format.");

        // Same reasoning as every other GUID in this app: always server-generated.
        request.SalaryGuid = Guid.NewGuid().ToString();

        var response = await dbUtils.AddEmployeeSalary(request, cancellationToken);

        if (response.Status == 200)
            await auditLogger.Log(request.EmployeeGuid, employerId, "Salary changed",
                $"Brutto salary set to {request.BruttoSalary} effective {request.EffectiveDate}");

        return response;
    }

    public async Task<ResponseModel<object>> GetSalaryHistoryFunction(string employeeGuid,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(employeeGuid) || !GuidValidation.ValidateGuid(employeeGuid))
            return new ResponseModel<object>(400, "A valid employee GUID is required.");

        return await dbUtils.GetEmployeeSalaryHistory(employeeGuid, cancellationToken);
    }
}
