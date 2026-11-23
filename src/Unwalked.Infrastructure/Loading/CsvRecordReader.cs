using System.Text;

namespace Unwalked.Infrastructure.Loading;

/// <summary>
/// Minimal RFC 4180 reader for the catalogue export, with quoted fields and doubled quotes.
/// </summary>
public static class CsvRecordReader
{
    private const char Separator = ',';
    private const char Quote = '"';

    /// <summary>
    /// Yields the fields of each record, header included.
    /// </summary>
    public static IEnumerable<IReadOnlyList<string>> Read(TextReader reader)
    {
        var fields = new List<string>();
        var field = new StringBuilder();
        var inQuotes = false;
        var hasContent = false;

        int next;
        while ((next = reader.Read()) != -1)
        {
            var character = (char)next;
            hasContent = true;

            if (inQuotes)
            {
                if (character != Quote)
                {
                    field.Append(character);
                }
                else if (reader.Peek() == Quote)
                {
                    field.Append(Quote);
                    reader.Read();
                }
                else
                {
                    inQuotes = false;
                }

                continue;
            }

            switch (character)
            {
                case Quote:
                    inQuotes = true;
                    break;
                case Separator:
                    fields.Add(field.ToString());
                    field.Clear();
                    break;
                case '\r':
                    break;
                case '\n':
                    fields.Add(field.ToString());
                    field.Clear();
                    yield return fields;
                    fields = [];
                    hasContent = false;
                    break;
                default:
                    field.Append(character);
                    break;
            }
        }

        if (inQuotes)
        {
            throw new FormatException("The CSV input ends inside a quoted field.");
        }

        if (hasContent)
        {
            fields.Add(field.ToString());
            yield return fields;
        }
    }
}
