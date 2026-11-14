using System.Net;
using Npgsql;

namespace Unwalked.IntegrationTests;

public class PlatformTests : ApiTestBase
{
    [Test]
    public async Task Health_ReportsHealthyDatabase()
    {
        var response = await Client.GetAsync("/health");

        Assert.Multiple(async () =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(await response.Content.ReadAsStringAsync(), Is.EqualTo("Healthy"));
        });
    }

    [Test]
    public async Task OpenApiDocument_IsVersion30()
    {
        var document = await ReadJsonAsync(await Client.GetAsync("/openapi/v1.json"));

        Assert.That(document["openapi"]!.GetValue<string>(), Does.StartWith("3.0."));
    }

    [TestCase("INSERT INTO category (code) VALUES ('forbidden')", "category")]
    [TestCase("DELETE FROM place", "place")]
    [TestCase("TRUNCATE recommendation_feedback", "recommendation_feedback")]
    [TestCase("DROP TABLE place", "place")]
    public async Task ApiLogin_CannotChangeCatalogueOrSchema(string sql, string table)
    {
        await using var connection = new NpgsqlConnection(PostgisEnvironment.ApiConnectionString);
        await connection.OpenAsync();
        await using var command = new NpgsqlCommand(sql, connection);

        var error = Assert.ThrowsAsync<PostgresException>(() => command.ExecuteNonQueryAsync());

        Assert.That(error!.SqlState, Is.EqualTo(PostgresErrorCodes.InsufficientPrivilege).Or.EqualTo("42501"), error.MessageText);
        Assert.That(error.MessageText, Does.Contain(table));
    }
}
