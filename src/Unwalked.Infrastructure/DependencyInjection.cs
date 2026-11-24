using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NetTopologySuite;
using NetTopologySuite.Geometries;
using Npgsql;
using Unwalked.Application.Abstractions;
using Unwalked.Infrastructure.Areas;
using Unwalked.Infrastructure.Persistence;
using Unwalked.Infrastructure.Persistence.Repositories;
using Unwalked.Infrastructure.Security;
using Unwalked.Infrastructure.Time;

namespace Unwalked.Infrastructure;

/// <summary>
/// Registers the infrastructure layer: database, repositories, security and time.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Spatial reference id of WGS84, used for every geometry column.
    /// </summary>
    public const int Wgs84Srid = 4326;

    /// <summary>
    /// Adds the data source, the DbContext, repositories, the password hasher and the JWT issuer. Fails at startup when the connection string or JWT settings are missing.
    /// </summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString(UnwalkedDbContextOptions.ConnectionStringName)
            ?? throw new InvalidOperationException($"Connection string '{UnwalkedDbContextOptions.ConnectionStringName}' is not configured.");

        services.AddSingleton(_ => UnwalkedDbContextOptions.CreateDataSource(connectionString));
        services.AddDbContext<UnwalkedDbContext>((provider, options) =>
            UnwalkedDbContextOptions.Configure(options, provider.GetRequiredService<NpgsqlDataSource>()));
        services.AddSingleton(NtsGeometryServices.Instance.CreateGeometryFactory(Wgs84Srid));
        services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddSingleton(OverpassEndpoints.Parse(configuration["Areas:OverpassUrls"] ?? OverpassEndpoints.Default));
        services.AddHttpClient<IAreaPlaceSource, OverpassPlaceSource>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(100);
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Unwalked/0.2 (student diploma project)");
        });

        return services
            .AddSingleton<IClock, SystemClock>()
            .AddSingleton<BackgroundAreaLoader>()
            .AddSingleton<IAreaLoader>(provider => provider.GetRequiredService<BackgroundAreaLoader>())
            .AddScoped<IAreaRepository, AreaRepository>()
            .AddSingleton<IPasswordHasher, AspNetPasswordHasher>()
            .AddSingleton<IAccessTokenIssuer, JwtAccessTokenIssuer>()
            .AddScoped<IPlaceReadRepository, PlaceReadRepository>()
            .AddScoped<ICategoryRepository, CategoryRepository>()
            .AddScoped<IUserRepository, UserRepository>()
            .AddScoped<IShareRepository, ShareRepository>()
            .AddScoped<IFeedbackRepository, FeedbackRepository>();
    }
}
