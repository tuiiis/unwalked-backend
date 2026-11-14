using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Unwalked.Domain.Feedback;
using Unwalked.Domain.Places;

namespace Unwalked.Infrastructure.Persistence.Configurations;

public sealed class FeedbackDailyTotalConfiguration : IEntityTypeConfiguration<FeedbackDailyTotal>
{
    public void Configure(EntityTypeBuilder<FeedbackDailyTotal> builder)
    {
        builder.ToTable("feedback_daily_total", table =>
        {
            table.HasCheckConstraint("ck_feedback_daily_total_type", $"recommendation_type IN ({FeedbackColumns.AllowedValues<RecommendationType>()})");
            table.HasCheckConstraint("ck_feedback_daily_total_action", $"action IN ({FeedbackColumns.AllowedValues<FeedbackAction>()})");
            table.HasCheckConstraint("ck_feedback_daily_total_count_positive", "event_count > 0");
        });
        builder.HasKey(total => total.Id);
        FeedbackColumns.Configure(builder.Property(total => total.RecommendationType), builder.Property(total => total.Action));
        builder.HasIndex(total => new { total.ReceivedOn, total.RecommendationType, total.Action, total.CategoryId })
            .IsUnique()
            .AreNullsDistinct(false)
            .HasDatabaseName("ux_feedback_daily_total_day_type_action_category");
        builder.HasOne<Category>()
            .WithMany()
            .HasForeignKey(total => total.CategoryId)
            .HasConstraintName("fk_feedback_daily_total_category")
            .OnDelete(DeleteBehavior.SetNull);
    }
}
