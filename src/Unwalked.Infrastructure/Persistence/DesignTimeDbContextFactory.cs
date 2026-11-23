using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Unwalked.Infrastructure.Persistence;

/// <summary>
/// Lets the EF Core tools create the context when generating migrations.
/// </summary>
public sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<UnwalkedDbContext>
{
    private const string DesignTimeConnectionString = "Host=localhost;Database=design_time_only";

    /// <summary>
    /// Creates a context with a placeholder connection string. The tools only read the model and never connect.
    /// </summary>
    public UnwalkedDbContext CreateDbContext(string[] args)
    {
        var builder = new DbContextOptionsBuilder<UnwalkedDbContext>();
        UnwalkedDbContextOptions.Configure(builder, DesignTimeConnectionString);
        return new UnwalkedDbContext(builder.Options);
    }
}
