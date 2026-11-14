using System.ComponentModel.DataAnnotations;
using Unwalked.Application.Places;

namespace Unwalked.Api.Contracts;

public sealed record PlacesByCellsRequest(
    [Required, MinLength(1), MaxLength(PlaceQueryService.MaxRequestedCells)] IReadOnlyList<string> Cells,
    string? Category,
    int? Limit);
