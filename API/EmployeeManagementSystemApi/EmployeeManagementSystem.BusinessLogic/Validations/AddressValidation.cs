using System.Text.RegularExpressions;
using EmployeeManagementSystem.BusinessLogic.Constants;
using EmployeeManagementSystem.Domain.Constants;
using EmployeeManagementSystem.Domain.Models;

namespace EmployeeManagementSystem.BusinessLogic.Validations;

public static partial class AddressValidation
{
    // Returns an error message, or null if every present field is within its DB column's length.
    public static string? ValidateLengths(AddressModel address)
    {
        if (address.Country?.Length > FieldLengthConstants.Country) return "Country is too long.";
        if (address.County?.Length > FieldLengthConstants.County) return "County is too long.";
        if (address.Town?.Length > FieldLengthConstants.Town) return "Town is too long.";
        if (address.Zip?.Length > FieldLengthConstants.Zip) return "Zip is too long.";
        // EmployeeAddress.PostalCode is VARCHAR, not NVARCHAR — a non-ASCII character would be
        // silently stored as '?', so reject it instead. Real postal codes never need one.
        if (!string.IsNullOrEmpty(address.Zip) && !ZipRegex().IsMatch(address.Zip))
            return "Zip may only contain letters, digits, spaces and hyphens.";
        if (address.Street?.Length > FieldLengthConstants.Street) return "Street is too long.";
        if (address.Number?.Length > FieldLengthConstants.Number) return "Number is too long.";
        return null;
    }

    [GeneratedRegex(RegexConstants.ZipRegex)]
    private static partial Regex ZipRegex();
}
