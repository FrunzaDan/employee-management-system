using EmployeeManagementSystem.BusinessLogic.AuthFunctions;
using EmployeeManagementSystem.Domain.Models;

namespace EmployeeManagementSystem.BusinessLogic.Services.Implementation;

public class AuthService(JwtCreation jwtCreation) : IAuthService
{
    public async Task<ResponseModel<AccessTokenResponse>> GetAccessToken(EmployerCredentials employerCredentials,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(employerCredentials.Username) ||
            string.IsNullOrWhiteSpace(employerCredentials.Password))
            return new ResponseModel<AccessTokenResponse>(400, "Username and password are required.");

        return await jwtCreation.GenerateBearerJwt(employerCredentials, cancellationToken);
    }
}