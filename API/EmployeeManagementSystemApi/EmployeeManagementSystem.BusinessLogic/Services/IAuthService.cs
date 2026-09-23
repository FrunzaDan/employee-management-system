using EmployeeManagementSystem.Domain.Models;

namespace EmployeeManagementSystem.BusinessLogic.Services;

public interface IAuthService
{
    Task<ResponseModel<AccessTokenResponse>> GetAccessToken(EmployerCredentials employerCredentials,
        CancellationToken cancellationToken = default);
}