namespace SerenissimaSql.Repl;

public interface IReplRunner
{
    Task RunAsync(CancellationToken ct = default);
}