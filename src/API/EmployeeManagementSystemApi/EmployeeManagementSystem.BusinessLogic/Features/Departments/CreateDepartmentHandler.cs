using EmployeeManagementSystem.BusinessLogic.Abstractions;
using EmployeeManagementSystem.BusinessLogic.Contracts;
using EmployeeManagementSystem.Domain.Constants;

namespace EmployeeManagementSystem.BusinessLogic.Features.Departments;

public class CreateDepartmentHandler(IDepartmentRepository departments)
{
    public async Task<ResponseModel<Guid?>> HandleAsync(CreateDepartmentRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return new ResponseModel<Guid?>(400, "Department name is required.");
        if (request.Name.Length > FieldLengthConstants.DepartmentName)
            return new ResponseModel<Guid?>(400, "Department name is too long.");

        return await departments.CreateDepartmentAsync(request, cancellationToken);
    }
}
