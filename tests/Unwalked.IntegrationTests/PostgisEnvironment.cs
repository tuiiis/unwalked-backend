using Microsoft.EntityFrameworkCore;
using NetTopologySuite;
using Npgsql;
using Unwalked.Infrastructure;
using Unwalked.Infrastructure.Loading;
using Unwalked.Infrastructure.Persistence;
using Unwalked.Infrastructure.Seeding;
using Testcontainers.PostgreSql;

namespace Unwalked.IntegrationTests;

[SetUpFixture]
public sealed class PostgisEnvironment
{
    public const string PostgisImage = "imresamu/postgis:17-3.5";
    private const string Database = "unwalked";
    private const string MigratorPassword = "migrator-test-password";
    private const string ApiPassword = "api-test-password";
    private const string AnalystPassword = "analyst-test-password";

    private static PostgreSqlContainer? container;

    public static string MigratorConnectionString { get; private set; } = string.Empty;

    public static string ApiConnectionString { get; private set; } = string.Empty;

    public static string AnalystConnectionString { get; private set; } = string.Empty;

    public static string SeedDirectory(string name) => Path.Combine(TestContext.CurrentContext.TestDirectory, "db", "seed", name);

    [OneTimeSetUp]
    public async Task StartDatabase()
    {
        var rolesScript = Path.Combine(TestContext.CurrentContext.TestDirectory, "db", "01-roles.sh");

        container = new PostgreSqlBuilder(Environment.GetEnvironmentVariable("POSTGIS_IMAGE") ?? PostgisImage)
            .WithDatabase(Database)
            .WithEnvironment("MIGRATOR_PASSWORD", MigratorPassword)
            .WithEnvironment("API_PASSWORD", ApiPassword)
            .WithEnvironment("ANALYST_PASSWORD", AnalystPassword)
            .WithResourceMapping(new FileInfo(rolesScript), "/docker-entrypoint-initdb.d/")
            .Build();
        await container.StartAsync();

        var superuser = new NpgsqlConnectionStringBuilder(container.GetConnectionString());
        MigratorConnectionString = WithLogin(superuser, "unwalked_migrator", MigratorPassword);
        ApiConnectionString = WithLogin(superuser, "unwalked_api", ApiPassword);
        AnalystConnectionString = WithLogin(superuser, "unwalked_analyst", AnalystPassword);

        await WaitUntilAcceptingAsync(MigratorConnectionString);
        await MigrateAsync();
        await SeedReferenceDataAsync();
        await LoadFixturePlacesAsync();
    }

    [OneTimeTearDown]
    public async Task StopDatabase()
    {
        if (container is not null)
        {
            await container.DisposeAsync();
        }
    }

    private static string WithLogin(NpgsqlConnectionStringBuilder template, string username, string password) =>
        new NpgsqlConnectionStringBuilder(template.ConnectionString) { Username = username, Password = password }.ConnectionString;

    private static async Task WaitUntilAcceptingAsync(string connectionString)
    {
        for (var attempt = 1; ; attempt++)
        {
            try
            {
                await using var connection = new NpgsqlConnection(connectionString);
                await connection.OpenAsync();
                return;
            }
            catch (NpgsqlException) when (attempt < 30)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(500));
            }
        }
    }

    private static async Task MigrateAsync()
    {
        var options = new DbContextOptionsBuilder<UnwalkedDbContext>();
        UnwalkedDbContextOptions.Configure(options, MigratorConnectionString);
        await using var context = new UnwalkedDbContext(options.Options);
        await context.Database.MigrateAsync();
    }

    private static async Task SeedReferenceDataAsync()
    {
        await using var dataSource = NpgsqlDataSource.Create(MigratorConnectionString);
        await new SqlScriptRunner(dataSource).RunDirectoryAsync(SeedDirectory("reference"), CancellationToken.None);
    }

    private static async Task LoadFixturePlacesAsync()
    {
        await using var dataSource = UnwalkedDbContextOptions.CreateDataSource(MigratorConnectionString);
        var geometryFactory = NtsGeometryServices.Instance.CreateGeometryFactory(DependencyInjection.Wgs84Srid);
        using var csv = new StringReader(FixturePlaces.Csv);
        await new PlaceBulkLoader(dataSource, geometryFactory).LoadAsync(FixturePlaces.Region, csv, CancellationToken.None);
    }
}
