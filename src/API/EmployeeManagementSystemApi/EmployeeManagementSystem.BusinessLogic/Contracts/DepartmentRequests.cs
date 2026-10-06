namespace EmployeeManagementSystem.BusinessLogic.Contracts;

public sealed class CreateDepartmentRequest
{
    public string? Name { get; set; }
}

public sealed class UpdateDepartmentRequest
{
    public Guid DepartmentId { get; set; }

    public string? Name { get; set; }
}
