using EmployeeManagementSystem.Domain.Models;

namespace EmployeeManagementSystem.BusinessLogic.Abstractions;

/// <summary>An employer's stored login data; the password check itself happens in BusinessLogic.</summary>
public sealed record EmployerAuthData(byte[] PasswordHash, byte[] PasswordSalt, EmployerRole EmployerRole);

/// <summary>Employer login data. Implemented by DataAccess (stored procedures).</summary>
public interface IEmployerRepository
{
    Task<EmployerAuthData?> GetEmployerAuthDataAsync(string username, CancellationToken cancellationToken = default);
    Task RecordEmployerLoginAsync(string username, CancellationToken cancellationToken = default);
}
