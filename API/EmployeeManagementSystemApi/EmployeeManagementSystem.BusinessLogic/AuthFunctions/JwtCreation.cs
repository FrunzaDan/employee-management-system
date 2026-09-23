using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using EmployeeManagementSystem.DataAccess.DBConnection;
using EmployeeManagementSystem.Domain.Configuration;
using EmployeeManagementSystem.Domain.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;

namespace EmployeeManagementSystem.BusinessLogic.AuthFunctions;

public class JwtCreation
{
    private readonly IAppSettingsConfig _configuration;
    private readonly IDbUtils _dbUtils;
    private readonly SymmetricSecurityKey _signingKey;

    public JwtCreation(IAppSettingsConfig appSettingsConfig, IDbUtils dbUtils)
    {
        _dbUtils = dbUtils;
        _configuration = appSettingsConfig;
        _signingKey = JwtSigningKey.Create(_configuration.SecureJwtKey);
    }

    public async Task<ResponseModel<object>> GenerateBearerJwt(EmployerCredentials employerCredentials,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(employerCredentials.EmployerId))
            return new ResponseModel<object>(403, "Invalid or empty employer ID.");

        try
        {
            // Validate employer credentials
            var credentialsCheck = await _dbUtils.CheckEmployerCredentialsFromDb(employerCredentials, cancellationToken);

            if (credentialsCheck.Status != 200)
                return new ResponseModel<object>(403,
                    credentialsCheck.ResponseMessage);

            // Validate config before doing any signing work: BuildTokenDescriptor() would
            // otherwise call double.Parse(AccessTokenTimeout) directly and throw on a bad
            // value, making this check unreachable and wasting a signed token in the process.
            if (!double.TryParse(_configuration.AccessTokenTimeout, out var timeoutMinutes))
                return new ResponseModel<object>(500, "Invalid AccessTokenTimeout configuration.");

            // Generate token
            var token = GenerateJwtToken(employerCredentials.EmployerId, credentialsCheck.Data, timeoutMinutes);

            return new ResponseModel<object>
            {
                Status = StatusCodes.Status200OK,
                ResponseMessage = "Success!",
                Data = new AccessTokenResponse
                {
                    AccessToken = token,
                    ValidUntil = DateTime.UtcNow.AddMinutes(timeoutMinutes)
                }
            };
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception)
        {
            // Unlike the rest of the app, this endpoint is unauthenticated, and any exception
            // here is caught locally rather than bubbling to the global exception handler (whose
            // Details-only-in-Development guard wouldn't apply to this method's own response
            // anyway) — so ex.Message must never be echoed back to an anonymous caller.
            return new ResponseModel<object>(500, "An error occurred while generating the access token.");
        }
    }

    private string GenerateJwtToken(string employerId, int? employerRole, double timeoutMinutes)
    {
        var tokenDescriptor = BuildTokenDescriptor(employerId, employerRole, timeoutMinutes);
        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    private SecurityTokenDescriptor BuildTokenDescriptor(string employerId, int? employerRole, double timeoutMinutes)
    {
        return new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity([
                new Claim(ClaimTypes.Sid, employerId),
                new Claim(JwtRegisteredClaimNames.Sub, employerId),
                new Claim(ClaimTypes.Name, employerId),
                new Claim(ClaimTypes.Role, employerRole?.ToString() ?? string.Empty),
                new Claim("amr", "pwd"),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTime.UtcNow.ToString("o"))
            ]),
            Expires = DateTime.UtcNow.AddMinutes(timeoutMinutes),
            SigningCredentials = new SigningCredentials(_signingKey, SecurityAlgorithms.HmacSha256Signature),
            Issuer = _configuration.JwtIssuer,
            Audience = _configuration.JwtAudience
        };
    }
}