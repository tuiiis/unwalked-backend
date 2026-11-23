using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Unwalked.Infrastructure.Persistence;

/// <summary>
/// Recognises PostgreSQL errors that the repositories turn into domain results.
/// </summary>
internal static class PostgresErrors
{
    /// <summary>
    /// Whether the save failed on a unique index, SQLSTATE 23505.
    /// </summary>
    public static bool IsUniqueViolation(DbUpdateException exception) =>
        exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation };
}
