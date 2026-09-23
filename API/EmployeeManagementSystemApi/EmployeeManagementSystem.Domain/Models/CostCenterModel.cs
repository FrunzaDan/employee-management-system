namespace EmployeeManagementSystem.Domain.Models;

public class CostCenterModel
{
    public string? Guid { get; set; }
    public string? CostCenterCode { get; set; }
    public string? CostCenterName { get; set; }
    public int EmployeeCount { get; set; }
    public decimal TotalBruttoSalary { get; set; }
}
