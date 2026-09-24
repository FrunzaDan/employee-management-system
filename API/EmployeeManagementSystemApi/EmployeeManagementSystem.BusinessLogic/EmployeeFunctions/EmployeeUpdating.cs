using EmployeeManagementSystem.BusinessLogic.Validations;
using EmployeeManagementSystem.DataAccess.DBConnection;
using EmployeeManagementSystem.Domain.Constants;
using EmployeeManagementSystem.Domain.Models;

namespace EmployeeManagementSystem.BusinessLogic.EmployeeFunctions;

public class EmployeeUpdating(IDbUtils dbUtils, IEmployeeAuditLogger auditLogger)
{
    public async Task<ResponseModel<object>> UpdateEmployeeAsync(UpdateEmployeeRequest request, string performedBy,
        CancellationToken cancellationToken = default)
    {
        if (request.EmployeeId == Guid.Empty)
            return new ResponseModel<object>(400, "Invalid or empty employee ID.");

        if (!string.IsNullOrEmpty(request.FirstName) && request.FirstName.Length > FieldLengthConstants.FirstName)
            return new ResponseModel<object>(400, "First name is too long.");

        if (!string.IsNullOrEmpty(request.LastName) && request.LastName.Length > FieldLengthConstants.LastName)
            return new ResponseModel<object>(400, "Last name is too long.");

        if (!string.IsNullOrEmpty(request.Email) && EmailValidation.ValidateEmail(request.Email) == false)
            return new ResponseModel<object>(400, "Invalid Email.");
        if (!string.IsNullOrEmpty(request.Email) && request.Email.Length > FieldLengthConstants.Email)
            return new ResponseModel<object>(400, "Email is too long.");

        if (!string.IsNullOrEmpty(request.PhoneNumber) && PhoneNumberValidation.ValidatePhoneNumber(request.PhoneNumber) == false)
            return new ResponseModel<object>(400, "Invalid phone number.");

        if (request.Gender is { } gender && !Enum.IsDefined(gender))
            return new ResponseModel<object>(400, "Invalid Gender value.");

        if (request.Address is not null)
        {
            var addressLengthError = AddressValidation.ValidateLengths(request.Address);
            if (addressLengthError is not null)
                return new ResponseModel<object>(400, addressLengthError);
        }

        var response = await dbUtils.UpdateEmployeeAsync(request, cancellationToken);

        if (response.Status == 200)
            await auditLogger.LogAsync(request.EmployeeId, performedBy, AuditAction.Edited, DescribeChangedFields(request));

        return response;
    }

    private static string DescribeChangedFields(UpdateEmployeeRequest request)
    {
        var changedFields = new List<string>();

        if (!string.IsNullOrEmpty(request.FirstName)) changedFields.Add("first name");
        if (!string.IsNullOrEmpty(request.LastName)) changedFields.Add("last name");
        if (!string.IsNullOrEmpty(request.Email)) changedFields.Add("email");
        if (!string.IsNullOrEmpty(request.PhoneNumber)) changedFields.Add("phone number");
        if (request.Gender is not null) changedFields.Add("gender");
        if (request.BirthDate is not null) changedFields.Add("birth date");
        if (request.Address is not null) changedFields.Add("address");
        if (request.HireDate is not null) changedFields.Add("hire date");
        if (request.OfficeId is not null) changedFields.Add("office");
        if (request.DepartmentId is not null) changedFields.Add("department");
        if (request.CostCenterId is not null) changedFields.Add("cost center");

        return changedFields.Count > 0 ? $"Updated: {string.Join(", ", changedFields)}" : "No fields changed";
    }
}
