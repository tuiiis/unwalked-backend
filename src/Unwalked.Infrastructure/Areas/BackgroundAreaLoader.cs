using System.Collections.Concurrent;
using H3;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Unwalked.Application.Abstractions;

namespace Unwalked.Infrastructure.Areas;

/// <summary>
/// Loads areas on background tasks inside the API process. A cell is loaded at most once at a time, failures are remembered so the service can ask clients to wait.
/// </summary>
public sealed partial class BackgroundAreaLoader(IServiceScopeFactory scopes, IClock clock, IHostApplicationLifetime lifetime, ILogger<BackgroundAreaLoader> logger)
    : IAreaLoader
{
    private readonly ConcurrentDictionary<ulong, Task> running = new();
    private readonly ConcurrentDictionary<ulong, DateTimeOffset> failures = new();
    private readonly ConcurrentQueue<DateTimeOffset> starts = new();

    /// <summary>
    /// True while the cell is being loaded.
    /// </summary>
    public bool IsLoading(H3Index cell) => running.ContainsKey((ulong)cell);

    /// <summary>
    /// When the last load of the cell failed, or null.
    /// </summary>
    public DateTimeOffset? FailedAt(H3Index cell) => failures.TryGetValue((ulong)cell, out var at) ? at : null;

    /// <summary>
    /// Number of loads started since the given time.
    /// </summary>
    public int StartedSince(DateTimeOffset since)
    {
        while (starts.TryPeek(out var first) && first < since.AddHours(-1))
        {
            starts.TryDequeue(out _);
        }

        return starts.Count(start => start >= since);
    }

    /// <summary>
    /// Starts loading the cell unless a load is already running.
    /// </summary>
    public void Start(H3Index cell)
    {
        var key = (ulong)cell;
        var added = false;
        running.GetOrAdd(key, _ =>
        {
            added = true;
            return Task.Run(() => LoadAsync(cell), lifetime.ApplicationStopping);
        });
        if (added)
        {
            starts.Enqueue(clock.UtcNow);
        }
    }

    /// <summary>
    /// Waits for the running load of the cell, if any. Used by tests.
    /// </summary>
    public Task WaitAsync(H3Index cell) => running.TryGetValue((ulong)cell, out var task) ? task : Task.CompletedTask;

    private async Task LoadAsync(H3Index cell)
    {
        try
        {
            await using var scope = scopes.CreateAsyncScope();
            var places = await scope.ServiceProvider.GetRequiredService<IAreaPlaceSource>().FetchAsync(cell, lifetime.ApplicationStopping);
            var region = await scope.ServiceProvider.GetRequiredService<IAreaRepository>().SaveAsync(cell, places, lifetime.ApplicationStopping);
            failures.TryRemove((ulong)cell, out _);
            await (scope.ServiceProvider.GetService<IPlaceCache>()?.EvictAsync(lifetime.ApplicationStopping) ?? Task.CompletedTask);
            LogLoaded(logger, cell, region, places.Count);
        }
        catch (Exception exception)
        {
            failures[(ulong)cell] = clock.UtcNow;
            LogFailed(logger, exception, cell);
        }
        finally
        {
            running.TryRemove((ulong)cell, out _);
        }
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Loaded area {Cell} as {Region} with {Places} places")]
    private static partial void LogLoaded(ILogger logger, H3Index cell, string region, int places);

    [LoggerMessage(Level = LogLevel.Warning, Message = "Loading area {Cell} failed")]
    private static partial void LogFailed(ILogger logger, Exception exception, H3Index cell);
}
