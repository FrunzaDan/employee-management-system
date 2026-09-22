using Microsoft.Extensions.Configuration;

namespace EmployeeManagementSystem.Domain.Configuration;

public class AppSettingsConfig(IConfiguration configuration) : IAppSettingsConfig
{
    public string SecureJwtKey => configuration["Auth:SecureJWTKey"] ??
                                  throw new ArgumentNullException(nameof(SecureJwtKey),
                                      "The config value SecureJWTKey cannot be null.");

    public string JwtIssuer => configuration["Auth:JWTIssuer"] ??
                               throw new ArgumentNullException(nameof(JwtIssuer),
                                   "The config value JWTIssuer cannot be null.");

    public string JwtAudience => configuration["Auth:JWTAudience"] ??
                                 throw new ArgumentNullException(nameof(JwtAudience),
                                     "The config value JWTAudience cannot be null.");

    public string AccessTokenTimeout => configuration["Auth:AccessTokenTimeout"] ??
                                        throw new ArgumentNullException(nameof(AccessTokenTimeout),
                                            "The config value AccessTokenTimeout cannot be null.");

    public string EmployeeManagementSystemDbWindows =>
        configuration["ConnectionStrings:EmployeeManagementSystemDB_Windows"] ?? throw new ArgumentNullException(
            nameof(EmployeeManagementSystemDbWindows),
            "The config value EmployeeManagementSystemDB_Windows cannot be null.");

    public string EmployeeManagementSystemDbDocker =>
        configuration["ConnectionStrings:EmployeeManagementSystemDB_Docker"] ?? throw new ArgumentNullException(
            nameof(EmployeeManagementSystemDbDocker),
            "The config value EmployeeManagementSystemDB_Docker cannot be null.");
}