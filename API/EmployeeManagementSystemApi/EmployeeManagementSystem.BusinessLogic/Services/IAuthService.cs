using EmployeeManagementSystem.Domain.Models;

namespace EmployeeManagementSystem.BusinessLogic.Services;

public interface IAuthService
{
    Task<ResponseModel<AccessTokenResponse>> GetAccessTokenAsync(EmployerCredentials employerCredentials,
        CancellationToken cancellationToken = default);
}