using H3;

namespace Unwalked.Application.Abstractions;

/// <summary>
/// Runs area loads in the background, one at a time per cell.
/// </summary>
public interface IAreaLoader
{
    /// <summary>
    /// True while the cell is being loaded.
    /// </summary>
    bool IsLoading(H3Index cell);

    /// <summary>
    /// When the last load of the cell failed, or null.
    /// </summary>
    DateTimeOffset? FailedAt(H3Index cell);

    /// <summary>
    /// Number of loads started since the given time.
    /// </summary>
    int StartedSince(DateTimeOffset since);

    /// <summary>
    /// Starts loading the cell unless a load is already running.
    /// </summary>
    void Start(H3Index cell);
}
