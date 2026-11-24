using System.Collections.Concurrent;
using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Serilog.Core;
using Serilog.Events;

namespace Unwalked.IntegrationTests;

public class RequestLoggingTests
{
    private sealed class CollectingSink : ILogEventSink
    {
        public ConcurrentQueue<LogEvent> Events { get; } = new();

        public void Emit(LogEvent logEvent) => Events.Enqueue(logEvent);
    }

    [Test]
    public async Task RejectedLogin_IsLoggedWithTheStatusTheClientReceives()
    {
        var sink = new CollectingSink();
        await using var factory = new UnwalkedApiFactory().WithWebHostBuilder(builder =>
        {
            builder.UseSetting("Serilog:MinimumLevel:Default", "Information");
            builder.ConfigureServices(services => services.AddSingleton<ILogEventSink>(sink));
        });
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/v1/auth/login", new { email = "nobody@example.lt", password = "wrong password" });

        var statuses = sink.Events
            .Where(e => e.MessageTemplate.Text.StartsWith("HTTP ", StringComparison.Ordinal) && e.Properties["RequestPath"].ToString() == "\"/api/v1/auth/login\"")
            .Select(e => e.Properties["StatusCode"].ToString())
            .ToList();
        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Unauthorized));
            Assert.That(statuses, Is.EqualTo(new[] { "401" }));
        });
    }
}
