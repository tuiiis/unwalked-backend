using H3;
using NetTopologySuite.Geometries;
using Npgsql;
using NpgsqlTypes;
using Unwalked.Application.Abstractions;
using Unwalked.Domain.Geo;
using Unwalked.Domain.Places;

namespace Unwalked.Infrastructure.Areas;

/// <summary>
/// Stores areas loaded on demand as regions with their places. Uses only INSERT, so the API login keeps no right to change or delete the catalogue.
/// </summary>
public sealed class AreaRepository(NpgsqlDataSource dataSource, GeometryFactory geometryFactory) : IAreaRepository
{
    private const decimal OsmConfidence = 0.700m;

    /// <summary>
    /// Code of a region whose bounds overlap the cell, or null. Overlap is enough so that a loaded area never repeats places of a preloaded region.
    /// </summary>
    public async Task<string?> FindCoveringRegionAsync(H3Index cell, CancellationToken cancellationToken)
    {
        await using var command = dataSource.CreateCommand(
            "SELECT code FROM region WHERE ST_Intersects(bounds, @outline) ORDER BY code LIKE 'area-%', code LIMIT 1");
        command.Parameters.Add(new NpgsqlParameter("outline", NpgsqlDbType.Geography) { Value = AreaCell.Outline(cell, geometryFactory) });
        return await command.ExecuteScalarAsync(cancellationToken) as string;
    }

    /// <summary>
    /// Number of regions loaded on demand.
    /// </summary>
    public async Task<int> CountLoadedAreasAsync(CancellationToken cancellationToken)
    {
        await using var command = dataSource.CreateCommand("SELECT count(*) FROM region WHERE code LIKE 'area-%'");
        return (int)(long)(await command.ExecuteScalarAsync(cancellationToken))!;
    }

    /// <summary>
    /// Inserts the region and its places in one transaction. Places whose category is not in the catalogue are skipped.
    /// </summary>
    public async Task<string> SaveAsync(H3Index cell, IReadOnlyList<AreaPlace> places, CancellationToken cancellationToken)
    {
        var code = AreaCell.RegionCode(cell);
        var centre = AreaCell.Centre(cell);
        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        int regionId;
        await using (var region = new NpgsqlCommand("INSERT INTO region (code, name, bounds) VALUES (@code, @name, @bounds) RETURNING id", connection, transaction))
        {
            region.Parameters.AddWithValue("code", code);
            region.Parameters.AddWithValue("name", FormattableString.Invariant($"Area around {centre.Latitude:F2}, {centre.Longitude:F2}"));
            region.Parameters.Add(new NpgsqlParameter("bounds", NpgsqlDbType.Geography) { Value = AreaCell.Outline(cell, geometryFactory) });
            regionId = (int)(await region.ExecuteScalarAsync(cancellationToken))!;
        }

        var categories = new Dictionary<string, int>(StringComparer.Ordinal);
        await using (var select = new NpgsqlCommand("SELECT code, id FROM category WHERE code = ANY(@codes)", connection, transaction))
        {
            select.Parameters.AddWithValue("codes", places.Select(place => place.Category).Distinct().ToArray());
            await using var reader = await select.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                categories[reader.GetString(0)] = reader.GetInt32(1);
            }
        }

        await using (var writer = await connection.BeginBinaryImportAsync(
            "COPY place (external_id, name, confidence, location, h3_r11, region_id, primary_category_id) FROM STDIN (FORMAT BINARY)",
            cancellationToken))
        {
            foreach (var place in places.Where(place => categories.ContainsKey(place.Category)).DistinctBy(place => place.ExternalId))
            {
                await writer.StartRowAsync(cancellationToken);
                await writer.WriteAsync(place.ExternalId, NpgsqlDbType.Varchar, cancellationToken);
                await writer.WriteAsync(place.Name, NpgsqlDbType.Varchar, cancellationToken);
                await writer.WriteAsync(OsmConfidence, NpgsqlDbType.Numeric, cancellationToken);
                await writer.WriteAsync(geometryFactory.CreatePoint(new Coordinate(place.Longitude, place.Latitude)), NpgsqlDbType.Geography, cancellationToken);
                await writer.WriteAsync(H3Cell.ForCoordinate(GeoCoordinate.Create(place.Latitude, place.Longitude)), NpgsqlDbType.Bigint, cancellationToken);
                await writer.WriteAsync(regionId, NpgsqlDbType.Integer, cancellationToken);
                await writer.WriteAsync(categories[place.Category], NpgsqlDbType.Integer, cancellationToken);
            }

            await writer.CompleteAsync(cancellationToken);
        }

        await transaction.CommitAsync(cancellationToken);
        return code;
    }
}
