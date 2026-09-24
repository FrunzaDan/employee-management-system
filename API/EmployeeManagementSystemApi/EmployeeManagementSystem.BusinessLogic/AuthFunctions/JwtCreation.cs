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

        // 401 (wrong username or password) or 403 (a role that may not sign in), with its message.
        if (credentialsCheck.Status != StatusCodes.Status200OK)
            return new ResponseModel<AccessTokenResponse>(credentialsCheck.Status, credentialsCheck.ResponseMessage);

        // One timestamp for both the token's exp claim and the ExpiresAt reported to the
        // client, so the two can't drift apart.
        var expires = DateTime.UtcNow.AddMinutes(_authOptions.AccessTokenTimeoutMinutes);
        var token = GenerateJwtToken(employerCredentials.Username, credentialsCheck.Data, expires);

        return new ResponseModel<AccessTokenResponse>(StatusCodes.Status200OK, "Success!",
            new AccessTokenResponse { AccessToken = token, ExpiresAt = expires });
    }

    private string GenerateJwtToken(string username, EmployerRole? employerRole, DateTime expires)
    {
        var tokenDescriptor = BuildTokenDescriptor(username, employerRole, expires);
        // JsonWebTokenHandler is the current IdentityModel handler (the one JwtBearer validates
        // with); it writes claim types as given, so the claims below use the short JWT names.
        return new JsonWebTokenHandler().CreateToken(tokenDescriptor);
    }

    private SecurityTokenDescriptor BuildTokenDescriptor(string username, EmployerRole? employerRole, DateTime expires)
    {
        return new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity([
                new Claim(JwtRegisteredClaimNames.Sub, username),
                // unique_name/role are mapped back to ClaimTypes.Name/Role when JwtBearer reads the token.
                new Claim(JwtRegisteredClaimNames.UniqueName, username),
                // The role claim is the numeric code ("1801"), which is what [Authorize(Roles = "1801")]
                // checks — not the enum member's name.
                new Claim("role",
                    employerRole is { } role ? ((short)role).ToString(CultureInfo.InvariantCulture) : string.Empty),
                new Claim(JwtRegisteredClaimNames.Amr, "pwd"),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            ]),
            // iat is set here rather than as a hand-built claim: RFC 7519 requires a NumericDate
            // (seconds since the Unix epoch), which the token handler writes from IssuedAt.
            IssuedAt = DateTime.UtcNow,
            Expires = expires,
            SigningCredentials = new SigningCredentials(_signingKey, SecurityAlgorithms.HmacSha256Signature),
            Issuer = _authOptions.JwtIssuer,
            Audience = _authOptions.JwtAudience
        };
    }
}