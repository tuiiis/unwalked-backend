using System.Net;
using System.Net.Http.Json;

namespace Unwalked.IntegrationTests;

public class SharingApiTests : ApiTestBase
{
    private static readonly byte[] TinyPng = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00, 0x00];

    private static object ShareBody(string name = "First 100 cells") => new
    {
        achievementName = name,
        achievedOn = "2026-10-17",
        image = new { contentType = "image/png", data = Convert.ToBase64String(TinyPng) },
    };

    [Test]
    public async Task Register_RejectsDuplicateEmailWithConflict()
    {
        var body = new { email = $"dup-{Guid.NewGuid():N}@example.lt", password = "integration password" };
        await Client.PostAsJsonAsync("/api/v1/auth/register", body);

        var second = await Client.PostAsJsonAsync("/api/v1/auth/register", body);

        Assert.That(second.StatusCode, Is.EqualTo(HttpStatusCode.Conflict));
    }

    [Test]
    public async Task Login_RejectsWrongPasswordWithUnauthorized()
    {
        var email = $"walker-{Guid.NewGuid():N}@example.lt";
        await Client.PostAsJsonAsync("/api/v1/auth/register", new { email, password = "integration password" });

        var login = await Client.PostAsJsonAsync("/api/v1/auth/login", new { email, password = "something else entirely" });

        Assert.That(login.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task CreateShare_RequiresToken()
    {
        var response = await Client.PostAsJsonAsync("/api/v1/shares", ShareBody());

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task Share_IsPublicReadableAndDeletableOnlyByOwner()
    {
        var owner = await RegisterAndLoginAsync();
        var stranger = await RegisterAndLoginAsync();

        var created = await Client.SendAsync(Authorized(HttpMethod.Post, "/api/v1/shares", owner, ShareBody()));
        var share = await ReadJsonAsync(created);
        var id = share["id"]!.GetValue<string>();

        var publicView = await Client.GetAsync($"/api/v1/shares/{id}");
        var image = await Client.GetAsync(share["imageUrl"]!.GetValue<string>());
        var strangerDelete = await Client.SendAsync(Authorized(HttpMethod.Delete, $"/api/v1/shares/{id}", stranger));
        var ownerDelete = await Client.SendAsync(Authorized(HttpMethod.Delete, $"/api/v1/shares/{id}", owner));
        var afterDelete = await Client.GetAsync($"/api/v1/shares/{id}");

        Assert.Multiple(async () =>
        {
            Assert.That(created.StatusCode, Is.EqualTo(HttpStatusCode.Created));
            Assert.That(created.Headers.Location?.AbsolutePath, Is.EqualTo($"/api/v1/shares/{id}"));
            Assert.That(publicView.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(image.Content.Headers.ContentType?.MediaType, Is.EqualTo("image/png"));
            Assert.That(await image.Content.ReadAsByteArrayAsync(), Is.EqualTo(TinyPng));
            Assert.That(strangerDelete.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            Assert.That(ownerDelete.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
            Assert.That(afterDelete.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
        });
    }

    [Test]
    public async Task DeleteAccount_RemovesSharesAndRetiresTheToken()
    {
        var owner = await RegisterAndLoginAsync();
        var created = await Client.SendAsync(Authorized(HttpMethod.Post, "/api/v1/shares", owner, ShareBody()));
        var id = (await ReadJsonAsync(created))["id"]!.GetValue<string>();

        var deleted = await Client.SendAsync(Authorized(HttpMethod.Delete, "/api/v1/account", owner));
        var repeated = await Client.SendAsync(Authorized(HttpMethod.Delete, "/api/v1/account", owner));
        var shareAfter = await Client.GetAsync($"/api/v1/shares/{id}");
        var staleCreate = await Client.SendAsync(Authorized(HttpMethod.Post, "/api/v1/shares", owner, ShareBody()));

        Assert.Multiple(() =>
        {
            Assert.That(deleted.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
            Assert.That(repeated.StatusCode, Is.EqualTo(HttpStatusCode.NoContent));
            Assert.That(shareAfter.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            Assert.That(staleCreate.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
        });
    }

    [Test]
    public async Task DeleteAccount_RequiresToken()
    {
        var response = await Client.DeleteAsync("/api/v1/account");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
    }

    [Test]
    public async Task CreateShare_RejectsImageNotMatchingContentType()
    {
        var token = await RegisterAndLoginAsync();
        var body = new
        {
            achievementName = "Fake",
            achievedOn = "2026-10-17",
            image = new { contentType = "image/jpeg", data = Convert.ToBase64String(TinyPng) },
        };

        var response = await Client.SendAsync(Authorized(HttpMethod.Post, "/api/v1/shares", token, body));

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
    }
}
