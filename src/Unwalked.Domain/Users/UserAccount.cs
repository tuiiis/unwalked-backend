namespace Unwalked.Domain.Users;

public sealed class UserAccount
{
    public Guid Id { get; set; }

    public required string Email { get; set; }

    public required string PasswordHash { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}
