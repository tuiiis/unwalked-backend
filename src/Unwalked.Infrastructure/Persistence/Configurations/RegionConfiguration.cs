using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Unwalked.Domain.Regions;

namespace Unwalked.Infrastructure.Persistence.Configurations;

public sealed class RegionConfiguration : IEntityTypeConfiguration<Region>
{
    public void Configure(EntityTypeBuilder<Region> builder)
    {
        builder.ToTable("region", table =>
        {
            table.HasCheckConstraint("ck_region_code_format", "code ~ '^[a-z0-9-]{2,32}$'");
            table.HasCheckConstraint("ck_region_name_not_blank", "btrim(name) <> ''");
        });
        builder.HasKey(region => region.Id);
        builder.Property(region => region.Code).HasMaxLength(Region.MaxCodeLength).IsRequired();
        builder.Property(region => region.Name).HasMaxLength(Region.MaxNameLength).IsRequired();
        builder.Property(region => region.Bounds).HasColumnType("geography (polygon, 4326)").IsRequired();
        builder.HasIndex(region => region.Code).IsUnique();
    }
}
