using Npgsql;

namespace Unwalked.Infrastructure.Seeding;

public sealed class SqlScriptRunner(NpgsqlDataSource dataSource)
{
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
