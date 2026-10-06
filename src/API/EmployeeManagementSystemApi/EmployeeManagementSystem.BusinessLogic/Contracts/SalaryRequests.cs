namespace EmployeeManagementSystem.BusinessLogic.Contracts;

public sealed class CreateSalaryRequest
{
    public Guid EmployeeId { get; set; }

    public decimal? GrossSalary { get; set; }

    public DateOnly? EffectiveDate { get; set; }
}
