using EmployeeManagementSystem.BusinessLogic.AuthFunctions;
using EmployeeManagementSystem.Domain.Models;

namespace EmployeeManagementSystem.BusinessLogic.Services.Implementation;

public class AuthService(JwtCreation jwtCreation) : IAuthService
{
    public async Task<ResponseModel<object>> GetAccessToken(EmployerCredentials employerCredentials,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(employerCredentials.EmployerId) ||
            string.IsNullOrWhiteSpace(employerCredentials.EmployerPassword))
            return new ResponseModel<object>(403, "Invalid or empty employer credentials.");

        return await jwtCreation.GenerateBearerJwt(employerCredentials, cancellationToken);
    }
}