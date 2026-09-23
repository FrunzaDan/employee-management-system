using EmployeeManagementSystem.Domain.Constants;
using EmployeeManagementSystem.BusinessLogic.Validations;
using EmployeeManagementSystem.DataAccess.DBConnection;
using EmployeeManagementSystem.Domain.Models;

namespace EmployeeManagementSystem.BusinessLogic.EmployeeFunctions;

public class EmployeeEditing(IDbUtils dbUtils, IEmployeeAuditLogger auditLogger)
{
    public async Task<ResponseModel<object>> EditEmployeeFunction(EmployeeModel request, string employerId,
        CancellationToken cancellationToken = default)
    {
        if (request.Guid is not { } guid || guid == Guid.Empty)
            return new ResponseModel<object>(400, "Invalid or empty Guid.");

        if (!string.IsNullOrEmpty(request.FirstName) && request.FirstName.Length > FieldLengthConstants.FirstName)
            return new ResponseModel<object>(400, "First name is too long.");

        if (!string.IsNullOrEmpty(request.LastName) && request.LastName.Length > FieldLengthConstants.LastName)
            return new ResponseModel<object>(400, "Last name is too long.");

        if (!string.IsNullOrEmpty(request.Email) && EmailValidation.ValidateEmail(request.Email) == false)
            return new ResponseModel<object>(400, "Invalid Email.");
        if (!string.IsNullOrEmpty(request.Email) && request.Email.Length > FieldLengthConstants.Email)
            return new ResponseModel<object>(400, "Email is too long.");

        if (!string.IsNullOrEmpty(request.Msisdn) && MsisdnValidation.ValidateMsisdn(request.Msisdn) == false)
            return new ResponseModel<object>(400, "Invalid MSISDN.");

        // Birthdate/HireDate are DateOnly — already format-checked by model binding. Gender
        // isn't: JSON-to-enum binding accepts any integer, not just the defined ones.
        if (request.Gender is { } gender && !Enum.IsDefined(gender))
            return new ResponseModel<object>(400, "Invalid Gender value.");

        if (request.Address is not null)
        {
            var addressLengthError = AddressValidation.ValidateLengths(request.Address);
            if (addressLengthError is not null)
                return new ResponseModel<object>(400, addressLengthError);
        }

        // Edit is a partial update, not a lifecycle transition — Active/Test are the only
        // statuses that arrive this way "unchanged" from an existing record; anything else
        // (e.g. a client-supplied Deactivated) would bypass DeactivateEmployee/DeleteEmployee's
        // dedicated audit trail and the stored procedures' own lifecycle guardrails.
        if (request.EmployeeStatus is not (null or EmployeeStatus.Active or EmployeeStatus.Test))
            return new ResponseModel<object>(400,
                "Employee status can't be changed via edit; use deactivate/reactivate/delete instead.");

        var response = await dbUtils.EditEmployee(request, cancellationToken);

        // Not forwarding cancellationToken: the edit already succeeded, so the audit write
        // should still be attempted even if the client has since disconnected.
        if (response.Status == 200)
            await auditLogger.Log(guid, employerId, "Edited", DescribeChangedFields(request));

        return response;
    }

    // Employee_Update is a partial update (ISNULL(@param, column)) — only the fields
    // actually present in the request were touched, so list just those.
    private static string DescribeChangedFields(EmployeeModel request)
    {
        var changedFields = new List<string>();

        if (!string.IsNullOrEmpty(request.FirstName)) changedFields.Add("first name");
        if (!string.IsNullOrEmpty(request.LastName)) changedFields.Add("last name");
        if (!string.IsNullOrEmpty(request.Email)) changedFields.Add("email");
        if (!string.IsNullOrEmpty(request.Msisdn)) changedFields.Add("MSISDN");
        if (request.Gender is not null) changedFields.Add("gender");
        if (request.Birthdate is not null) changedFields.Add("birthdate");
        if (request.Address is not null) changedFields.Add("address");
        if (request.HireDate is not null) changedFields.Add("hire date");
        if (request.OfficeGuid is not null) changedFields.Add("office");
        if (request.DepartmentGuid is not null) changedFields.Add("department");
        if (request.CostCenterGuid is not null) changedFields.Add("cost center");

        return changedFields.Count > 0 ? $"Updated: {string.Join(", ", changedFields)}" : "No fields changed";
    }
}