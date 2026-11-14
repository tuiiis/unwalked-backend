using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Unwalked.Domain.Feedback;

namespace Unwalked.Infrastructure.Persistence.Configurations;

internal static class FeedbackColumns
{
    public static void Configure(PropertyBuilder<RecommendationType> type, PropertyBuilder<FeedbackAction> action)
    {
        type.HasConversion(value => WireName.From(value), text => WireName.Parse<RecommendationType>("recommendationType", text))
            .HasMaxLength(32);
        action.HasConversion(value => WireName.From(value), text => WireName.Parse<FeedbackAction>("action", text))
            .HasMaxLength(16);
    }

    public static string AllowedValues<TEnum>()
        where TEnum : struct, Enum =>
        string.Join(", ", Enum.GetValues<TEnum>().Select(value => $"'{WireName.From(value)}'"));
}
