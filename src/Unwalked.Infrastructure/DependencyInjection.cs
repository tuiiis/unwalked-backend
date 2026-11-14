using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NetTopologySuite;
using NetTopologySuite.Geometries;
using Npgsql;
using Unwalked.Application.Abstractions;
using Unwalked.Infrastructure.Persistence;
using Unwalked.Infrastructure.Persistence.Repositories;
using Unwalked.Infrastructure.Security;
using Unwalked.Infrastructure.Time;

namespace Unwalked.Infrastructure;

public static class DependencyInjection
{
    public const int Wgs84Srid = 4326;

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

        return services
            .AddSingleton<IClock, SystemClock>()
            .AddSingleton<IPasswordHasher, AspNetPasswordHasher>()
            .AddSingleton<IAccessTokenIssuer, JwtAccessTokenIssuer>()
            .AddScoped<IPlaceReadRepository, PlaceReadRepository>()
            .AddScoped<ICategoryRepository, CategoryRepository>()
            .AddScoped<IUserRepository, UserRepository>()
            .AddScoped<IShareRepository, ShareRepository>()
            .AddScoped<IFeedbackRepository, FeedbackRepository>();
    }
}
