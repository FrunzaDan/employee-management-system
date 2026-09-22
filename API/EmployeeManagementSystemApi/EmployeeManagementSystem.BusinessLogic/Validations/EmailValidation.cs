using System.Text.RegularExpressions;
using EmployeeManagementSystem.BusinessLogic.Constants;

namespace EmployeeManagementSystem.BusinessLogic.Validations;

public static partial class EmailValidation
{
    public static bool ValidateEmail(string email)
    {
        if (string.IsNullOrEmpty(email)) return false;
        var regexMatch = EmailRegex().Match(email);
        return regexMatch.Success;
    }

    [GeneratedRegex(RegexConstants.EmailRegex, RegexOptions.IgnoreCase, "en-US")]
    private static partial Regex EmailRegex();
}