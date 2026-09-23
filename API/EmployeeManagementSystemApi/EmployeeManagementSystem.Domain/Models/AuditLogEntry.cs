namespace EmployeeManagementSystem.Domain.Models;

public sealed record AuditLogEntry
{
    public required int EmployeeAuditLogId { get; init; }

    public required Guid EmployeeId { get; init; }

    public required string PerformedBy { get; init; }

    public required AuditAction ActionType { get; init; }

    public string? Details { get; init; }

    // UTC.
    public required DateTime OccurredAt { get; init; }
}

public sealed record GlobalAuditLogEntry
{
    public required int EmployeeAuditLogId { get; init; }

    public required Guid EmployeeId { get; init; }

    // Null when the employee no longer exists (EmployeeAuditLog_List LEFT
    // JOINs Employee, since audit history outlives a deleted employee).
    public string? EmployeeFirstName { get; init; }

    public string? EmployeeLastName { get; init; }

    public required string PerformedBy { get; init; }

    public required AuditAction ActionType { get; init; }

    public string? Details { get; init; }

    // UTC.
    public required DateTime OccurredAt { get; init; }
}
