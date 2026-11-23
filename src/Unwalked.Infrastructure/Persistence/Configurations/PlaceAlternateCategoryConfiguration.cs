using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Unwalked.Domain.Places;

namespace Unwalked.Infrastructure.Persistence.Configurations;

/// <summary>
/// Maps alternate categories of places to the place_alternate_category table.
/// </summary>
public sealed class PlaceAlternateCategoryConfiguration : IEntityTypeConfiguration<PlaceAlternateCategory>
{
    public void Configure(EntityTypeBuilder<PlaceAlternateCategory> builder)
    {
        builder.ToTable("place_alternate_category");
        builder.HasKey(alternate => new { alternate.PlaceId, alternate.CategoryId });
        builder.HasIndex(alternate => new { alternate.CategoryId, alternate.PlaceId });
        builder.HasOne(alternate => alternate.Category)
            .WithMany()
            .HasForeignKey(alternate => alternate.CategoryId)
            .HasConstraintName("fk_place_alternate_category_category")
            .OnDelete(DeleteBehavior.Restrict);
    }
}
