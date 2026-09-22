namespace EmployeeManagementSystem.BusinessLogic.EmployeeFunctions;

public interface IEmployeeAuditLogger
{
    Task Log(string employeeGuid, string employerId, string action, string? details = null,
        CancellationToken cancellationToken = default);
}
