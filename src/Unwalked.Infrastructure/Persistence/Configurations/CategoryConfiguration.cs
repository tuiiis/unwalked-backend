using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Unwalked.Domain.Places;

namespace Unwalked.Infrastructure.Persistence.Configurations;

public sealed class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("category", table => table.HasCheckConstraint("ck_category_code_format", "code ~ '^[a-z0-9_]{1,64}$'"));
        builder.HasKey(category => category.Id);
        builder.Property(category => category.Code).HasMaxLength(CategoryCode.MaxLength).IsRequired();
        builder.HasIndex(category => category.Code).IsUnique();
    }
}
