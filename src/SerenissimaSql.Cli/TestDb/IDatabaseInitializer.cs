namespace SerenissimaSql.Cli.TestDb;

public interface IDatabaseInitializer
{
    Task InitializeAsync(CancellationToken ct = default);
}