using EmployeeManagementSystem.BusinessLogic.Abstractions;
using EmployeeManagementSystem.BusinessLogic.Contracts;
using EmployeeManagementSystem.BusinessLogic.Validations;
using EmployeeManagementSystem.Domain.Constants;
using EmployeeManagementSystem.Domain.Models;

namespace EmployeeManagementSystem.BusinessLogic.Features.Employees;

public class GetEmployeeHandler(IEmployeeRepository employees)
{
    private static EmployeeLookup? DetermineLookup(string searchTerm)
    {
        if (Guid.TryParse(searchTerm, out var employeeId))
            return new EmployeeLookup(EmployeeId: employeeId);

        if (PhoneNumberValidation.ValidatePhoneNumber(searchTerm))
            return new EmployeeLookup(PhoneNumber: searchTerm);

        if (EmailValidation.ValidateEmail(searchTerm) && searchTerm.Length <= FieldLengthConstants.Email)
            return new EmployeeLookup(Email: searchTerm);

        return null;
    }

    public async Task<ResponseModel<EmployeeModel>> HandleAsync(string? searchTerm,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return new ResponseModel<EmployeeModel>(400, "Search variable cannot be null or empty.");

        var lookup = DetermineLookup(searchTerm.Trim());
        if (lookup is null)
            return new ResponseModel<EmployeeModel>(400,
                "No valid search variable was provided! It must be a employee ID, phone number, or email.");

        return await employees.GetEmployeeAsync(lookup, cancellationToken);
    }
}
