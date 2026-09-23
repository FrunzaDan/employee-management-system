namespace EmployeeManagementSystem.Domain.Models;

public class OfficeModel
{
    public string? Guid { get; set; }
    public string? OfficeName { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public int EmployeeCount { get; set; }
    public decimal TotalBruttoSalary { get; set; }
}
