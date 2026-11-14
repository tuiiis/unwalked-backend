using Microsoft.Extensions.DependencyInjection;
using Unwalked.Application.Auth;
using Unwalked.Application.Feedback;
using Unwalked.Application.Places;
using Unwalked.Application.Sharing;

namespace Unwalked.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services) =>
        services
            .AddScoped<PlaceQueryService>()
            .AddScoped<AuthService>()
            .AddScoped<ShareService>()
            .AddScoped<FeedbackService>();
}
