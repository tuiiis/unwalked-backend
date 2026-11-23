namespace Unwalked.Application.Feedback;

/// <summary>
/// Input of the submit feedback use case, with names still in wire format.
/// </summary>
public sealed record SubmitFeedbackCommand(Guid EventId, string RecommendationType, string Action, string? Category);
