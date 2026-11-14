using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Unwalked.Domain.Places;

namespace Unwalked.Infrastructure.Persistence.Configurations;

public sealed class PlaceConfiguration : IEntityTypeConfiguration<Place>
{
    public void Configure(EntityTypeBuilder<Place> builder)
    {
        builder.ToTable("place", table =>
        {
            table.HasCheckConstraint("ck_place_confidence_range", "confidence BETWEEN 0 AND 1");
            table.HasCheckConstraint("ck_place_name_not_blank", "btrim(name) <> ''");
        });
        builder.HasKey(place => place.Id);
        builder.Property(place => place.ExternalId).HasMaxLength(Place.MaxExternalIdLength).IsRequired();
        builder.Property(place => place.Name).HasMaxLength(Place.MaxNameLength).IsRequired();
        builder.Property(place => place.Confidence).HasPrecision(4, 3);
        builder.Property(place => place.Location).HasColumnType("geography (point, 4326)").IsRequired();
        builder.Property(place => place.H3R11).HasColumnName("h3_r11");

        builder.HasIndex(place => place.ExternalId).IsUnique();
        builder.HasIndex(place => place.Location).HasMethod("gist");
        builder.HasIndex(place => place.H3R11);
        builder.HasIndex(place => place.PrimaryCategoryId);
        builder.HasIndex(place => place.RegionId);

        builder.HasOne(place => place.PrimaryCategory)
            .WithMany()
            .HasForeignKey(place => place.PrimaryCategoryId)
            .HasConstraintName("fk_place_primary_category")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(place => place.Region)
            .WithMany()
            .HasForeignKey(place => place.RegionId)
            .HasConstraintName("fk_place_region")
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(place => place.AlternateCategories)
            .WithOne()
            .HasForeignKey(alternate => alternate.PlaceId)
            .HasConstraintName("fk_place_alternate_category_place")
            .OnDelete(DeleteBehavior.Cascade);
    }
}
