using EmployeeManagementSystem.BusinessLogic.Constants;
using EmployeeManagementSystem.BusinessLogic.Validations;
using EmployeeManagementSystem.DataAccess.DBConnection;
using EmployeeManagementSystem.Domain.Models;

namespace EmployeeManagementSystem.BusinessLogic.EmployeeFunctions;

public class EmployeeRegistration
{
    private readonly IDbUtils _dbUtils;
    private readonly IEmployeeAuditLogger _auditLogger;

    public EmployeeRegistration(IDbUtils dbUtils, IEmployeeAuditLogger auditLogger)
    {
        _dbUtils = dbUtils;
        _auditLogger = auditLogger;
    }

    public async Task<ResponseModel<object>> RegisterEmployeeFunction(EmployeeModel request, string employerId,
        CancellationToken cancellationToken = default)
    {
        // first_name/last_name are nullable columns with no SQL-side requirement, so without
        // this check a request that omits them would silently create a nameless employee.
        if (string.IsNullOrWhiteSpace(request.FirstName))
            return new ResponseModel<object>(400, "First name is required.");
        if (request.FirstName.Length > FieldLengthConstants.FirstName)
            return new ResponseModel<object>(400, "First name is too long.");

        if (string.IsNullOrWhiteSpace(request.LastName))
            return new ResponseModel<object>(400, "Last name is required.");
        if (request.LastName.Length > FieldLengthConstants.LastName)
            return new ResponseModel<object>(400, "Last name is too long.");

        if (string.IsNullOrEmpty(request.Email) || EmailValidation.ValidateEmail(request.Email) == false)
            return new ResponseModel<object>(400, "Invalid or empty Email.");
        if (request.Email.Length > FieldLengthConstants.Email)
            return new ResponseModel<object>(400, "Email is too long.");

        if (string.IsNullOrEmpty(request.Msisdn) || MsisdnValidation.ValidateMsisdn(request.Msisdn) == false)
            return new ResponseModel<object>(400, "Invalid or empty MSISDN.");

        if (request.Birthdate is not null)
        {
            if (request.Birthdate.Length > FieldLengthConstants.Birthdate
                || !DateOnly.TryParse(request.Birthdate, out _))
                return new ResponseModel<object>(400, "Invalid Birthdate format.");
        }

        if (request.Gender is not null && request.Gender is not (0 or 1 or 2))
            return new ResponseModel<object>(400, "Invalid Gender value.");

        if (request.HireDate is not null)
        {
            if (request.HireDate.Length > FieldLengthConstants.HireDate
                || !DateOnly.TryParse(request.HireDate, out _))
                return new ResponseModel<object>(400, "Invalid Hire date format.");
        }

        // usp_createEmployee's address parameters have no SQL-side defaults, so a missing
        // Address would otherwise surface as an opaque 500 instead of a validation error.
        if (request.Address is null)
            return new ResponseModel<object>(400, "Address is required.");

        var addressLengthError = AddressValidation.ValidateLengths(request.Address);
        if (addressLengthError is not null)
            return new ResponseModel<object>(400, addressLengthError);

        // A new employee's identifier is always generated server-side; a client-supplied GUID is never trusted.
        request.Guid = Guid.NewGuid().ToString();

        // A new employee is active by default. The only other status a caller may request at
        // creation time is Test (used by the About page's bulk test-data generator) — anything
        // else (e.g. Deactivated) would bypass the deactivate/reactivate/delete lifecycle rules
        // that are otherwise enforced by the stored procedures.
        if (request.EmployeeStatus is not null
            && request.EmployeeStatus != EmployeeStatusCodes.Active
            && request.EmployeeStatus != EmployeeStatusCodes.Test)
            return new ResponseModel<object>(400, "Invalid employee status.");

        request.EmployeeStatus ??= EmployeeStatusCodes.Active;

        var response = await _dbUtils.RegisterEmployee(request, cancellationToken);

        // Deliberately not forwarding cancellationToken here: the employee was already created
        // successfully, so the audit write should still be attempted even if the client that
        // triggered it has since disconnected — same best-effort guarantee EmployeeAuditLogger
        // already gives on write failures, just not conditional on the caller still being there.
        if (response.Status == 200)
            await _auditLogger.Log(request.Guid, employerId, "Created",
                $"Email: {request.Email}, MSISDN: {request.Msisdn}");

        return response;
    }
}