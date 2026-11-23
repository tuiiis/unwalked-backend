namespace Unwalked.IntegrationTests;

public class OpenApiDocumentTests : ApiTestBase
{
    private const string UpdateVariable = "UPDATE_OPENAPI";

    [Test]
    public async Task CommittedSpecification_MatchesServedDocument()
    {
        var served = Normalize(await Client.GetStringAsync("/openapi/v1.yaml"));
        var path = Path.Combine(RepositoryRoot(), "docs", "api", "openapi.yaml");

        if (Environment.GetEnvironmentVariable(UpdateVariable) == "1")
        {
            await File.WriteAllTextAsync(path, served);
        }

        Assert.That(File.Exists(path), Is.True, $"Run the tests with {UpdateVariable}=1 to create {path}.");
        Assert.That(Normalize(await File.ReadAllTextAsync(path)), Is.EqualTo(served), $"The API changed. Run the tests with {UpdateVariable}=1 and commit docs/api/openapi.yaml.");
    }

    [Test]
    public async Task YamlAndJsonDocuments_AreBothServed()
    {
        var yaml = await Client.GetAsync("/openapi/v1.yaml");
        var json = await Client.GetAsync("/openapi/v1.json");

        Assert.Multiple(() =>
        {
            Assert.That(yaml.IsSuccessStatusCode, Is.True);
            Assert.That(json.IsSuccessStatusCode, Is.True);
        });
    }

    private static string Normalize(string text) => text.Replace("\r\n", "\n").TrimEnd() + "\n";

    private static string RepositoryRoot()
    {
        var directory = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Unwalked.sln")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName ?? throw new DirectoryNotFoundException("Unwalked.sln was not found above the test directory.");
    }
}
