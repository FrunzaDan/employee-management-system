namespace EmployeeManagementSystem.Domain.Models;

public sealed class AccessTokenResponse
{
    public string? AccessToken { get; set; }

    public DateTime? ValidUntil { get; set; }
}