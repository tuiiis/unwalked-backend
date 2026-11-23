namespace Unwalked.Application.Abstractions;

/// <summary>
/// Lookup of place categories.
/// </summary>
public interface ICategoryRepository
{
    /// <summary>
    /// Returns the id of the category with this code, or null when it does not exist.
    /// </summary>
    Task<int?> FindIdByCodeAsync(string code, CancellationToken cancellationToken);
}
