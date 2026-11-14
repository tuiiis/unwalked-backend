using Unwalked.Domain.Common;

namespace Unwalked.Domain.Sharing;

public sealed record ShareImage
{
    public const int MaxBytes = 300 * 1024;
    public const string Png = "image/png";
    public const string Jpeg = "image/jpeg";
    public const string Webp = "image/webp";

    private static readonly byte[] PngSignature = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];
    private static readonly byte[] JpegSignature = [0xFF, 0xD8, 0xFF];
    private static readonly byte[] RiffSignature = "RIFF"u8.ToArray();
    private static readonly byte[] WebpSignature = "WEBP"u8.ToArray();

    private ShareImage(string contentType, byte[] content)
    {
        ContentType = contentType;
        Content = content;
    }

    public string ContentType { get; }

    public byte[] Content { get; }

    public static ShareImage FromBase64(string contentType, string base64)
    {
        byte[] content;
        try
        {
            content = Convert.FromBase64String(base64);
        }
        catch (FormatException)
        {
            throw new DomainValidationException("image.data", "Image data must be base64 encoded.");
        }

        return Create(contentType, content);
    }

    public static ShareImage Create(string contentType, byte[] content)
    {
        var normalizedType = contentType.Trim().ToLowerInvariant();
        if (content.Length == 0 || content.Length > MaxBytes)
        {
            throw new DomainValidationException("image.data", $"Image must be between 1 byte and {MaxBytes} bytes.");
        }

        if (!MatchesSignature(normalizedType, content))
        {
            throw new DomainValidationException("image.contentType", "Image must be a PNG, JPEG or WebP file matching its declared content type.");
        }

        return new ShareImage(normalizedType, content);
    }

    private static bool MatchesSignature(string contentType, byte[] content) => contentType switch
    {
        Png => content.AsSpan().StartsWith(PngSignature),
        Jpeg => content.AsSpan().StartsWith(JpegSignature),
        Webp => content.Length >= 12
            && content.AsSpan(0, 4).SequenceEqual(RiffSignature)
            && content.AsSpan(8, 4).SequenceEqual(WebpSignature),
        _ => false,
    };
}
