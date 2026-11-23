using Npgsql;

namespace Unwalked.Infrastructure.Seeding;

/// <summary>
/// Runs seed scripts against the database.
/// </summary>
public sealed class SqlScriptRunner(NpgsqlDataSource dataSource)
{
    /// <summary>
    /// Runs every .sql file in the directory in name order inside one transaction and returns the file names.
    /// </summary>
    public async Task<IReadOnlyList<string>> RunDirectoryAsync(string directory, CancellationToken cancellationToken)
    {
        var scripts = Directory.GetFiles(directory, "*.sql").Order(StringComparer.Ordinal).ToList();

        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);
        foreach (var script in scripts)
        {
            await using var command = new NpgsqlCommand(await File.ReadAllTextAsync(script, cancellationToken), connection, transaction);
            await command.ExecuteNonQueryAsync(cancellationToken);
        }

        await transaction.CommitAsync(cancellationToken);
        return scripts.Select(Path.GetFileName).ToList()!;
    }
}
