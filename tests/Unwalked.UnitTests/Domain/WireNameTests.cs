using Unwalked.Domain.Common;
using Unwalked.Domain.Feedback;

namespace Unwalked.UnitTests.Domain;

public class WireNameTests
{
    [TestCase(RecommendationType.NearbyNewPlace, "nearby_new_place")]
    [TestCase(RecommendationType.AlongHabitualRoute, "along_habitual_route")]
    [TestCase(RecommendationType.UnvisitedCell, "unvisited_cell")]
    [TestCase(RecommendationType.WalkingRoute, "walking_route")]
    public void From_ConvertsPascalCaseToSnakeCase(RecommendationType value, string expected)
    {
        Assert.That(WireName.From(value), Is.EqualTo(expected));
    }

    [TestCase("visited", FeedbackAction.Visited)]
    [TestCase(" Accepted ", FeedbackAction.Accepted)]
    public void Parse_ReadsWireNameIgnoringCaseAndWhitespace(string text, FeedbackAction expected)
    {
        Assert.That(WireName.Parse<FeedbackAction>("action", text), Is.EqualTo(expected));
    }

    [Test]
    public void Parse_RejectsUnknownNameAndListsAllowedValues()
    {
        var error = Assert.Throws<DomainValidationException>(() => WireName.Parse<FeedbackAction>("action", "liked"));

        Assert.Multiple(() =>
        {
            Assert.That(error!.Field, Is.EqualTo("action"));
            Assert.That(error.Message, Is.EqualTo("'liked' is not one of: accepted, dismissed, visited."));
        });
    }

    [Test]
    public void Parse_RejectsPascalCaseName()
    {
        Assert.Throws<DomainValidationException>(() => WireName.Parse<RecommendationType>("recommendationType", "NearbyNewPlace"));
    }
}
