namespace Unwalked.Application.Abstractions;

/// <summary>
/// Hashing and verification of account passwords.
/// </summary>
public interface IPasswordHasher
{
    /// <summary>
    /// Returns a salted hash of the password.
    /// </summary>
    string Hash(string password);

    /// <summary>
    /// Checks a password against a stored hash.
    /// </summary>
    bool Verify(string hash, string password);
}
