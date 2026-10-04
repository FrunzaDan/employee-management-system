namespace EmployeeManagementSystem.Domain.Models;

public sealed record AuditLogEntry
{
    public required int EmployeeAuditLogId { get; init; }

    public required Guid EmployeeId { get; init; }

    public required string PerformedBy { get; init; }

    public required AuditAction ActionType { get; init; }

    public string? Details { get; init; }

    public required DateTime OccurredAt { get; init; }
}

public sealed record GlobalAuditLogEntry
{
    public required int EmployeeAuditLogId { get; init; }

    public required Guid EmployeeId { get; init; }

    public string? EmployeeFirstName { get; init; }

    public string? EmployeeLastName { get; init; }

    public required string PerformedBy { get; init; }

    public required AuditAction ActionType { get; init; }

    public string? Details { get; init; }

    public required DateTime OccurredAt { get; init; }
}
