namespace EmployeeManagementSystem.Domain.Models;

public class DepartmentModel
{
    public Guid? Guid { get; set; }
    public string? DepartmentName { get; set; }
    public int EmployeeCount { get; set; }
    public decimal TotalBruttoSalary { get; set; }
}
