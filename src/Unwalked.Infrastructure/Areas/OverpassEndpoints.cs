namespace Unwalked.Infrastructure.Areas;

/// <summary>
/// Overpass API instances tried in turn when one of them is busy.
/// </summary>
public sealed record OverpassEndpoints(IReadOnlyList<Uri> Urls)
{
    /// <summary>
    /// The main instance first, then public mirrors.
    /// </summary>
    public static readonly string Default = "https://overpass-api.de/api/interpreter,https://overpass.private.coffee/api/interpreter,https://overpass.kumi.systems/api/interpreter";

    /// <summary>
    /// Reads a comma-separated list of URLs.
    /// </summary>
    public static OverpassEndpoints Parse(string list) =>
        new(list.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Select(url => new Uri(url)).ToArray());
}
