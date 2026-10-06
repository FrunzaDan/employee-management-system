using System.Reflection;
using EmployeeManagementSystem.BusinessLogic;
using EmployeeManagementSystem.DataAccess;
using EmployeeManagementSystem.Domain.Models;
using EmployeeManagementSystem.WebAPI.Controllers;
using NetArchTest.Rules;

namespace EmployeeManagementSystem.Tests.Architecture;

// Dependencies point inward: Domain ← BusinessLogic ← DataAccess, with WebAPI composing them.
public class LayerDependencyTests
{
    private static readonly Assembly Domain = typeof(EmployeeModel).Assembly;
    private static readonly Assembly BusinessLogic = typeof(BusinessLogicDependencyInjection).Assembly;
    private static readonly Assembly DataAccess = typeof(DataAccessDependencyInjection).Assembly;
    private static readonly Assembly WebApi = typeof(ApiControllerBase).Assembly;

    private static void AssertNoDependency(Assembly assembly, params string[] forbidden)
    {
        var result = Types.InAssembly(assembly)
            .ShouldNot()
            .HaveDependencyOnAny(forbidden)
            .GetResult();

        Assert.True(result.IsSuccessful, string.Join(", ", result.FailingTypeNames ?? []));
    }

    [Fact]
    public void Domain_Should_Not_Reference_AnyOtherLayer_OrFramework() =>
        AssertNoDependency(Domain,
            "EmployeeManagementSystem.BusinessLogic",
            "EmployeeManagementSystem.DataAccess",
            "EmployeeManagementSystem.WebAPI",
            "Microsoft.AspNetCore",
            "Microsoft.Data.SqlClient");

    [Fact]
    public void BusinessLogic_Should_Not_Reference_DataAccess_WebApi_AspNetCore_OrSqlClient() =>
        AssertNoDependency(BusinessLogic,
            "EmployeeManagementSystem.DataAccess",
            "EmployeeManagementSystem.WebAPI",
            "Microsoft.AspNetCore",
            "Microsoft.Data.SqlClient");

    [Fact]
    public void DataAccess_Should_Not_Reference_WebApi_OrAspNetCore() =>
        AssertNoDependency(DataAccess,
            "EmployeeManagementSystem.WebAPI",
            "Microsoft.AspNetCore");

    [Fact]
    public void Controllers_Should_Not_Reference_DataAccess_OrSqlClient()
    {
        var result = Types.InAssembly(WebApi)
            .That()
            .ResideInNamespace("EmployeeManagementSystem.WebAPI.Controllers")
            .ShouldNot()
            .HaveDependencyOnAny("EmployeeManagementSystem.DataAccess", "Microsoft.Data.SqlClient")
            .GetResult();

        Assert.True(result.IsSuccessful, string.Join(", ", result.FailingTypeNames ?? []));
    }
}
