namespace EmployeeManagementSystem.BusinessLogic.EmployeeFunctions;

public interface IEmployeeAuditLogger
{
    Task Log(Guid employeeGuid, string employerId, string action, string? details = null,
        CancellationToken cancellationToken = default);
}
