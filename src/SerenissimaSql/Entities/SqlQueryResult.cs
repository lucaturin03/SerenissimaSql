using System.Data.Common;

namespace SerenissimaSql.Entities;

public sealed class SqlQueryResult : IAsyncDisposable
{
    private readonly DbCommand _cmd;
    public readonly DbDataReader Reader;
    private readonly ConnectionScope _scope;

    internal SqlQueryResult(ConnectionScope scope, DbCommand cmd, DbDataReader reader)
    {
        _scope = scope;
        _cmd = cmd;
        Reader = reader;

        var names = new string[reader.FieldCount];
        for (var i = 0; i < names.Length; i++)
        {
            names[i] = reader.GetName(i);
        }
        
        Columns = names;
    }

    public IReadOnlyList<string> Columns { get; }
    
    public int RecordsAffected => Reader.RecordsAffected;

    public async ValueTask DisposeAsync()
    {
        await Reader.DisposeAsync().ConfigureAwait(false);
        await _cmd.DisposeAsync().ConfigureAwait(false);
        await _scope.DisposeAsync().ConfigureAwait(false);
    }
}