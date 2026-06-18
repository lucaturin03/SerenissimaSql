using System.Text;
using Microsoft.Extensions.DependencyInjection;
using SerenissimaSql.Cli;
using SerenissimaSql.Cli.Repl;
using SerenissimaSql.Cli.TestDb;
using SerenissimaSql.Extensions;

Console.OutputEncoding = Encoding.UTF8;
Console.InputEncoding = Encoding.UTF8;

if (args is ["-h"] or ["--help"])
{
    CliOptions.PrintUsage();
    return 0;
}

var options = CliOptions.Parse(args);
if (options is null)
{
    CliOptions.PrintUsage();
    return 1;
}

if (!ProviderRegistry.TryResolve(options.Provider, out var factory))
{
    Console.Error.WriteLine($"Provider sconosciuo: '{options.Provider}'. Disponibii: {string.Join(", ", ProviderRegistry.Names)}");
    return 1;
}

var services = new ServiceCollection();
services.AddSerenissimaSql(factory, options.ConnectionString);
services.AddSingleton<IReplRunner, ReplRunner>();

await using var provider = services.BuildServiceProvider();

if (options.IsSqliteDemo)
{
    await new SqliteDatabaseInitializer(options.ConnectionString).InitializeAsync();
}

await provider.GetRequiredService<IReplRunner>().RunAsync();
return 0;
