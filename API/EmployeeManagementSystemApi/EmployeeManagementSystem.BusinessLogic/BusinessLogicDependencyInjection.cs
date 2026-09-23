using EmployeeManagementSystem.BusinessLogic.AuthFunctions;
using EmployeeManagementSystem.BusinessLogic.EmployeeFunctions;
using EmployeeManagementSystem.BusinessLogic.OrgFunctions;
using EmployeeManagementSystem.BusinessLogic.Services;
using EmployeeManagementSystem.BusinessLogic.Services.Implementation;
using EmployeeManagementSystem.DataAccess.DBConnection;
using EmployeeManagementSystem.Domain.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EmployeeManagementSystem.BusinessLogic;

public static class BusinessLogicDependencyInjection
{
    public static void AddBusinessLogic(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IEmployeeService, EmployeeService>();
        services.AddScoped<IOfficeService, OfficeService>();
        services.AddScoped<IDepartmentService, DepartmentService>();
        services.AddScoped<ICostCenterService, CostCenterService>();
        services.AddSingleton<IDbUtils, DbUtils>();
        services.AddSingleton<IAppSettingsConfig, AppSettingsConfig>();
        services.AddSingleton<JwtCreation>();

        services.AddScoped<IEmployeeAuditLogger, EmployeeAuditLogger>();
        services.AddScoped<EmployeeCreation>();
        services.AddScoped<EmployeeGetting>();
        services.AddScoped<EmployeeUpdating>();
        services.AddScoped<EmployeeActivation>();
        services.AddScoped<EmployeeDeletion>();
        services.AddScoped<EmployeeSalary>();

        services.AddScoped<OfficeFunctions>();
        services.AddScoped<DepartmentFunctions>();
        services.AddScoped<CostCenterFunctions>();
    }
}