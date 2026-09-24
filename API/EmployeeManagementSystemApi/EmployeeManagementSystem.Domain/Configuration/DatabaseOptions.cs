using System.ComponentModel.DataAnnotations;

namespace EmployeeManagementSystem.Domain.Configuration;

public sealed class DatabaseOptions
{
    public const string SectionName = "ConnectionStrings";

    [Required]
    public string Docker { get; set; } = string.Empty;

    public string? LocalSqlServer { get; set; }
}
