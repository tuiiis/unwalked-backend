using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Unwalked.Domain.Feedback;
using Unwalked.Domain.Places;

namespace Unwalked.Infrastructure.Persistence.Configurations;

/// <summary>
/// Maps raw feedback events to the recommendation_feedback table.
/// </summary>
public sealed class RecommendationFeedbackConfiguration : IEntityTypeConfiguration<RecommendationFeedback>
{
    public void Configure(EntityTypeBuilder<RecommendationFeedback> builder)
    {
        builder.ToTable("recommendation_feedback", table =>
        {
            table.HasCheckConstraint("ck_recommendation_feedback_type", $"recommendation_type IN ({FeedbackColumns.AllowedValues<RecommendationType>()})");
            table.HasCheckConstraint("ck_recommendation_feedback_action", $"action IN ({FeedbackColumns.AllowedValues<FeedbackAction>()})");
        });
        builder.HasKey(feedback => feedback.Id);
        FeedbackColumns.Configure(builder.Property(feedback => feedback.RecommendationType), builder.Property(feedback => feedback.Action));
        builder.HasIndex(feedback => feedback.EventId).IsUnique();
        builder.HasIndex(feedback => new { feedback.ReceivedOn, feedback.RecommendationType });
        builder.HasOne<Category>()
            .WithMany()
            .HasForeignKey(feedback => feedback.CategoryId)
            .HasConstraintName("fk_recommendation_feedback_category")
            .OnDelete(DeleteBehavior.SetNull);
    }
}
