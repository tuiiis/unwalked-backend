using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Unwalked.Domain.Places;

namespace Unwalked.Infrastructure.Persistence.Configurations;

/// <summary>
/// Maps building footprints to the building table with a spatial index.
/// </summary>
public sealed class BuildingConfiguration : IEntityTypeConfiguration<Building>
{
    public void Configure(EntityTypeBuilder<Building> builder)
    {
        builder.ToTable("building", table =>
        {
            table.HasCheckConstraint("ck_building_footprint_valid", "ST_IsValid(footprint::geometry)");
        });
        builder.HasKey(building => building.Id);
        builder.Property(building => building.ExternalId).HasMaxLength(Building.MaxExternalIdLength).IsRequired();
        builder.Property(building => building.Footprint).HasColumnType("geography (multipolygon, 4326)").IsRequired();
        builder.Property(building => building.H3R11).HasColumnName("h3_r11");

        builder.HasIndex(building => building.ExternalId).IsUnique();
        builder.HasIndex(building => building.Footprint).HasMethod("gist");
        builder.HasIndex(building => building.H3R11);
        builder.HasIndex(building => building.RegionId);

        builder.HasOne(building => building.Region)
            .WithMany()
            .HasForeignKey(building => building.RegionId)
            .HasConstraintName("fk_building_region")
            .OnDelete(DeleteBehavior.Restrict);
    }
}
