using Unwalked.Domain.Common;
using Unwalked.Domain.Sharing;

namespace Unwalked.UnitTests.Domain;

public class ShareImageTests
{
    public static readonly byte[] TinyPng = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00, 0x00];
    public static readonly byte[] TinyJpeg = [0xFF, 0xD8, 0xFF, 0xE0, 0x00];
    public static readonly byte[] TinyWebp = [.. "RIFF"u8.ToArray(), 0x10, 0x00, 0x00, 0x00, .. "WEBP"u8.ToArray(), 0x00];

    private static IEnumerable<TestCaseData> MatchingImages()
    {
        yield return new TestCaseData(ShareImage.Png, TinyPng);
        yield return new TestCaseData(" IMAGE/JPEG ", TinyJpeg);
        yield return new TestCaseData(ShareImage.Webp, TinyWebp);
    }

    [TestCaseSource(nameof(MatchingImages))]
    public void Create_AcceptsImagesMatchingDeclaredType(string contentType, byte[] content)
    {
        var image = ShareImage.Create(contentType, content);

        Assert.Multiple(() =>
        {
            Assert.That(image.ContentType, Is.EqualTo(contentType.Trim().ToLowerInvariant()));
            Assert.That(image.Content, Is.EqualTo(content));
        });
    }

    private static IEnumerable<TestCaseData> MismatchedImages()
    {
        yield return new TestCaseData(ShareImage.Jpeg, TinyPng);
        yield return new TestCaseData(ShareImage.Png, TinyJpeg);
        yield return new TestCaseData(ShareImage.Webp, "RIFF0000WAVE"u8.ToArray());
        yield return new TestCaseData(ShareImage.Webp, "RIFF"u8.ToArray());
        yield return new TestCaseData("image/gif", "GIF89a"u8.ToArray());
        yield return new TestCaseData("image/svg+xml", "<svg/>"u8.ToArray());
    }

    [TestCaseSource(nameof(MismatchedImages))]
    public void Create_RejectsContentNotMatchingDeclaredType(string contentType, byte[] content)
    {
        var error = Assert.Throws<DomainValidationException>(() => ShareImage.Create(contentType, content));

        Assert.That(error!.Field, Is.EqualTo("image.contentType"));
    }

    [Test]
    public void Create_RejectsEmptyImage()
    {
        var error = Assert.Throws<DomainValidationException>(() => ShareImage.Create(ShareImage.Png, []));

        Assert.That(error!.Field, Is.EqualTo("image.data"));
    }

    [Test]
    public void Create_RejectsImageLargerThanLimit()
    {
        var oversized = new byte[ShareImage.MaxBytes + 1];
        TinyPng.CopyTo(oversized, 0);

        var error = Assert.Throws<DomainValidationException>(() => ShareImage.Create(ShareImage.Png, oversized));

        Assert.That(error!.Field, Is.EqualTo("image.data"));
    }

    [Test]
    public void FromBase64_DecodesContent()
    {
        var image = ShareImage.FromBase64(ShareImage.Png, Convert.ToBase64String(TinyPng));

        Assert.That(image.Content, Is.EqualTo(TinyPng));
    }

    [Test]
    public void FromBase64_RejectsInvalidBase64()
    {
        var error = Assert.Throws<DomainValidationException>(() => ShareImage.FromBase64(ShareImage.Png, "not base64!"));

        Assert.That(error!.Field, Is.EqualTo("image.data"));
    }
}
