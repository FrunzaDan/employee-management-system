namespace EmployeeManagementSystem.Domain.Models;

public class AuditLogEntry
{
    public int AuditId { get; set; }

    public Guid EmployeeGuid { get; set; }

    public string? EmployerId { get; set; }

    public string? Action { get; set; }

    public string? Details { get; set; }

    // UTC.
    public DateTime ActionDate { get; set; }
}

public class GlobalAuditLogEntry
{
    public int AuditId { get; set; }

    public Guid EmployeeGuid { get; set; }

    // Null when the employee no longer exists (usp_getAllEmployeeAuditLog LEFT
    // JOINs tbl_employees, since audit history outlives a deleted employee).
    public string? EmployeeFirstName { get; set; }

    public string? EmployeeLastName { get; set; }

    public string? EmployerId { get; set; }

    public string? Action { get; set; }

    public string? Details { get; set; }

    // UTC.
    public DateTime ActionDate { get; set; }
}
