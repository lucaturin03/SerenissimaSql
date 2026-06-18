using System.Data.Common;
using SerenissimaSql.Entities;

namespace SerenissimaSql.Translator;

public interface ISqlInterpreter
{
    Task<SqlQueryResult> ExecuteQueryAsync(DbConnection conn, string query, CancellationToken ct = default);

    Task<int> ExecuteNonQueryAsync(DbConnection conn, string query, CancellationToken ct = default);
}