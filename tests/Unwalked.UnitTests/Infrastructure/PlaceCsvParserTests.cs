using Unwalked.Infrastructure.Loading;

namespace Unwalked.UnitTests.Infrastructure;

public class PlaceCsvParserTests
{
    private const string Header = "id,name,category,alternate_categories,confidence,lon,lat,sources";

    [Test]
    public void Parse_ReadsValidRowAndNormalizesCategories()
    {
        var csv = $"{Header}\n08f1,Vilniaus katedra,Church,\"[\"\"landmark\"\", \"\"church\"\", \"\"Museum\"\"]\",0.97,25.2877,54.6858,meta";

        var record = new PlaceCsvParser().Parse(new StringReader(csv)).Single();

        Assert.That(record, Is.EqualTo(record with
        {
            ExternalId = "08f1",
            Name = "Vilniaus katedra",
            PrimaryCategory = "church",
            Confidence = 0.97m,
            Longitude = 25.2877,
            Latitude = 54.6858,
        }));
        Assert.That(record.AlternateCategories, Is.EqualTo(new[] { "landmark", "museum" }));
    }

    [Test]
    public void Parse_SkipsInvalidAndDuplicateRowsAndCountsThem()
    {
        string[] rows =
        [
            Header,
            "a,Valid,cafe,,0.5,25.28,54.68,x",
            "a,Duplicate id,cafe,,0.5,25.28,54.68,x",
            "b,No category,,,0.5,25.28,54.68,x",
            "c,Bad latitude,cafe,,0.5,25.28,95,x",
            "d,Bad number,cafe,,high,25.28,54.68,x",
            ",No id,cafe,,0.5,25.28,54.68,x",
            "e,Short row",
        ];
        var parser = new PlaceCsvParser();

        var records = parser.Parse(new StringReader(string.Join('\n', rows))).ToList();

        Assert.Multiple(() =>
        {
            Assert.That(records.Select(record => record.ExternalId), Is.EqualTo(new[] { "a" }));
            Assert.That(parser.SkippedRows, Is.EqualTo(6));
        });
    }

    [Test]
    public void Parse_ClampsConfidenceAndAcceptsReorderedColumns()
    {
        const string csv = "lat,lon,confidence,alternate_categories,category,name,id\n54.68,25.28,1.7,null,park,Vingis,p1";

        var record = new PlaceCsvParser().Parse(new StringReader(csv)).Single();

        Assert.Multiple(() =>
        {
            Assert.That(record.Confidence, Is.EqualTo(1m));
            Assert.That(record.AlternateCategories, Is.Empty);
        });
    }

    [Test]
    public void Parse_RejectsHeaderWithoutRequiredColumns()
    {
        var error = Assert.Throws<FormatException>(() => new PlaceCsvParser().Parse(new StringReader("id,name\n1,x")).ToList());

        Assert.That(error!.Message, Does.Contain("category"));
    }

    [Test]
    public void Parse_ReturnsNothingForEmptyInput()
    {
        Assert.That(new PlaceCsvParser().Parse(new StringReader(string.Empty)), Is.Empty);
    }

    [TestCase("a|b;c", new[] { "a", "b", "c" })]
    [TestCase("['a', 'a', 'b']", new[] { "a", "b" })]
    [TestCase("", new string[0])]
    public void ParseCategoryList_AcceptsCommonListFormats(string raw, string[] expected)
    {
        Assert.That(PlaceCsvParser.ParseCategoryList(raw), Is.EqualTo(expected));
    }
}
