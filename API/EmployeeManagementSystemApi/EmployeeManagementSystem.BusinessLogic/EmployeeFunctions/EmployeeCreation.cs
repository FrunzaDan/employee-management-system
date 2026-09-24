using EmployeeManagementSystem.BusinessLogic.Validations;
using EmployeeManagementSystem.DataAccess.DBConnection;
using EmployeeManagementSystem.Domain.Constants;
using EmployeeManagementSystem.Domain.Models;

namespace EmployeeManagementSystem.BusinessLogic.EmployeeFunctions;

public class EmployeeCreation(IDbUtils dbUtils, IEmployeeAuditLogger auditLogger)
{
    public async Task<ResponseModel<Guid?>> CreateEmployeeAsync(CreateEmployeeRequest request, string performedBy,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.FirstName))
            return new ResponseModel<Guid?>(400, "First name is required.");
        if (request.FirstName.Length > FieldLengthConstants.FirstName)
            return new ResponseModel<Guid?>(400, "First name is too long.");

        if (string.IsNullOrWhiteSpace(request.LastName))
            return new ResponseModel<Guid?>(400, "Last name is required.");
        if (request.LastName.Length > FieldLengthConstants.LastName)
            return new ResponseModel<Guid?>(400, "Last name is too long.");

        if (string.IsNullOrEmpty(request.Email) || EmailValidation.ValidateEmail(request.Email) == false)
            return new ResponseModel<Guid?>(400, "Invalid or empty Email.");
        if (request.Email.Length > FieldLengthConstants.Email)
            return new ResponseModel<Guid?>(400, "Email is too long.");

        if (string.IsNullOrEmpty(request.PhoneNumber) || PhoneNumberValidation.ValidatePhoneNumber(request.PhoneNumber) == false)
            return new ResponseModel<Guid?>(400, "Invalid or empty phone number.");

        if (request.Gender is { } gender && !Enum.IsDefined(gender))
            return new ResponseModel<Guid?>(400, "Invalid Gender value.");

        if (request.BirthDate > DateOnly.FromDateTime(DateTime.UtcNow))
            return new ResponseModel<Guid?>(400, "Birth date cannot be in the future.");

        if (request.Address is null)
            return new ResponseModel<Guid?>(400, "Address is required.");

        var addressError = AddressValidation.ValidateRequired(request.Address)
                           ?? AddressValidation.ValidateLengths(request.Address);
        if (addressError is not null)
            return new ResponseModel<Guid?>(400, addressError);

        if (request.Status is not null and not (EmployeeStatus.Active or EmployeeStatus.Test))
            return new ResponseModel<Guid?>(400, "Invalid employee status.");

        var response = await dbUtils.CreateEmployeeAsync(request, cancellationToken);

        if (response is { Status: 200, Data: { } employeeId })
            await auditLogger.LogAsync(employeeId, performedBy, AuditAction.Created,
                $"Email: {request.Email}, Phone number: {request.PhoneNumber}");

        return response;
    }
}
