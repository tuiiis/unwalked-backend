namespace Unwalked.Domain.Users;

/// <summary>
/// An account used only to publish achievements. Stores no location data.
/// </summary>
public sealed class UserAccount
{
    public Guid Id { get; set; }

    public required string Email { get; set; }

    /// <summary>
    /// Salted hash of the password, never the password itself.
    /// </summary>
    public required string PasswordHash { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}
