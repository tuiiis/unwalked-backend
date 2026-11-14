using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Unwalked.Infrastructure.Persistence;

internal static class PostgresErrors
{
    public static bool IsUniqueViolation(DbUpdateException exception) =>
        exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation };
}
