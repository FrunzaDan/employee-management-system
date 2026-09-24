namespace EmployeeManagementSystem.Domain.Models;

public sealed record OfficeModel
{
    public required Guid OfficeId { get; init; }

    public required string Name { get; init; }

    public string? City { get; init; }

    public string? Country { get; init; }

    public int? EmployeeCount { get; init; }

    public decimal? TotalGrossSalary { get; init; }
}

public sealed class CreateOfficeRequest
{
    public string? Name { get; set; }

    public string? City { get; set; }

    public string? Country { get; set; }
}

public sealed class UpdateOfficeRequest
{
    public Guid OfficeId { get; set; }

    public string? Name { get; set; }

    public string? City { get; set; }

    public string? Country { get; set; }
}
