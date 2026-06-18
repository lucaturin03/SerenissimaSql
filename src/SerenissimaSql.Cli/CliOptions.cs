namespace SerenissimaSql.Cli;

public sealed class CliOptions
{
    private const string DefaultSqliteConnection = "Data Source=serenissima.db";

    public required string Provider { get; init; }
    public required string ConnectionString { get; init; }
    public required bool IsSqliteDemo { get; init; }
    
    public static CliOptions? Parse(string[] args)
    {
        string? provider = null;
        string? connection = null;

        for (var i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "-h" or "--help":
                    return null;
                case "-p" or "--provider" when i + 1 < args.Length:
                    provider = args[++i];
                    break;
                case "-c" or "--connection" when i + 1 < args.Length:
                    connection = args[++i];
                    break;
                default:
                    Console.Error.WriteLine($"Argomento no riconossuo: '{args[i]}'");
                    return null;
            }
        }

        provider ??= "sqlite";
        var isSqlite = string.Equals(provider, "sqlite", StringComparison.OrdinalIgnoreCase);

        if (connection is null)
        {
            if (!isSqlite)
            {
                Console.Error.WriteLine($"Par el provider '{provider}' ghe vol na connection string (--connection).");
                return null;
            }

            connection = DefaultSqliteConnection;
        }

        return new CliOptions
        {
            Provider = provider,
            ConnectionString = connection,
            IsSqliteDemo = isSqlite && connection == DefaultSqliteConnection,
        };
    }

    public static void PrintUsage()
    {
        Console.WriteLine("""
            SerenissimaSQL - SQL come che se parla in dialetto Veneto

            Uso:
              serenissima [--provider <nome>] [--connection <stringa>]

            Opzioni:
              -p, --provider <nome>      Provider DB (default: sqlite)
              -c, --connection <stringa> Connection string del database
              -h, --help                 Mostra sto aiuto

            Provider supportai:
              sqlite, sqlserver/mssql, postgres/postgresql/npgsql, mysql/mariadb

            Esempi:
              serenissima
              serenissima -p postgres -c "Host=localhost;Database=test;Username=u;Password=p"
              serenissima -p sqlserver -c "Server=localhost;Database=test;Trusted_Connection=True;TrustServerCertificate=True"

            Sensa --connection, el provider sqlite el parte in modalità demo (tabella 'utenti' za piena).
            Scrivi 'moighea' par sortir.
            """);
    }
}
