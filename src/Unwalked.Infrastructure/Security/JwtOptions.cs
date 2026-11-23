using System.ComponentModel.DataAnnotations;

namespace Unwalked.Infrastructure.Security;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";
    public const int MinSigningKeyLength = 32;

    [Required]
    public string Issuer { get; set; } = string.Empty;

    [Required]
    public string Audience { get; set; } = string.Empty;

    [Required]
    [MinLength(MinSigningKeyLength)]
    public string SigningKey { get; set; } = string.Empty;

    [Range(1, 43_200)]
    public int LifetimeMinutes { get; set; } = 43_200;
}
