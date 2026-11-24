using Unwalked.Application.Abstractions;
using Unwalked.Application.Errors;
using Unwalked.Domain.Geo;

namespace Unwalked.Application.Areas;

/// <summary>
/// Grows the catalogue area by area. An area already covered by a region is ready, any other area is loaded in the background within global limits.
/// </summary>
public sealed class AreaService(IAreaRepository areas, IAreaLoader loader, IClock clock)
{
    /// <summary>
    /// Most areas that may be loaded on demand in total.
    /// </summary>
    public const int MaxAreas = 200;
    /// <summary>
    /// Most area loads that may start within one hour.
    /// </summary>
    public const int MaxLoadsPerHour = 10;
    /// <summary>
    /// How long a failed area waits before it may be requested again.
    /// </summary>
    public static readonly TimeSpan RetryAfterFailure = TimeSpan.FromMinutes(10);

    /// <summary>
    /// Current state of the area without starting a load.
    /// </summary>
    public async Task<AreaState> GetAsync(string cell, CancellationToken cancellationToken)
    {
        var index = AreaCell.Parse(cell);
        var region = await areas.FindCoveringRegionAsync(index, cancellationToken);
        if (region is not null)
        {
            return new AreaState(index.ToString(), AreaStatus.Ready, region);
        }

        if (loader.IsLoading(index))
        {
            return new AreaState(index.ToString(), AreaStatus.Loading);
        }

        return new AreaState(index.ToString(), loader.FailedAt(index) is null ? AreaStatus.Missing : AreaStatus.Failed);
    }

    /// <summary>
    /// Makes sure the area is in the catalogue. Returns Ready when it is, otherwise starts a load and returns Loading.
    /// </summary>
    public async Task<AreaState> EnsureAsync(string cell, CancellationToken cancellationToken)
    {
        var state = await GetAsync(cell, cancellationToken);
        if (state.Status is AreaStatus.Ready or AreaStatus.Loading)
        {
            return state;
        }

        var index = AreaCell.Parse(cell);
        var now = clock.UtcNow;
        var retryAt = loader.FailedAt(index) + RetryAfterFailure;
        if (retryAt > now)
        {
            throw new TooManyRequestsException("Loading this area failed a moment ago. Try again later.", retryAt.Value - now);
        }

        if (loader.StartedSince(now.AddHours(-1)) >= MaxLoadsPerHour)
        {
            throw new TooManyRequestsException("Too many new areas were requested in the last hour. Try again later.", TimeSpan.FromMinutes(10));
        }

        if (await areas.CountLoadedAreasAsync(cancellationToken) >= MaxAreas)
        {
            throw new TooManyRequestsException("The catalogue cannot take more areas at the moment.", TimeSpan.FromHours(1));
        }

        loader.Start(index);
        return state with { Status = AreaStatus.Loading };
    }
}
