using System.Data.Common;
using Microsoft.Extensions.DependencyInjection;
using SerenissimaSql.ConnectionFactories;
using SerenissimaSql.Translator;

namespace SerenissimaSql.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddSerenissimaSql(this IServiceCollection services,
        DbProviderFactory providerFactory, string connectionString)
    {
        services.AddSingleton<IDbConnectionFactory>(
            _ => new DbProviderConnectionFactory(providerFactory, connectionString));
        services.AddSingleton<ISqlInterpreter, SqlInterpreter>();
        return services;
    }
}
