using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Unwalked.Domain.Feedback;

namespace Unwalked.Infrastructure.Persistence.Configurations;

/// <summary>
/// Column mapping shared by the raw and the daily feedback tables.
/// </summary>
internal static class FeedbackColumns
{
    /// <summary>
    /// Stores both enums as snake_case text of limited length.
    /// </summary>
    public static void Configure(PropertyBuilder<RecommendationType> type, PropertyBuilder<FeedbackAction> action)
    {
        type.HasConversion(value => WireName.From(value), text => WireName.Parse<RecommendationType>("recommendationType", text))
            .HasMaxLength(32);
        action.HasConversion(value => WireName.From(value), text => WireName.Parse<FeedbackAction>("action", text))
            .HasMaxLength(16);
    }

    /// <summary>
    /// Comma separated, quoted snake_case values of the enum for use in check constraints.
    /// </summary>
    public static string AllowedValues<TEnum>()
        where TEnum : struct, Enum =>
        string.Join(", ", Enum.GetValues<TEnum>().Select(value => $"'{WireName.From(value)}'"));
}
