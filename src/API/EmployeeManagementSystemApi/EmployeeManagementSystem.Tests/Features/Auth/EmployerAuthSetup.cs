using EmployeeManagementSystem.BusinessLogic.Abstractions;
using EmployeeManagementSystem.BusinessLogic.Features.Auth;
using EmployeeManagementSystem.Domain.Models;
using Moq;

namespace EmployeeManagementSystem.Tests.Features.Auth;

// Stores an employer whose password is the given one, the way the database would hand it back.
internal static class EmployerAuthSetup
{
    public static void SetupEmployer(this Mock<IEmployerRepository> employers, string password,
        EmployerRole role = EmployerRole.Employer)
    {
        var (hash, salt) = PasswordHasher.HashPassword(password);
        employers.Setup(d => d.GetEmployerAuthDataAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EmployerAuthData(hash, salt, role));
    }
}
