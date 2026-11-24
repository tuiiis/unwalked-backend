using Microsoft.Extensions.DependencyInjection;
using Unwalked.Application.Areas;
using Unwalked.Application.Auth;
using Unwalked.Application.Feedback;
using Unwalked.Application.Places;
using Unwalked.Application.Sharing;

namespace Unwalked.Application;

/// <summary>
/// Registers the application layer services.
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Adds the use case services to the container.
    /// </summary>
    public static IServiceCollection AddApplication(this IServiceCollection services) =>
        services
            .AddScoped<PlaceQueryService>()
            .AddScoped<AreaService>()
            .AddScoped<AuthService>()
            .AddScoped<ShareService>()
            .AddScoped<FeedbackService>();
}
