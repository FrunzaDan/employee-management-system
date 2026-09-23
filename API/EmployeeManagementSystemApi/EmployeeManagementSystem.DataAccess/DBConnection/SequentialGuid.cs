using System.Runtime.InteropServices;

namespace EmployeeManagementSystem.DataAccess.DBConnection;

// Every table's clustered primary key is an app-generated UNIQUEIDENTIFIER. Guid.NewGuid()
// is fully random, so each insert lands at a random page of the clustered index (page
// splits, fragmentation). Guid.CreateVersion7() doesn't fix that for SQL Server either: it
// puts its timestamp in the first bytes, but SQL Server orders UNIQUEIDENTIFIER by the
// *last* six bytes first. This is the same scheme EF Core's SequentialGuidValueGenerator
// uses for SQL Server: random bytes, with a monotonically increasing counter (seeded from
// the clock, so it keeps increasing across restarts) written into the bytes SQL Server
// compares first — new keys append at the end of the index instead.
public static class SequentialGuid
{
    private static long _counter = DateTime.UtcNow.Ticks;

    public static Guid NewGuid()
    {
        Span<byte> guidBytes = stackalloc byte[16];
        Guid.NewGuid().TryWriteBytes(guidBytes);

        var counter = Interlocked.Increment(ref _counter);
        Span<byte> counterBytes = stackalloc byte[sizeof(long)];
        MemoryMarshal.Write(counterBytes, in counter);
        if (!BitConverter.IsLittleEndian)
            counterBytes.Reverse();

        guidBytes[08] = counterBytes[1];
        guidBytes[09] = counterBytes[0];
        guidBytes[10] = counterBytes[7];
        guidBytes[11] = counterBytes[6];
        guidBytes[12] = counterBytes[5];
        guidBytes[13] = counterBytes[4];
        guidBytes[14] = counterBytes[3];
        guidBytes[15] = counterBytes[2];

        return new Guid(guidBytes);
    }
}
