namespace SerenissimaSql;

public static class Constants
{
    public static readonly Dictionary<string, string> Keywords =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ["ciapa"] = "SELECT",
            ["buta via"] = "DELETE",
            ["rangia"] = "UPDATE",
            ["sbati rento"] = "INSERT INTO",
            ["tuto cuanto"] = "*",
            ["fora da"] = "FROM",
            ["tacà co"] = "INNER JOIN",
            ["ndove"] = "WHERE",
            ["meti"] = "SET",
            ["se"] = "=",
            ["sti qua"] = "VALUES",
            ["in riga par"] = "ORDER BY",
            ["e"] = "AND",
            ["o"] = "OR",
            ["no xe"] = "IS NOT",
            ["xe"] = "IS",
            ["gnente"] = "NULL",
            ["daghe"] = "BEGIN TRANSACTION",
            ["va ben cussì"] = "COMMIT",
            ["torna indrio"] = "ROLLBACK",
            ["fa su"] = "CREATE TABLE",
            ["tira zo"] = "DROP TABLE",
            ["svoda"] = "TRUNCATE TABLE",
            ["par roverso"] = "DESC",
            ["mucia par"] = "GROUP BY",
            ["soeo"] = "LIMIT",
            ["ciamà"] = "AS",
            ["conta"] = "COUNT",
            ["sensa copie"] = "DISTINCT",
            ["tacà co a zanca"] = "LEFT JOIN",
            ["tacà co a drita"] = "RIGHT JOIN",
            ["uguae a"] = "LIKE"
        };
    
    public static readonly HashSet<string> ValidStarts =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "SELECT", "INSERT", "UPDATE", "DELETE", "CREATE", "DROP", "TRUNCATE", "BEGIN", "COMMIT", "ROLLBACK"
        };
    
    public static readonly int MaxPhraseWords = Keywords.Keys.Max(k => k.Split(' ').Length);
}