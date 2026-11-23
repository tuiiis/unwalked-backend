using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Unwalked.Infrastructure.Persistence;

/// <summary>
/// Shared Npgsql and EF Core setup for the API, the tools and the tests.
/// </summary>
public static class UnwalkedDbContextOptions
{
    /// <summary>
    /// Name of the connection string in configuration.
    /// </summary>
    public const string ConnectionStringName = "Unwalked";

    /// <summary>
    /// Creates a data source with PostGIS and NetTopologySuite support.
    /// </summary>
    public static NpgsqlDataSource CreateDataSource(string connectionString)
    {
        var builder = new NpgsqlDataSourceBuilder(connectionString);
        builder.UseNetTopologySuite();
        return builder.Build();
    }

    /// <summary>
    /// Configures EF Core to use the data source with snake_case names and spatial types.
    /// </summary>
    public static DbContextOptionsBuilder Configure(DbContextOptionsBuilder builder, NpgsqlDataSource dataSource) =>
        builder
            .UseNpgsql(dataSource, npgsql => npgsql.UseNetTopologySuite())
            .UseSnakeCaseNamingConvention();

    /// <summary>
    /// Same as the overload above, building the data source from a connection string.
    /// </summary>
    public static DbContextOptionsBuilder Configure(DbContextOptionsBuilder builder, string connectionString) =>
        builder
            .UseNpgsql(connectionString, npgsql => npgsql.UseNetTopologySuite())
            .UseSnakeCaseNamingConvention();
}
