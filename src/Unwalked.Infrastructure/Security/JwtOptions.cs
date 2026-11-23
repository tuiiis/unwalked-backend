using System.ComponentModel.DataAnnotations;

namespace Unwalked.Infrastructure.Security;

/// <summary>
/// JWT settings from the Jwt configuration section. The signing key comes only from the environment.
/// </summary>
public sealed class JwtOptions
{
    /// <summary>
    /// Configuration section that holds these settings.
    /// </summary>
    public const string SectionName = "Jwt";
    /// <summary>
    /// Shortest accepted signing key, 32 characters for 256 bits.
    /// </summary>
    public const int MinSigningKeyLength = 32;

    /// <summary>
    /// Value of the iss claim.
    /// </summary>
    [Required]
    public string Issuer { get; set; } = string.Empty;

    /// <summary>
    /// Value of the aud claim.
    /// </summary>
    [Required]
    public string Audience { get; set; } = string.Empty;

    /// <summary>
    /// Secret used to sign tokens.
    /// </summary>
    [Required]
    [MinLength(MinSigningKeyLength)]
    public string SigningKey { get; set; } = string.Empty;

    /// <summary>
    /// How long a token stays valid, at most 30 days.
    /// </summary>
    [Range(1, 43_200)]
    public int LifetimeMinutes { get; set; } = 43_200;
}
