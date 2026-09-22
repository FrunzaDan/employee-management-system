using EmployeeManagementSystem.BusinessLogic.Constants;
using EmployeeManagementSystem.BusinessLogic.Validations;
using EmployeeManagementSystem.DataAccess.DBConnection;
using EmployeeManagementSystem.Domain.Models;

namespace EmployeeManagementSystem.BusinessLogic.OrgFunctions;

public class DepartmentFunctions(IDbUtils dbUtils)
{
    public async Task<ResponseModel<object>> CreateDepartmentFunction(DepartmentModel request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.DepartmentName))
            return new ResponseModel<object>(400, "Department name is required.");
        if (request.DepartmentName.Length > FieldLengthConstants.DepartmentName)
            return new ResponseModel<object>(400, "Department name is too long.");

        request.Guid = Guid.NewGuid().ToString();

        return await dbUtils.CreateDepartment(request, cancellationToken);
    }

    public async Task<ResponseModel<object>> GetDepartmentFunction(string guid,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(guid) || !GuidValidation.ValidateGuid(guid))
            return new ResponseModel<object>(400, "A valid department GUID is required.");

        return await dbUtils.GetDepartment(guid, cancellationToken);
    }

    public async Task<ResponseModel<object>> GetDepartmentsFunction(CancellationToken cancellationToken = default) =>
        await dbUtils.GetDepartments(cancellationToken);

    public async Task<ResponseModel<object>> EditDepartmentFunction(DepartmentModel request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(request.Guid) || !GuidValidation.ValidateGuid(request.Guid))
            return new ResponseModel<object>(400, "A valid department GUID is required.");
        if (!string.IsNullOrEmpty(request.DepartmentName) &&
            request.DepartmentName.Length > FieldLengthConstants.DepartmentName)
            return new ResponseModel<object>(400, "Department name is too long.");

        return await dbUtils.EditDepartment(request, cancellationToken);
    }

    public async Task<ResponseModel<object>> DeleteDepartmentFunction(string guid,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(guid) || !GuidValidation.ValidateGuid(guid))
            return new ResponseModel<object>(400, "A valid department GUID is required.");

        return await dbUtils.DeleteDepartment(guid, cancellationToken);
    }

    public async Task<ResponseModel<object>> GetEmployeesByDepartmentFunction(string guid,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(guid) || !GuidValidation.ValidateGuid(guid))
            return new ResponseModel<object>(400, "A valid department GUID is required.");

        return await dbUtils.GetEmployeesByDepartment(guid, cancellationToken);
    }
}
