namespace EmployeeManagementSystem.Domain.Models;

public class CostCenterModel
{
    public Guid? Guid { get; set; }
    public string? CostCenterCode { get; set; }
    public string? CostCenterName { get; set; }
    public int EmployeeCount { get; set; }
    public decimal TotalBruttoSalary { get; set; }
}
