using System.Data;
using System.Data.Common;

namespace SerenissimaSql.Entities;

public readonly struct ConnectionScope(DbConnection connection, bool ownsConnection) : IAsyncDisposable
{
    public static async ValueTask<ConnectionScope> OpenAsync(DbConnection connection, CancellationToken ct)
    {
        var wasClosed = connection.State == ConnectionState.Closed;
        if (wasClosed)
        {
            await connection.OpenAsync(ct).ConfigureAwait(false);
        }

        return new ConnectionScope(connection, wasClosed);
    }

    public async ValueTask DisposeAsync()
    {
        if (ownsConnection)
        {
            await connection.CloseAsync().ConfigureAwait(false);
        }
    }
}