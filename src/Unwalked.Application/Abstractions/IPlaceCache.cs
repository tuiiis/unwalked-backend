namespace Unwalked.Application.Abstractions;

/// <summary>
/// Cached place search results that must be dropped when the catalogue grows.
/// </summary>
public interface IPlaceCache
{
    /// <summary>
    /// Drops every cached place search result.
    /// </summary>
    Task EvictAsync(CancellationToken cancellationToken);
}
