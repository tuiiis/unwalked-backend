using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Unwalked.IntegrationTests;

public abstract class ApiTestBase
{
    private UnwalkedApiFactory factory = null!;

    protected HttpClient Client { get; private set; } = null!;

    [OneTimeSetUp]
    public void StartApi()
    {
        factory = new UnwalkedApiFactory();
        Client = factory.CreateClient();
    }

    [OneTimeTearDown]
    public void StopApi()
    {
        Client.Dispose();
        factory.Dispose();
    }

    protected static async Task<JsonNode> ReadJsonAsync(HttpResponseMessage response) =>
        JsonNode.Parse(await response.Content.ReadAsStringAsync())
        ?? throw new InvalidOperationException("The response body is empty.");

    protected async Task<string> RegisterAndLoginAsync()
    {
        var email = $"walker-{Guid.NewGuid():N}@example.lt";
        const string password = "integration password";
        var register = await Client.PostAsJsonAsync("/api/v1/auth/register", new { email, password });
        register.EnsureSuccessStatusCode();

        var login = await Client.PostAsJsonAsync("/api/v1/auth/login", new { email, password });
        login.EnsureSuccessStatusCode();
        return (await ReadJsonAsync(login))["accessToken"]!.GetValue<string>();
    }

    protected static HttpRequestMessage Authorized(HttpMethod method, string url, string token, object? body = null)
    {
        var request = new HttpRequestMessage(method, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        if (body is not null)
        {
            request.Content = JsonContent.Create(body, options: new JsonSerializerOptions(JsonSerializerDefaults.Web));
        }

        return request;
    }
}
