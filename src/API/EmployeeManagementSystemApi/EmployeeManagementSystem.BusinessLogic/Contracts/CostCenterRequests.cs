namespace EmployeeManagementSystem.BusinessLogic.Contracts;

public sealed class CreateCostCenterRequest
{
    public string? Code { get; set; }

    public string? Name { get; set; }
}

public sealed class UpdateCostCenterRequest
{
    public Guid CostCenterId { get; set; }

    public string? Code { get; set; }

    public string? Name { get; set; }
}
