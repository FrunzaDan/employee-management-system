using EmployeeManagementSystem.BusinessLogic.Abstractions;
using EmployeeManagementSystem.BusinessLogic.AuthFunctions;
using EmployeeManagementSystem.Domain.Models;
using Moq;

namespace EmployeeManagementSystem.Tests.AuthFunctions;

// Stores an employer whose password is the given one, the way the database would hand it back.
internal static class EmployerAuthSetup
{
    public static void SetupEmployer(this Mock<IDbUtils> dbUtils, string password,
        EmployerRole role = EmployerRole.Employer)
    {
        var (hash, salt) = PasswordHasher.HashPassword(password);
        dbUtils.Setup(d => d.GetEmployerAuthDataAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new EmployerAuthData(hash, salt, role));
    }
}
