using System.Globalization;
using System.Security.Claims;
using EmployeeManagementSystem.DataAccess.DBConnection;
using EmployeeManagementSystem.Domain.Configuration;
using EmployeeManagementSystem.Domain.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace EmployeeManagementSystem.BusinessLogic.AuthFunctions;

public class JwtCreation
{
    private readonly AuthOptions _authOptions;
    private readonly IDbUtils _dbUtils;
    private readonly SymmetricSecurityKey _signingKey;

    public JwtCreation(IOptions<AuthOptions> authOptions, IDbUtils dbUtils)
    {
        _dbUtils = dbUtils;
        _authOptions = authOptions.Value;
        _signingKey = JwtSigningKey.Create(_authOptions.SecureJwtKey);
    }

    public async Task<ResponseModel<AccessTokenResponse>> GenerateBearerJwtAsync(EmployerCredentials employerCredentials,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(employerCredentials.Username))
            return new ResponseModel<AccessTokenResponse>(400, "Username and password are required.");

        var credentialsCheck = await _dbUtils.CheckEmployerCredentialsFromDbAsync(employerCredentials, cancellationToken);

        if (credentialsCheck.Status != StatusCodes.Status200OK)
            return new ResponseModel<AccessTokenResponse>(credentialsCheck.Status, credentialsCheck.ResponseMessage);

        var expires = DateTime.UtcNow.AddMinutes(_authOptions.AccessTokenTimeoutMinutes);
        var token = GenerateJwtToken(employerCredentials.Username, credentialsCheck.Data, expires);

        return new ResponseModel<AccessTokenResponse>(StatusCodes.Status200OK, "Success!",
            new AccessTokenResponse { AccessToken = token, ExpiresAt = expires });
    }

    private string GenerateJwtToken(string username, EmployerRole? employerRole, DateTime expires)
    {
        var tokenDescriptor = BuildTokenDescriptor(username, employerRole, expires);
        return new JsonWebTokenHandler().CreateToken(tokenDescriptor);
    }

    private SecurityTokenDescriptor BuildTokenDescriptor(string username, EmployerRole? employerRole, DateTime expires)
    {
        return new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity([
                new Claim(JwtRegisteredClaimNames.Sub, username),
                new Claim(JwtRegisteredClaimNames.UniqueName, username),
                new Claim("role",
                    employerRole is { } role ? ((short)role).ToString(CultureInfo.InvariantCulture) : string.Empty),
                new Claim(JwtRegisteredClaimNames.Amr, "pwd"),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            ]),
            IssuedAt = DateTime.UtcNow,
            Expires = expires,
            SigningCredentials = new SigningCredentials(_signingKey, SecurityAlgorithms.HmacSha256Signature),
            Issuer = _authOptions.JwtIssuer,
            Audience = _authOptions.JwtAudience
        };
    }
}