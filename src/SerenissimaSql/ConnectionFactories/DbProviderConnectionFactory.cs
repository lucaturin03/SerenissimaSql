using System.Data.Common;

namespace SerenissimaSql.ConnectionFactories;

public sealed class DbProviderConnectionFactory(DbProviderFactory providerFactory,
    string connectionString) : IDbConnectionFactory
{
    public DbConnection Create()
    {
        var connection = providerFactory.CreateConnection()
                         ?? throw new InvalidOperationException("El provider no'l ga tornà na connession");

        connection.ConnectionString = connectionString;
        return connection;
    }
}