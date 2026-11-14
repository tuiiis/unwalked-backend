using System.Diagnostics;
using NetTopologySuite.Geometries;
using Npgsql;
using NpgsqlTypes;
using Unwalked.Domain.Geo;

namespace Unwalked.Infrastructure.Loading;

public sealed record PlaceLoadReport(int Places, int AlternateLinks, int Categories, int SkippedRows, TimeSpan Elapsed);

public sealed class PlaceBulkLoader(NpgsqlDataSource dataSource, GeometryFactory geometryFactory)
{
    public async Task<PlaceLoadReport> LoadAsync(string regionCode, TextReader csv, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        var parser = new PlaceCsvParser();
        var records = parser.Parse(csv).ToList();

        await using var connection = await dataSource.OpenConnectionAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        var regionId = await FindRegionAsync(connection, regionCode, cancellationToken);
        await using (var delete = new NpgsqlCommand("DELETE FROM place WHERE region_id = @region", connection))
        {
            delete.Parameters.AddWithValue("region", regionId);
            await delete.ExecuteNonQueryAsync(cancellationToken);
        }

        var firstId = await ScalarAsync<long>(connection, "SELECT coalesce(max(id), 0) + 1 FROM place", cancellationToken);
        var categoryIds = await UpsertCategoriesAsync(connection, records, cancellationToken);
        await CopyPlacesAsync(connection, records, firstId, regionId, categoryIds, cancellationToken);
        var links = await CopyAlternateCategoriesAsync(connection, records, firstId, categoryIds, cancellationToken);
        await ExecuteAsync(connection, "SELECT setval(pg_get_serial_sequence('place', 'id'), GREATEST((SELECT max(id) FROM place), 1))", cancellationToken);

        await transaction.CommitAsync(cancellationToken);
        await ExecuteAsync(connection, "ANALYZE place; ANALYZE place_alternate_category; ANALYZE category", cancellationToken);

        return new PlaceLoadReport(records.Count, links, categoryIds.Count, parser.SkippedRows, stopwatch.Elapsed);
    }

    private static async Task<int> FindRegionAsync(NpgsqlConnection connection, string regionCode, CancellationToken cancellationToken)
    {
        await using var command = new NpgsqlCommand("SELECT id FROM region WHERE code = @code", connection);
        command.Parameters.AddWithValue("code", regionCode);
        return await command.ExecuteScalarAsync(cancellationToken) as int?
            ?? throw new InvalidOperationException($"Region '{regionCode}' does not exist. Run the reference seed first.");
    }

    private static async Task<T> ScalarAsync<T>(NpgsqlConnection connection, string sql, CancellationToken cancellationToken)
    {
        await using var command = new NpgsqlCommand(sql, connection);
        return (T)(await command.ExecuteScalarAsync(cancellationToken))!;
    }

    private static async Task<Dictionary<string, int>> UpsertCategoriesAsync(
        NpgsqlConnection connection,
        IReadOnlyCollection<PlaceImportRecord> records,
        CancellationToken cancellationToken)
    {
        var codes = records
            .SelectMany(record => record.AlternateCategories.Append(record.PrimaryCategory))
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        await using (var insert = new NpgsqlCommand("INSERT INTO category (code) SELECT unnest(@codes) ON CONFLICT (code) DO NOTHING", connection))
        {
            insert.Parameters.AddWithValue("codes", codes);
            await insert.ExecuteNonQueryAsync(cancellationToken);
        }

        var ids = new Dictionary<string, int>(StringComparer.Ordinal);
        await using var select = new NpgsqlCommand("SELECT code, id FROM category WHERE code = ANY(@codes)", connection);
        select.Parameters.AddWithValue("codes", codes);
        await using var reader = await select.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            ids[reader.GetString(0)] = reader.GetInt32(1);
        }

        return ids;
    }

    private async Task CopyPlacesAsync(
        NpgsqlConnection connection,
        IReadOnlyList<PlaceImportRecord> records,
        long firstId,
        int regionId,
        IReadOnlyDictionary<string, int> categoryIds,
        CancellationToken cancellationToken)
    {
        await using var writer = await connection.BeginBinaryImportAsync(
            "COPY place (id, external_id, name, confidence, location, h3_r11, region_id, primary_category_id) FROM STDIN (FORMAT BINARY)",
            cancellationToken);

        for (var index = 0; index < records.Count; index++)
        {
            var record = records[index];
            var coordinate = GeoCoordinate.Create(record.Latitude, record.Longitude);
            await writer.StartRowAsync(cancellationToken);
            await writer.WriteAsync(firstId + index, NpgsqlDbType.Bigint, cancellationToken);
            await writer.WriteAsync(record.ExternalId, NpgsqlDbType.Varchar, cancellationToken);
            await writer.WriteAsync(record.Name, NpgsqlDbType.Varchar, cancellationToken);
            await writer.WriteAsync(record.Confidence, NpgsqlDbType.Numeric, cancellationToken);
            await writer.WriteAsync(geometryFactory.CreatePoint(new Coordinate(record.Longitude, record.Latitude)), NpgsqlDbType.Geography, cancellationToken);
            await writer.WriteAsync(H3Cell.ForCoordinate(coordinate), NpgsqlDbType.Bigint, cancellationToken);
            await writer.WriteAsync(regionId, NpgsqlDbType.Integer, cancellationToken);
            await writer.WriteAsync(categoryIds[record.PrimaryCategory], NpgsqlDbType.Integer, cancellationToken);
        }

        await writer.CompleteAsync(cancellationToken);
    }

    private static async Task<int> CopyAlternateCategoriesAsync(
        NpgsqlConnection connection,
        IReadOnlyList<PlaceImportRecord> records,
        long firstId,
        IReadOnlyDictionary<string, int> categoryIds,
        CancellationToken cancellationToken)
    {
        var links = 0;
        await using var writer = await connection.BeginBinaryImportAsync(
            "COPY place_alternate_category (place_id, category_id) FROM STDIN (FORMAT BINARY)",
            cancellationToken);

        for (var index = 0; index < records.Count; index++)
        {
            foreach (var code in records[index].AlternateCategories)
            {
                await writer.StartRowAsync(cancellationToken);
                await writer.WriteAsync(firstId + index, NpgsqlDbType.Bigint, cancellationToken);
                await writer.WriteAsync(categoryIds[code], NpgsqlDbType.Integer, cancellationToken);
                links++;
            }
        }

        await writer.CompleteAsync(cancellationToken);
        return links;
    }

    private static async Task ExecuteAsync(NpgsqlConnection connection, string sql, CancellationToken cancellationToken)
    {
        await using var command = new NpgsqlCommand(sql, connection);
        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}
