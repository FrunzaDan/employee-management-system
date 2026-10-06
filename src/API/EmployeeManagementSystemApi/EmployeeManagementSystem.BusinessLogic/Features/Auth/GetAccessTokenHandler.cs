using EmployeeManagementSystem.BusinessLogic.Contracts;

namespace EmployeeManagementSystem.BusinessLogic.Features.Auth;

public class GetAccessTokenHandler(JwtCreation jwtCreation)
{
    public async Task<ResponseModel<AccessTokenResponse>> HandleAsync(EmployerCredentials employerCredentials,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(employerCredentials.Username) ||
            string.IsNullOrWhiteSpace(employerCredentials.Password))
            return new ResponseModel<AccessTokenResponse>(400, "Username and password are required.");

        return await jwtCreation.GenerateBearerJwtAsync(employerCredentials, cancellationToken);
    }
}
