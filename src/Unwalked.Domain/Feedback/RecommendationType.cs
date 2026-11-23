namespace Unwalked.Domain.Feedback;

/// <summary>
/// Kinds of recommendation compared in the study.
/// </summary>
public enum RecommendationType
{
    /// <summary>
    /// A new place close to where the user often is.
    /// </summary>
    NearbyNewPlace,
    /// <summary>
    /// A place on or next to a street the user walks often.
    /// </summary>
    AlongHabitualRoute,
    /// <summary>
    /// An area next to the usual ones that the user has never entered.
    /// </summary>
    UnvisitedCell,
    /// <summary>
    /// A generated walk through unwalked streets.
    /// </summary>
    WalkingRoute,
}
