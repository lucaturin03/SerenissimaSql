using System.Data.Common;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using MySqlConnector;
using Npgsql;

namespace SerenissimaSql.Cli;

public static class ProviderRegistry
{
    private static readonly Dictionary<string, DbProviderFactory> Factories =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["sqlite"] = SqliteFactory.Instance,
            ["sqlserver"] = SqlClientFactory.Instance,
            ["mssql"] = SqlClientFactory.Instance,
            ["postgres"] = NpgsqlFactory.Instance,
            ["postgresql"] = NpgsqlFactory.Instance,
            ["npgsql"] = NpgsqlFactory.Instance,
            ["mysql"] = MySqlConnectorFactory.Instance,
            ["mariadb"] = MySqlConnectorFactory.Instance,
        };

    public static IReadOnlyCollection<string> Names => Factories.Keys;

    public static bool TryResolve(string name, out DbProviderFactory factory) =>
        Factories.TryGetValue(name, out factory!);
}
