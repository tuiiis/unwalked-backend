using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Unwalked.IntegrationTests;

public sealed class UnwalkedApiFactory : WebApplicationFactory<Program>
{
    public const string SigningKey = "integration-test-signing-key-long-enough";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("ConnectionStrings:Unwalked", PostgisEnvironment.ApiConnectionString);
        builder.UseSetting("Jwt:SigningKey", SigningKey);
        builder.UseSetting("Serilog:MinimumLevel:Default", "Warning");
    }
}
