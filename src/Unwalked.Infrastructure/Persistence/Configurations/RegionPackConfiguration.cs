using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Unwalked.Domain.Regions;

namespace Unwalked.Infrastructure.Persistence.Configurations;

public sealed class RegionPackConfiguration : IEntityTypeConfiguration<RegionPack>
{
    public void Configure(EntityTypeBuilder<RegionPack> builder)
    {
        builder.ToTable("region_pack", table =>
        {
            table.HasCheckConstraint("ck_region_pack_version_positive", "version > 0");
            table.HasCheckConstraint("ck_region_pack_size_positive", "size_bytes > 0");
            table.HasCheckConstraint("ck_region_pack_sha256_hex", "sha256 ~ '^[0-9a-f]{64}$'");
        });
        builder.HasKey(pack => pack.Id);
        builder.Property(pack => pack.Sha256).HasMaxLength(RegionPack.Sha256HexLength).IsFixedLength().IsRequired();
        builder.Property(pack => pack.ObjectKey).HasMaxLength(RegionPack.MaxObjectKeyLength).IsRequired();
        builder.HasIndex(pack => new { pack.RegionId, pack.Version }).IsUnique();
        builder.HasIndex(pack => pack.ObjectKey).IsUnique();
        builder.HasIndex(pack => pack.RegionId)
            .IsUnique()
            .HasFilter("is_current")
            .HasDatabaseName("ux_region_pack_current");
        builder.HasOne(pack => pack.Region)
            .WithMany()
            .HasForeignKey(pack => pack.RegionId)
            .HasConstraintName("fk_region_pack_region")
            .OnDelete(DeleteBehavior.Restrict);
    }
}
