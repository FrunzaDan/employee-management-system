using EmployeeManagementSystem.BusinessLogic.Features.AuditLog;
using EmployeeManagementSystem.BusinessLogic.Features.Auth;
using EmployeeManagementSystem.BusinessLogic.Features.CostCenters;
using EmployeeManagementSystem.BusinessLogic.Features.Departments;
using EmployeeManagementSystem.BusinessLogic.Features.Employees;
using EmployeeManagementSystem.BusinessLogic.Features.Offices;
using EmployeeManagementSystem.BusinessLogic.Features.Salaries;
using Microsoft.Extensions.DependencyInjection;

namespace EmployeeManagementSystem.BusinessLogic;

public static class BusinessLogicDependencyInjection
{
    public static void AddBusinessLogic(this IServiceCollection services)
    {
        services.AddSingleton<JwtCreation>();
        services.AddScoped<GetAccessTokenHandler>();

        services.AddScoped<CreateEmployeeHandler>();
        services.AddScoped<UpdateEmployeeHandler>();
        services.AddScoped<DeactivateEmployeeHandler>();
        services.AddScoped<ReactivateEmployeeHandler>();
        services.AddScoped<DeleteEmployeeHandler>();
        services.AddScoped<GetEmployeeHandler>();
        services.AddScoped<GetEmployeesHandler>();
        services.AddScoped<ExportEmployeesHandler>();
        services.AddScoped<GetEmployeeInsightsHandler>();

        services.AddScoped<CreateEmployeeSalaryHandler>();
        services.AddScoped<GetEmployeeSalaryHistoryHandler>();

        services.AddScoped<IEmployeeAuditLogger, EmployeeAuditLogger>();
        services.AddScoped<DeleteAllEmployeeAuditLogHandler>();
        services.AddScoped<GetEmployeeAuditLogHandler>();
        services.AddScoped<GetAllEmployeeAuditLogHandler>();

        services.AddScoped<CreateOfficeHandler>();
        services.AddScoped<GetOfficeHandler>();
        services.AddScoped<GetOfficesHandler>();
        services.AddScoped<UpdateOfficeHandler>();
        services.AddScoped<DeleteOfficeHandler>();
        services.AddScoped<GetEmployeesByOfficeHandler>();

        services.AddScoped<CreateDepartmentHandler>();
        services.AddScoped<GetDepartmentHandler>();
        services.AddScoped<GetDepartmentsHandler>();
        services.AddScoped<UpdateDepartmentHandler>();
        services.AddScoped<DeleteDepartmentHandler>();
        services.AddScoped<GetEmployeesByDepartmentHandler>();

        services.AddScoped<CreateCostCenterHandler>();
        services.AddScoped<GetCostCenterHandler>();
        services.AddScoped<GetCostCentersHandler>();
        services.AddScoped<UpdateCostCenterHandler>();
        services.AddScoped<DeleteCostCenterHandler>();
        services.AddScoped<GetEmployeesByCostCenterHandler>();
    }
}
