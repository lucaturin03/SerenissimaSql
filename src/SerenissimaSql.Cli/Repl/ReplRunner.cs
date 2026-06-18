using SerenissimaSql.ConnectionFactories;
using SerenissimaSql.Entities;
using SerenissimaSql.Exceptions;
using SerenissimaSql.Extensions;
using SerenissimaSql.Translator;

namespace SerenissimaSql.Repl;

public sealed class ReplRunner(IDbConnectionFactory connectionFactory, ISqlInterpreter interpreter) : IReplRunner
{
    public async Task RunAsync(CancellationToken ct = default)
    {
        Console.WriteLine("SerenissimaSQL: SQL come che se parla dialetto Veneto");

        await using var conn = connectionFactory.Create();
        await conn.OpenAsync(ct).ConfigureAwait(false);

        while (true)
        {
            Console.Write("> ");
            var line = Console.ReadLine();
            if (line is null || line.Trim().Equals("moighea", StringComparison.OrdinalIgnoreCase))
            {
                break;
            }
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            try
            {
                await using var result = await interpreter.ExecuteQueryAsync(conn, line, ct);
                await PrintQueryAsync(result, ct).ConfigureAwait(false);
            }
            catch (NoVaUnOstregaException ex)
            {
                Console.WriteLine("Errore > " + ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Database > " + ex.Message);
            }
        }
    }

    private static async Task PrintQueryAsync(SqlQueryResult execution, CancellationToken ct)
    {
        if (execution.Columns.Count == 0)
        {
            Console.WriteLine($"OK > {execution.RecordsAffected} righe tocàe");
            return;
        }
        
        Console.WriteLine(string.Join(" | ", execution.Columns));
        await foreach (var row in execution.ReadRowsAsync(ct).ConfigureAwait(false))
        {
            Console.WriteLine(string.Join(" | ", row.Select(v => v?.ToString() ?? "NULL")));
        }
    }
}