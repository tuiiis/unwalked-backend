using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Unwalked.Domain.Sharing;
using Unwalked.Domain.Users;

namespace Unwalked.Infrastructure.Persistence.Configurations;

/// <summary>
/// Maps shared achievements to the achievement_share table.
/// </summary>
public sealed class AchievementShareConfiguration : IEntityTypeConfiguration<AchievementShare>
{
    public void Configure(EntityTypeBuilder<AchievementShare> builder)
    {
        builder.ToTable("achievement_share", table =>
        {
            table.HasCheckConstraint("ck_achievement_share_image_size", $"octet_length(image) BETWEEN 1 AND {ShareImage.MaxBytes}");
            table.HasCheckConstraint(
                "ck_achievement_share_image_type",
                $"image_content_type IN ('{ShareImage.Png}', '{ShareImage.Jpeg}', '{ShareImage.Webp}')");
            table.HasCheckConstraint("ck_achievement_share_name_not_blank", "btrim(achievement_name) <> ''");
        });
        builder.HasKey(share => share.Id);
        builder.Property(share => share.AchievementName).HasMaxLength(AchievementShare.MaxNameLength).IsRequired();
        builder.Property(share => share.ImageContentType).HasMaxLength(32).IsRequired();
        builder.Property(share => share.Image).IsRequired();
        builder.HasIndex(share => share.OwnerId);
        builder.HasOne<UserAccount>()
            .WithMany()
            .HasForeignKey(share => share.OwnerId)
            .HasConstraintName("fk_achievement_share_owner")
            .OnDelete(DeleteBehavior.Cascade);
    }
}
