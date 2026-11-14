using System.Globalization;
using Unwalked.Domain.Common;
using Unwalked.Domain.Geo;
using Unwalked.Domain.Places;

namespace Unwalked.Infrastructure.Loading;

public sealed class PlaceCsvParser
{
    public static readonly string[] RequiredColumns = ["id", "name", "category", "alternate_categories", "confidence", "lon", "lat"];

    private static readonly char[] ListSeparators = [',', '|', ';'];
    private static readonly char[] ListDecorations = ['[', ']', '"', '\'', ' '];

    public int SkippedRows { get; private set; }

    public IEnumerable<PlaceImportRecord> Parse(TextReader reader)
    {
        using var rows = CsvRecordReader.Read(reader).GetEnumerator();
        if (!rows.MoveNext())
        {
            yield break;
        }

        var columns = MapColumns(rows.Current);
        var seenIds = new HashSet<string>(StringComparer.Ordinal);

        while (rows.MoveNext())
        {
            var record = TryCreate(rows.Current, columns);
            if (record is null || !seenIds.Add(record.ExternalId))
            {
                SkippedRows++;
                continue;
            }

            yield return record;
        }
    }

    public static IReadOnlyList<string> ParseCategoryList(string raw) =>
        raw.Split(ListSeparators, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(item => item.Trim(ListDecorations))
            .Where(item => item.Length > 0 && !item.Equals("null", StringComparison.OrdinalIgnoreCase))
            .Distinct(StringComparer.Ordinal)
            .ToList();

    private static Dictionary<string, int> MapColumns(IReadOnlyList<string> header)
    {
        var columns = header
            .Select((name, index) => (Name: name.Trim().ToLowerInvariant(), Index: index))
            .ToDictionary(column => column.Name, column => column.Index);

        var missing = RequiredColumns.Where(column => !columns.ContainsKey(column)).ToList();
        if (missing.Count > 0)
        {
            throw new FormatException($"The CSV header is missing columns: {string.Join(", ", missing)}.");
        }

        return columns;
    }

    private static PlaceImportRecord? TryCreate(IReadOnlyList<string> row, IReadOnlyDictionary<string, int> columns)
    {
        if (row.Count < columns.Count)
        {
            return null;
        }

        string Field(string name) => row[columns[name]].Trim();

        var externalId = Field("id");
        var name = Field("name");
        if (externalId.Length is 0 or > Place.MaxExternalIdLength || name.Length == 0)
        {
            return null;
        }

        if (!double.TryParse(Field("lon"), NumberStyles.Float, CultureInfo.InvariantCulture, out var longitude)
            || !double.TryParse(Field("lat"), NumberStyles.Float, CultureInfo.InvariantCulture, out var latitude)
            || !decimal.TryParse(Field("confidence"), NumberStyles.Float, CultureInfo.InvariantCulture, out var confidence))
        {
            return null;
        }

        try
        {
            GeoCoordinate.Create(latitude, longitude);
            var primary = CategoryCode.Normalize(Field("category"));
            var alternates = ParseCategoryList(Field("alternate_categories"))
                .Select(CategoryCode.Normalize)
                .Where(code => code != primary)
                .Distinct(StringComparer.Ordinal)
                .ToList();

            return new PlaceImportRecord(
                externalId,
                name.Length > Place.MaxNameLength ? name[..Place.MaxNameLength] : name,
                primary,
                alternates,
                Math.Round(Math.Clamp(confidence, 0m, 1m), 3),
                longitude,
                latitude);
        }
        catch (DomainValidationException)
        {
            return null;
        }
    }
}
