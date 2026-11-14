using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Unwalked.Infrastructure.Persistence;

public sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<UnwalkedDbContext>
{
    private const string DesignTimeConnectionString = "Host=localhost;Database=design_time_only";

    public UnwalkedDbContext CreateDbContext(string[] args)
    {
        var builder = new DbContextOptionsBuilder<UnwalkedDbContext>();
        UnwalkedDbContextOptions.Configure(builder, DesignTimeConnectionString);
        return new UnwalkedDbContext(builder.Options);
    }
}
