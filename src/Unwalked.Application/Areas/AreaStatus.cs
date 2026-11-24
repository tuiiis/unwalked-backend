using System.Text.Json.Serialization;

namespace Unwalked.Application.Areas;

/// <summary>
/// Whether the catalogue covers an area.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter<AreaStatus>))]
public enum AreaStatus
{
    /// <summary>
    /// Places for the area are in the catalogue.
    /// </summary>
    Ready,

    /// <summary>
    /// Places for the area are being loaded.
    /// </summary>
    Loading,

    /// <summary>
    /// The last attempt failed and the area can be requested again later.
    /// </summary>
    Failed,

    /// <summary>
    /// The area has not been requested.
    /// </summary>
    Missing,
}

/// <summary>
/// State of one area with the region that covers it, when there is one.
/// </summary>
public sealed record AreaState(string Cell, AreaStatus Status, string? Region = null);
