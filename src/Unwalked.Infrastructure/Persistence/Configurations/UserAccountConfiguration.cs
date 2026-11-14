using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Unwalked.Domain.Users;

namespace Unwalked.Infrastructure.Persistence.Configurations;

public sealed class UserAccountConfiguration : IEntityTypeConfiguration<UserAccount>
{
    public void Configure(EntityTypeBuilder<UserAccount> builder)
    {
        builder.ToTable("user_account", table => table.HasCheckConstraint("ck_user_account_email_lowercase", "email = lower(email)"));
        builder.HasKey(user => user.Id);
        builder.Property(user => user.Email).HasMaxLength(EmailAddress.MaxLength).IsRequired();
        builder.Property(user => user.PasswordHash).IsRequired();
        builder.HasIndex(user => user.Email).IsUnique();
    }
}
