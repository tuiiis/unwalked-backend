using Unwalked.Infrastructure.Loading;

namespace Unwalked.UnitTests.Infrastructure;

public class CsvRecordReaderTests
{
    [Test]
    public void Read_SplitsRowsAndHandlesQuotedSeparatorsQuotesAndNewlines()
    {
        const string csv = "id,name\r\n1,\"Cafe, \"\"Kavos\"\" bar\"\n2,\"two\nlines\"";

        var rows = CsvRecordReader.Read(new StringReader(csv)).ToList();

        Assert.That(rows, Is.EqualTo(new[]
        {
            new[] { "id", "name" },
            new[] { "1", "Cafe, \"Kavos\" bar" },
            new[] { "2", "two\nlines" },
        }));
    }

    [Test]
    public void Read_KeepsEmptyFieldsAndIgnoresTrailingNewline()
    {
        var rows = CsvRecordReader.Read(new StringReader("a,,c\n")).ToList();

        Assert.That(rows, Is.EqualTo(new[] { new[] { "a", string.Empty, "c" } }));
    }

    [Test]
    public void Read_ThrowsWhenInputEndsInsideQuotes()
    {
        Assert.Throws<FormatException>(() => CsvRecordReader.Read(new StringReader("a,\"open")).ToList());
    }
}
