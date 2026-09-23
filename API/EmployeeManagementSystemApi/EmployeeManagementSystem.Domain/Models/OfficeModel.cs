namespace EmployeeManagementSystem.Domain.Models;

public sealed record OfficeModel
{
    public required Guid OfficeId { get; init; }

    public required string Name { get; init; }

    public string? City { get; init; }

    public string? Country { get; init; }

    // Aggregates only Office_List computes (the single-office Office_Get doesn't), so they're
    // null — and omitted from the JSON — on a single office.
    public int? EmployeeCount { get; init; }

    public decimal? TotalGrossSalary { get; init; }
}

// Request shape for POST create. No OfficeId: the DB generates it.
public sealed class CreateOfficeRequest
{
    public string? Name { get; set; }

    public string? City { get; set; }

    public string? Country { get; set; }
}

// Request shape for PATCH edit: an omitted field is left unchanged (Office_Update's
// ISNULL(@param, column)).
public sealed class UpdateOfficeRequest
{
    public Guid OfficeId { get; set; }

    public string? Name { get; set; }

    public string? City { get; set; }

    public string? Country { get; set; }
}
