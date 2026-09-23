using System.Data.SqlTypes;
using EmployeeManagementSystem.DataAccess.DBConnection;

namespace EmployeeManagementSystem.Tests.DataAccess;

public class SequentialGuidTests
{
    [Fact]
    public void NewGuid_ProducesValuesThatAscendInSqlServerSortOrder()
    {
        // SqlGuid compares the way SQL Server orders UNIQUEIDENTIFIER (last byte group first),
        // which is the order the clustered primary keys are stored in.
        var guids = Enumerable.Range(0, 1000).Select(_ => new SqlGuid(SequentialGuid.NewGuid())).ToList();

        for (var i = 1; i < guids.Count; i++)
            Assert.True(guids[i - 1].CompareTo(guids[i]) < 0, $"GUID {i} did not sort after GUID {i - 1}.");
    }

    [Fact]
    public void NewGuid_ProducesDistinctNonEmptyValues()
    {
        var guids = Enumerable.Range(0, 1000).Select(_ => SequentialGuid.NewGuid()).ToList();

        Assert.DoesNotContain(Guid.Empty, guids);
        Assert.Equal(guids.Count, guids.Distinct().Count());
    }
}
