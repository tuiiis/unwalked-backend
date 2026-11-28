using Microsoft.AspNetCore.OutputCaching;
using Unwalked.Application.Abstractions;

namespace Unwalked.Api.Caching;

/// <summary>
/// Output cache for the anonymous place searches. Entries live ten minutes and are dropped as soon as an area finishes loading.
/// </summary>
public sealed class PlaceOutputCache(IOutputCacheStore store) : IPlaceCache
{
    public const string Policy = "places";

    public static readonly TimeSpan Lifetime = TimeSpan.FromMinutes(10);

    public Task EvictAsync(CancellationToken cancellationToken) =>
        store.EvictByTagAsync(Policy, cancellationToken).AsTask();
}
