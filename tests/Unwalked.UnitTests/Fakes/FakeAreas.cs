using H3;
using Unwalked.Application.Abstractions;
using Unwalked.Domain.Places;

namespace Unwalked.UnitTests.Fakes;

public sealed class FakeAreaRepository : IAreaRepository
{
    public string? CoveringRegion { get; set; }

    public int LoadedAreas { get; set; }

    public Task<string?> FindCoveringRegionAsync(H3Index cell, CancellationToken cancellationToken) => Task.FromResult(CoveringRegion);

    public Task<int> CountLoadedAreasAsync(CancellationToken cancellationToken) => Task.FromResult(LoadedAreas);

    public Task<string> SaveAsync(H3Index cell, IReadOnlyList<AreaPlace> places, CancellationToken cancellationToken) => Task.FromResult("area-" + cell);
}

public sealed class FakeAreaLoader : IAreaLoader
{
    public HashSet<ulong> Loading { get; } = [];

    public Dictionary<ulong, DateTimeOffset> Failures { get; } = [];

    public List<ulong> Started { get; } = [];

    public int RecentStarts { get; set; }

    public bool IsLoading(H3Index cell) => Loading.Contains((ulong)cell);

    public DateTimeOffset? FailedAt(H3Index cell) => Failures.TryGetValue((ulong)cell, out var at) ? at : null;

    public int StartedSince(DateTimeOffset since) => RecentStarts;

    public void Start(H3Index cell)
    {
        Started.Add((ulong)cell);
        Loading.Add((ulong)cell);
    }
}
