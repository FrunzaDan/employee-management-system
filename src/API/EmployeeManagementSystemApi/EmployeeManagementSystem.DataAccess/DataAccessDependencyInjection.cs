using EmployeeManagementSystem.BusinessLogic.Abstractions;
using EmployeeManagementSystem.DataAccess.DBConnection;
using EmployeeManagementSystem.DataAccess.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace EmployeeManagementSystem.DataAccess;

public static class DataAccessDependencyInjection
{
    /// <summary>Registers the SQL Server implementations of the abstractions BusinessLogic declares.</summary>
    public static void AddDataAccess(this IServiceCollection services)
    {
        services.AddSingleton<ISqlConnectionFactory, SqlConnectionFactory>();
        services.AddSingleton<StoredProcedureExecutor>();
        services.AddSingleton<IEmployeeRepository, EmployeeRepository>();
        services.AddSingleton<ISalaryRepository, SalaryRepository>();
        services.AddSingleton<IAuditLogRepository, AuditLogRepository>();
        services.AddSingleton<IEmployerRepository, EmployerRepository>();
        services.AddSingleton<IOfficeRepository, OfficeRepository>();
        services.AddSingleton<IDepartmentRepository, DepartmentRepository>();
        services.AddSingleton<ICostCenterRepository, CostCenterRepository>();
    }
}
