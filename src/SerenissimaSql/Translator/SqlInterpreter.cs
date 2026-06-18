using System.Data.Common;
using SerenissimaSql.Entities;
using SerenissimaSql.Extensions;

namespace SerenissimaSql.Translator;

public class SqlInterpreter : ISqlInterpreter
{
    public async Task<SqlQueryResult> ExecuteQueryAsync(DbConnection conn, string query, CancellationToken ct = default)
    {
        var sql = query.Translate();
        var scope = await ConnectionScope.OpenAsync(conn, ct).ConfigureAwait(false);

        DbCommand? cmd = null;
        try
        {
            cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            var reader = await cmd.ExecuteReaderAsync(ct).ConfigureAwait(false);
            return new SqlQueryResult(scope, cmd, reader);
        }
        catch
        {
            if (cmd is not null)
            {
                await cmd.DisposeAsync().ConfigureAwait(false);
            }
            await scope.DisposeAsync().ConfigureAwait(false);
            throw;
        }
    }

    public async Task<int> ExecuteNonQueryAsync(DbConnection conn, string query, CancellationToken ct = default)
    {
        var sql = query.Translate();

        await using var scope = await ConnectionScope.OpenAsync(conn, ct).ConfigureAwait(false);
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        return await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
    }
}