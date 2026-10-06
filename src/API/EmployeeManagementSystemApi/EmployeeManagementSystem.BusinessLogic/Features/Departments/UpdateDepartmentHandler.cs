using EmployeeManagementSystem.BusinessLogic.Abstractions;
using EmployeeManagementSystem.BusinessLogic.Contracts;
using EmployeeManagementSystem.Domain.Constants;

namespace EmployeeManagementSystem.BusinessLogic.Features.Departments;

public class UpdateDepartmentHandler(IDepartmentRepository departments)
{
    public async Task<ResponseModel<object>> HandleAsync(UpdateDepartmentRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.DepartmentId == Guid.Empty)
            return new ResponseModel<object>(400, "A valid department ID is required.");
        if (request.Name is not null && string.IsNullOrWhiteSpace(request.Name))
            return new ResponseModel<object>(400, "Department name cannot be blank.");
        if (request.Name?.Length > FieldLengthConstants.DepartmentName)
            return new ResponseModel<object>(400, "Department name is too long.");

        return await departments.UpdateDepartmentAsync(request, cancellationToken);
    }
}
