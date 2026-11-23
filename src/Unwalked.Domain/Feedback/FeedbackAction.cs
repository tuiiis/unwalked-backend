namespace Unwalked.Domain.Feedback;

/// <summary>
/// What the user did with a recommendation.
/// </summary>
public enum FeedbackAction
{
    /// <summary>
    /// The user took up the recommendation.
    /// </summary>
    Accepted,
    /// <summary>
    /// The user hid the recommendation.
    /// </summary>
    Dismissed,
    /// <summary>
    /// The user later visited the recommended place.
    /// </summary>
    Visited,
}
