namespace SerenissimaSql.TestDb;

public interface IDatabaseInitializer
{
    Task InitializeAsync(CancellationToken ct = default);
}