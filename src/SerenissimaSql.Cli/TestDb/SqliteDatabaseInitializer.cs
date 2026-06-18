using System.Data.Common;
using Microsoft.Data.Sqlite;

namespace SerenissimaSql.Cli.TestDb;

public sealed class SqliteDatabaseInitializer(string connectionString) : IDatabaseInitializer
{
    public async Task InitializeAsync(CancellationToken ct = default)
    {
        await using var conn = new SqliteConnection(connectionString);
        await conn.OpenAsync(ct).ConfigureAwait(false);

        await CreateDbAsync(conn, ct).ConfigureAwait(false);
        
        if (await IsEmptyAsync(conn, ct).ConfigureAwait(false))
        {
            await SeedDbAsync(conn, ct).ConfigureAwait(false);
        }
    }

    private static async Task<bool> IsEmptyAsync(DbConnection conn, CancellationToken ct = default)
    {
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT COUNT(*) FROM utenti;";
        var count = Convert.ToInt64(await cmd.ExecuteScalarAsync(ct).ConfigureAwait(false));
        return count == 0;
    }

    private static async Task CreateDbAsync(DbConnection conn, CancellationToken ct = default)
    {
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = """
                          CREATE TABLE IF NOT EXISTS utenti (
                              id INTEGER PRIMARY KEY,
                              nome TEXT NOT NULL,
                              cognome TEXT,
                              citta TEXT
                          );
                          """;
        await cmd.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
    }

    private static async Task SeedDbAsync(DbConnection conn, CancellationToken ct = default)
    {
        await using var seed = conn.CreateCommand();
        seed.CommandText = """
                           INSERT INTO utenti (id, nome, cognome, citta) VALUES
                               (1, 'Bepi', 'Rossi', 'Padova'),
                               (2, 'Toni', 'Bianchi', 'Verona'),
                               (3, 'Bortolo', 'Verdi', 'Padova');
                           """;
        await seed.ExecuteNonQueryAsync(ct).ConfigureAwait(false);
    }
}
