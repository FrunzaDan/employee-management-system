using EmployeeManagementSystem.Domain.Constants;
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

        request.Guid = SequentialGuid.NewGuid();

        return await dbUtils.CreateDepartment(request, cancellationToken);
    }

    public async Task<ResponseModel<object>> GetDepartmentFunction(Guid guid,
        CancellationToken cancellationToken = default)
    {
        if (guid == Guid.Empty)
            return new ResponseModel<object>(400, "A valid department GUID is required.");

        return await dbUtils.GetDepartment(guid, cancellationToken);
    }

    public async Task<ResponseModel<object>> GetDepartmentsFunction(CancellationToken cancellationToken = default) =>
        await dbUtils.GetDepartments(cancellationToken);

    public async Task<ResponseModel<object>> EditDepartmentFunction(DepartmentModel request,
        CancellationToken cancellationToken = default)
    {
        if (request.Guid is null || request.Guid == Guid.Empty)
            return new ResponseModel<object>(400, "A valid department GUID is required.");
        if (!string.IsNullOrEmpty(request.DepartmentName) &&
            request.DepartmentName.Length > FieldLengthConstants.DepartmentName)
            return new ResponseModel<object>(400, "Department name is too long.");

        return await dbUtils.EditDepartment(request, cancellationToken);
    }

    public async Task<ResponseModel<object>> DeleteDepartmentFunction(Guid guid,
        CancellationToken cancellationToken = default)
    {
        if (guid == Guid.Empty)
            return new ResponseModel<object>(400, "A valid department GUID is required.");

        return await dbUtils.DeleteDepartment(guid, cancellationToken);
    }

    public async Task<ResponseModel<object>> GetEmployeesByDepartmentFunction(Guid guid,
        CancellationToken cancellationToken = default)
    {
        if (guid == Guid.Empty)
            return new ResponseModel<object>(400, "A valid department GUID is required.");

        return await dbUtils.GetEmployeesByDepartment(guid, cancellationToken);
    }
}
