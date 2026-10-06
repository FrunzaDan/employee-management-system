namespace EmployeeManagementSystem.BusinessLogic.Contracts;

public sealed class CreateOfficeRequest
{
    public string? Name { get; set; }

    public string? City { get; set; }

    public string? Country { get; set; }
}

public sealed class UpdateOfficeRequest
{
    public Guid OfficeId { get; set; }

    public string? Name { get; set; }

    public string? City { get; set; }

    public string? Country { get; set; }
}
