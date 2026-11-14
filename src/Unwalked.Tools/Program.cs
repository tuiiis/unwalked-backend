using System.Globalization;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite;
using Unwalked.Infrastructure;
using Unwalked.Infrastructure.Loading;
using Unwalked.Infrastructure.Persistence;
using Unwalked.Infrastructure.Seeding;

const string ConnectionStringVariable = "ConnectionStrings__Unwalked";
const string Usage = """
    Usage:
      unwalked-tools migrate
      unwalked-tools seed <directory>
      unwalked-tools load-places <region-code> <csv-path>
    The connection string is read from ConnectionStrings__Unwalked.
    """;

if (args.Length == 0)
{
    Console.Error.WriteLine(Usage);
    return 2;
}

using var cancellation = new CancellationTokenSource();
Console.CancelKeyPress += (_, eventArgs) =>
{
    eventArgs.Cancel = true;
    cancellation.Cancel();
};

string RequireConnectionString() =>
    Environment.GetEnvironmentVariable(ConnectionStringVariable)
    ?? throw new InvalidOperationException($"{ConnectionStringVariable} is not set.");

switch (args[0])
{
    case "migrate":
    {
        var options = new DbContextOptionsBuilder<UnwalkedDbContext>();
        UnwalkedDbContextOptions.Configure(options, RequireConnectionString());
        await using var context = new UnwalkedDbContext(options.Options);
        var pending = (await context.Database.GetPendingMigrationsAsync(cancellation.Token)).ToList();
        await context.Database.MigrateAsync(cancellation.Token);
        Console.WriteLine($"Applied {pending.Count} migration(s): {string.Join(", ", pending)}");
        return 0;
    }

    case "seed" when args.Length == 2:
    {
        await using var dataSource = UnwalkedDbContextOptions.CreateDataSource(RequireConnectionString());
        var applied = await new SqlScriptRunner(dataSource).RunDirectoryAsync(args[1], cancellation.Token);
        Console.WriteLine($"Applied {applied.Count} seed script(s): {string.Join(", ", applied)}");
        return 0;
    }

    case "load-places" when args.Length == 3:
    {
        await using var dataSource = UnwalkedDbContextOptions.CreateDataSource(RequireConnectionString());
        var geometryFactory = NtsGeometryServices.Instance.CreateGeometryFactory(DependencyInjection.Wgs84Srid);
        using var csv = File.OpenText(args[2]);
        var report = await new PlaceBulkLoader(dataSource, geometryFactory).LoadAsync(args[1], csv, cancellation.Token);
        Console.WriteLine(
            $"Loaded {report.Places} places, {report.AlternateLinks} alternate category links, {report.Categories} categories, " +
            $"skipped {report.SkippedRows} rows in {report.Elapsed.TotalSeconds.ToString("F2", CultureInfo.InvariantCulture)} s");
        return 0;
    }

    default:
        Console.Error.WriteLine(Usage);
        return 2;
}
