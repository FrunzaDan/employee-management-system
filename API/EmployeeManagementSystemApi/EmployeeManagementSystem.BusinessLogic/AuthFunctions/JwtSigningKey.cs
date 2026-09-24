using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace EmployeeManagementSystem.BusinessLogic.AuthFunctions;

public static class JwtSigningKey
{
    public static SymmetricSecurityKey Create(string secureJwtKey) =>
        new(Encoding.ASCII.GetBytes(secureJwtKey));
}
