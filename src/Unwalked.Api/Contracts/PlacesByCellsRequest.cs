using System.ComponentModel.DataAnnotations;
using Unwalked.Application.Places;

namespace Unwalked.Api.Contracts;

/// <summary>
/// Body of a search by H3 cells, with an optional category filter and result limit.
/// </summary>
public sealed record PlacesByCellsRequest(
    [Required, MinLength(1), MaxLength(PlaceQueryService.MaxRequestedCells)] IReadOnlyList<string> Cells,
    string? Category,
    int? Limit);
