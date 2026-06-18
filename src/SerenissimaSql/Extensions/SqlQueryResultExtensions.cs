using System.Runtime.CompilerServices;
using SerenissimaSql.Entities;

namespace SerenissimaSql.Extensions;

public static class SqlQueryResultExtensions
{
    public static async IAsyncEnumerable<object?[]> ReadRowsAsync(this SqlQueryResult result, [EnumeratorCancellation] CancellationToken ct = default)
    {
        var fieldCount = result.Reader.FieldCount;
        while (await result.Reader.ReadAsync(ct).ConfigureAwait(false))
        {
            var row = new object?[fieldCount];
            result.Reader.GetValues(row!);
            for (var i = 0; i < fieldCount; i++)
            {
                if (row[i] == DBNull.Value)
                {
                    row[i] = null;
                }
            }

            yield return row;
        }
    }
}