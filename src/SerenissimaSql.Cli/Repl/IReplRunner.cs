namespace SerenissimaSql.Cli.Repl;

public interface IReplRunner
{
    Task RunAsync(CancellationToken ct = default);
}