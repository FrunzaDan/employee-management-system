using EmployeeManagementSystem.Domain.Models;

namespace EmployeeManagementSystem.BusinessLogic.Services;

public interface IAuthService
{
    Task<ResponseModel<object>> GetAccessToken(EmployerCredentials employerCredentials,
        CancellationToken cancellationToken = default);
}