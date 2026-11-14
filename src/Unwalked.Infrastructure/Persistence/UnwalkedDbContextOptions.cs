using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace Unwalked.Infrastructure.Persistence;

public static class UnwalkedDbContextOptions
{
    public const string ConnectionStringName = "Unwalked";

    public static NpgsqlDataSource CreateDataSource(string connectionString)
    {
        var builder = new NpgsqlDataSourceBuilder(connectionString);
        builder.UseNetTopologySuite();
        return builder.Build();
    }

    public static DbContextOptionsBuilder Configure(DbContextOptionsBuilder builder, NpgsqlDataSource dataSource) =>
        builder
            .UseNpgsql(dataSource, npgsql => npgsql.UseNetTopologySuite())
            .UseSnakeCaseNamingConvention();

    public static DbContextOptionsBuilder Configure(DbContextOptionsBuilder builder, string connectionString) =>
        builder
            .UseNpgsql(connectionString, npgsql => npgsql.UseNetTopologySuite())
            .UseSnakeCaseNamingConvention();
}
