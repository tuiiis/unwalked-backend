namespace Unwalked.Application.Feedback;

public sealed record SubmitFeedbackCommand(Guid EventId, string RecommendationType, string Action, string? Category);
