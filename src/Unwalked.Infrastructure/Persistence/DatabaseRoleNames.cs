namespace Unwalked.Infrastructure.Persistence;

/// <summary>
/// Names of the database group roles created by the migrations.
/// </summary>
public static class DatabaseRoleNames
{
    /// <summary>
    /// Reads the catalogue and the public views.
    /// </summary>
    public const string AppRead = "app_read";
    /// <summary>
    /// Adds the few writes the API needs on top of read access.
    /// </summary>
    public const string AppWrite = "app_write";
    /// <summary>
    /// Reads anonymous feedback and masked accounts for research.
    /// </summary>
    public const string AppAnalyst = "app_analyst";
    /// <summary>
    /// Owns the schema. Used by migrations, seeds and the loader.
    /// </summary>
    public const string Admin = "admin";
}
