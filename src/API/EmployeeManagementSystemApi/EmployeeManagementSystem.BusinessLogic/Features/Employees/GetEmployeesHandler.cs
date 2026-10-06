using EmployeeManagementSystem.BusinessLogic.Abstractions;
using EmployeeManagementSystem.BusinessLogic.Constants;
using EmployeeManagementSystem.BusinessLogic.Contracts;
using EmployeeManagementSystem.Domain.Models;

namespace EmployeeManagementSystem.BusinessLogic.Features.Employees;

public class GetEmployeesHandler(IEmployeeRepository employees)
{
    public async Task<ResponseModel<PagedResponse<EmployeeModel>>> HandleAsync(GetEmployeesRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.PageNumber < 1)
            return new ResponseModel<PagedResponse<EmployeeModel>>(400, "Page number must be 1 or greater.");

        if (request.PageSize < 1 || request.PageSize > PagingConstants.MaxPageSize)
            return new ResponseModel<PagedResponse<EmployeeModel>>(400,
                $"Page size must be between 1 and {PagingConstants.MaxPageSize}.");

        var validationError = EmployeeListQuery.ValidateAndNormalizeSortAndSearch(request);
        if (validationError != null)
            return new ResponseModel<PagedResponse<EmployeeModel>>(400, validationError);

        return await employees.GetEmployeesAsync(request, cancellationToken);
    }
}
