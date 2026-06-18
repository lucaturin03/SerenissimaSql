# SerenissimaSql

> SQL the way they speak it in the Veneto dialect.

SerenissimaSql is a small, light-hearted .NET library that translates queries
written in the **Venetian dialect** into standard SQL — and, optionally, runs
them against a real database.

It is **provider-agnostic**: it is built purely on the ADO.NET abstractions
(`System.Data.Common`), so it works with any `DbProviderFactory` — SQLite, SQL
Server, PostgreSQL, MySQL/MariaDB, and so on. The package **does not** depend on
any specific provider: you bring the one you need.

---

## Table of contents

- [Features](#features)
- [Installation](#installation)
- [Quick start — translation only](#quick-start--translation-only)
- [Running queries against any database](#running-queries-against-any-database)
- [Streaming result rows](#streaming-result-rows)
- [Command-line tool (REPL)](#command-line-tool-repl)
- [Dialect dictionary](#dialect-dictionary)
- [How it works](#how-it-works)
- [Error handling](#error-handling)
- [Project layout](#project-layout)
- [Requirements](#requirements)
- [License](#license)

---

## Features

- **Venetian → SQL translator.** Turn `ciapa tuto cuanto fora da utenti` into
  `SELECT * FROM utenti`.
- **Provider-agnostic execution.** Run translated queries against any ADO.NET
  provider via `ISqlInterpreter`.
- **Zero provider lock-in.** The core library only references
  `Microsoft.Extensions.DependencyInjection.Abstractions`; you choose the
  database driver.
- **Dependency-injection friendly.** A single `AddSerenissimaSql(...)`
  extension wires everything up.
- **String literals are preserved.** Text inside single or double quotes is
  never translated, so `'Padova'` stays `'Padova'`.
- **Async, streaming results.** Read rows lazily with
  `IAsyncEnumerable<object?[]>`.
- **A ready-to-use REPL** distributed as a .NET global tool.

---

## Installation

```bash
dotnet add package SerenissimaSql
# plus the provider you actually use, e.g.:
dotnet add package Npgsql                 # PostgreSQL
# dotnet add package Microsoft.Data.Sqlite      # SQLite
# dotnet add package Microsoft.Data.SqlClient   # SQL Server
# dotnet add package MySqlConnector             # MySQL / MariaDB
```

---

## Quick start — translation only

If you only need the SQL string (for logging, inspection, or running it
yourself), call the `Translate()` extension method:

```csharp
using SerenissimaSql.Extensions;

string sql = "ciapa tuto cuanto fora da utenti ndove citta se 'Padova'".Translate();
// => SELECT * FROM utenti WHERE citta = 'Padova'
```

More examples:

```csharp
"sbati rento utenti sti qua (1, 'Bepi', 'Padova')".Translate();
// => INSERT INTO utenti VALUES (1, 'Bepi', 'Padova')

"rangia utenti meti citta se 'Verona' ndove id se 1".Translate();
// => UPDATE utenti SET citta = 'Verona' WHERE id = 1

"buta via fora da utenti ndove citta se 'Padova'".Translate();
// => DELETE FROM utenti WHERE citta = 'Padova'

"ciapa nome, conta(id) fora da utenti mucia par citta in riga par nome".Translate();
// => SELECT nome, COUNT(id) FROM utenti GROUP BY citta ORDER BY nome
```

---

## Running queries against any database

Register the services with the `DbProviderFactory` of your chosen provider and a
connection string, then resolve `ISqlInterpreter`:

```csharp
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using SerenissimaSql.ConnectionFactories;
using SerenissimaSql.Extensions;
using SerenissimaSql.Translator;

var services = new ServiceCollection();
services.AddSerenissimaSql(
    NpgsqlFactory.Instance,
    "Host=localhost;Database=test;Username=u;Password=p");

await using var provider = services.BuildServiceProvider();

var interpreter = provider.GetRequiredService<ISqlInterpreter>();
var factory = provider.GetRequiredService<IDbConnectionFactory>();

await using var conn = factory.Create();

// SELECT-style query -> returns a SqlQueryResult you can read from
await using var result = await interpreter.ExecuteQueryAsync(
    conn, "ciapa tuto cuanto fora da utenti");

Console.WriteLine(string.Join(" | ", result.Columns));
```

For statements that don't return rows (INSERT/UPDATE/DELETE/DDL), use
`ExecuteNonQueryAsync`, which returns the number of affected rows:

```csharp
int affected = await interpreter.ExecuteNonQueryAsync(
    conn, "buta via fora da utenti ndove citta se 'Padova'");
```

Both methods accept an optional `CancellationToken`.

### The public surface

| Member | Purpose |
|--------|---------|
| `string.Translate()` | Translate a Venetian query into SQL (no DB needed). |
| `IServiceCollection.AddSerenissimaSql(factory, connectionString)` | Register the interpreter and a connection factory. |
| `ISqlInterpreter.ExecuteQueryAsync(conn, query, ct)` | Translate and execute a query, returning a `SqlQueryResult`. |
| `ISqlInterpreter.ExecuteNonQueryAsync(conn, query, ct)` | Translate and execute a non-query statement; returns rows affected. |
| `IDbConnectionFactory.Create()` | Create a new `DbConnection` for the configured provider. |
| `SqlQueryResult` | Disposable wrapper exposing `Columns`, `RecordsAffected`, and the underlying `Reader`. |
| `SqlQueryResult.ReadRowsAsync(ct)` | Stream result rows as `object?[]` (DBNull is normalized to `null`). |

---

## Streaming result rows

`SqlQueryResult` exposes the column names and lets you read rows lazily:

```csharp
using SerenissimaSql.Extensions;

await using var result = await interpreter.ExecuteQueryAsync(
    conn, "ciapa tuto cuanto fora da utenti ndove citta se 'Padova'");

Console.WriteLine(string.Join(" | ", result.Columns));

await foreach (object?[] row in result.ReadRowsAsync())
{
    Console.WriteLine(string.Join(" | ", row.Select(v => v?.ToString() ?? "NULL")));
}
```

`SqlQueryResult` is `IAsyncDisposable`; the `await using` disposes the reader,
the command, and the connection scope together.

---

## Command-line tool (REPL)

A companion package, **`SerenissimaSql.Cli`**, ships an interactive REPL as a
.NET global tool named `serenissima`.

```bash
dotnet tool install --global SerenissimaSql.Cli
```

Usage:

```
serenissima [--provider <name>] [--connection <string>]

Options:
  -p, --provider <name>      DB provider (default: sqlite)
  -c, --connection <string>  Database connection string
  -h, --help                 Show help

Supported providers:
  sqlite, sqlserver/mssql, postgres/postgresql/npgsql, mysql/mariadb
```

Examples:

```bash
# SQLite demo mode: a pre-seeded `utenti` table is created automatically
serenissima

# PostgreSQL
serenissima -p postgres -c "Host=localhost;Database=test;Username=u;Password=p"

# SQL Server
serenissima -p sqlserver -c "Server=localhost;Database=test;Trusted_Connection=True;TrustServerCertificate=True"
```

When you run `serenissima` with no `--connection`, the SQLite provider starts in
**demo mode**: it creates and seeds a sample `utenti` table so you can try
queries immediately. Type queries at the `>` prompt and `moighea` to quit.

Try this in demo mode:

```
> ciapa tuto cuanto fora da utenti ndove citta se 'Padova'
id | nome | cognome | citta
1 | Bepi | Rossi | Padova
3 | Bortolo | Verdi | Padova
```

---

## Dialect dictionary

Keyword matching is **case-insensitive** and supports **multi-word phrases**
(the translator greedily matches the longest phrase first).

| Venetian | SQL |
|----------|-----|
| `ciapa` | `SELECT` |
| `tuto cuanto` | `*` |
| `fora da` | `FROM` |
| `ndove` | `WHERE` |
| `sbati rento` | `INSERT INTO` |
| `sti qua` | `VALUES` |
| `rangia` | `UPDATE` |
| `meti` | `SET` |
| `buta via` | `DELETE` |
| `tacà co` | `INNER JOIN` |
| `tacà co a zanca` | `LEFT JOIN` |
| `tacà co a drita` | `RIGHT JOIN` |
| `in riga par` | `ORDER BY` |
| `par roverso` | `DESC` |
| `mucia par` | `GROUP BY` |
| `conta` | `COUNT` |
| `sensa copie` | `DISTINCT` |
| `ciamà` | `AS` |
| `soeo` | `LIMIT` |
| `uguae a` | `LIKE` |
| `se` | `=` |
| `e` | `AND` |
| `o` | `OR` |
| `xe` | `IS` |
| `no xe` | `IS NOT` |
| `gnente` | `NULL` |
| `fa su` | `CREATE TABLE` |
| `tira zo` | `DROP TABLE` |
| `svoda` | `TRUNCATE TABLE` |
| `daghe` | `BEGIN TRANSACTION` |
| `va ben cussì` | `COMMIT` |
| `torna indrio` | `ROLLBACK` |

> **Note:** `svoda` translates to `TRUNCATE TABLE`, which is not supported by all
> databases (e.g. SQLite).

Any word that isn't a known keyword (table names, column names, numbers,
operators like `(`, `)`, `,`) is passed through unchanged.

---

## How it works

The translation pipeline has three stages:

1. **Tokenize** — `Tokenizer.Tokenize()` splits the input on whitespace. Text
   inside single (`'`) or double (`"`) quotes is captured as a single **literal**
   token and is never translated.
2. **Compose** — the translator walks the token stream and, for each position,
   greedily matches the **longest keyword phrase** (up to the longest phrase in
   the dictionary). Matched phrases are replaced with their SQL equivalent;
   everything else is emitted verbatim.
3. **Validate** — the resulting SQL must start with a recognized statement
   keyword (`SELECT`, `INSERT`, `UPDATE`, `DELETE`, `CREATE`, `DROP`,
   `TRUNCATE`, `BEGIN`, `COMMIT`, `ROLLBACK`). Otherwise a
   `NoVaUnOstregaException` is thrown.

At execution time, `SqlInterpreter` opens a connection scope, translates the
query, creates a `DbCommand`, and runs it via the provider's reader.

---

## Error handling

Translation problems raise `SerenissimaSql.Exceptions.NoVaUnOstregaException`
(roughly, *"this doesn't work, dang it"*) with a message in dialect:

```csharp
using SerenissimaSql.Exceptions;
using SerenissimaSql.Extensions;

try
{
    "ciao mama".Translate();
}
catch (NoVaUnOstregaException ex)
{
    Console.WriteLine(ex.Message);
    // "Sta roba no la xe na query bona: la scominsia co 'ciao'. ..."
}
```

- An empty/blank query throws: *"Ma cossa me scrivito? Ea query xe voda!"*
- A query that doesn't begin with a valid statement keyword throws a message
  listing the allowed starters.

Database errors (bad SQL for the target engine, connection failures, etc.)
surface as the provider's own exceptions.

---

## Project layout

```
src/
├── SerenissimaSql/             # The core library (provider-agnostic)
│   ├── Constants.cs            # The dialect dictionary & valid statement starts
│   ├── Extensions/
│   │   ├── Tokenizer.cs        # Splits input, preserves string literals
│   │   ├── Translator.cs       # Phrase matching + validation
│   │   ├── ServiceCollectionExtensions.cs   # AddSerenissimaSql(...)
│   │   └── SqlQueryResultExtensions.cs      # ReadRowsAsync(...)
│   ├── Translator/             # ISqlInterpreter / SqlInterpreter
│   ├── ConnectionFactories/    # IDbConnectionFactory
│   ├── Entities/               # Token, SqlQueryResult, ConnectionScope
│   └── Exceptions/             # NoVaUnOstregaException
│
└── SerenissimaSql.Cli/         # The `serenissima` REPL global tool (demo)
    ├── Program.cs
    ├── CliOptions.cs           # Argument parsing & usage
    ├── ProviderRegistry.cs     # Maps provider names -> DbProviderFactory
    ├── Repl/                   # Interactive prompt
    └── TestDb/                 # SQLite demo seeding
```

The CLI bundles SQLite, SQL Server, PostgreSQL, and MySQL drivers for
convenience; the **core library bundles none** — that's the point.

---

## Requirements

- .NET 10.0 or later.

---

## License

See the repository for license details.
