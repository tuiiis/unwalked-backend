using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Unwalked.Api.OpenApi;

/// <summary>
/// Configures the OpenAPI document: metadata, the bearer scheme, examples and naming rules.
/// </summary>
public static class SwaggerSetup
{
    /// <summary>
    /// Name of the OpenAPI document, also used in its URL.
    /// </summary>
    public const string DocumentName = "v1";
    private const string BearerSchemeId = "bearer";

    /// <summary>
    /// Registers the OpenAPI generator with all filters used by the API.
    /// </summary>
    public static IServiceCollection AddUnwalkedSwagger(this IServiceCollection services) =>
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc(DocumentName, new OpenApiInfo
            {
                Title = "Places and sharing API",
                Version = "1.0.0",
                Description = "Place catalogue queries, achievement share links and anonymous recommendation feedback for the mobile app.",
                Contact = new OpenApiContact { Name = "Tatsiana Halaburda", Url = new Uri("https://ehu.lt") },
                License = new OpenApiLicense { Name = "MIT", Url = new Uri("https://opensource.org/licenses/MIT") },
            });
            options.AddServer(new OpenApiServer { Url = "/", Description = "The host that serves this document" });
            options.EnableAnnotations();
            options.SupportNonNullableReferenceTypes();
            options.MapType<DateOnly>(() => new OpenApiSchema { Type = "string", Format = "date" });
            options.AddSecurityDefinition(BearerSchemeId, new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Description = "Access token from POST /api/v1/auth/login.",
            });
            options.SchemaFilter<RequiredNonNullablePropertiesFilter>();
            options.OperationFilter<SecurityRequirementFilter>();
            options.OperationFilter<ProblemResponsesFilter>();
            options.OperationFilter<ExamplesFilter>();
            options.DocumentFilter<TagDescriptionsFilter>();
        });

    private sealed class SecurityRequirementFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var metadata = context.ApiDescription.ActionDescriptor.EndpointMetadata;
            var requiresAuth = metadata.OfType<IAuthorizeData>().Any() && !metadata.OfType<IAllowAnonymous>().Any();
            var bearer = new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = BearerSchemeId },
            };

            operation.Security = requiresAuth
                ? [new OpenApiSecurityRequirement { [bearer] = [] }]
                : [new OpenApiSecurityRequirement()];
        }
    }

    private sealed class ProblemResponsesFilter : IOperationFilter
    {
        private const string ProblemJson = "application/problem+json";
        private const string ServerError = "500";

        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            foreach (var (statusCode, response) in operation.Responses.Where(entry => IsError(entry.Key)))
            {
                var schema = response.Content.Values.Select(media => media.Schema).FirstOrDefault(schema => schema is not null)
                    ?? ProblemDetailsSchema(context);
                response.Content = new Dictionary<string, OpenApiMediaType> { [ProblemJson] = new() { Schema = schema } };
                response.Description = ReasonFor(statusCode, response.Description);
            }

            operation.Responses.TryAdd(ServerError, new OpenApiResponse
            {
                Description = "Unexpected server error. The body carries a traceId to quote in bug reports.",
                Content = new Dictionary<string, OpenApiMediaType> { [ProblemJson] = new() { Schema = ProblemDetailsSchema(context) } },
            });
        }

        private static bool IsError(string statusCode) =>
            int.TryParse(statusCode, out var code) && code >= 400;

        private static OpenApiSchema ProblemDetailsSchema(OperationFilterContext context) =>
            context.SchemaGenerator.GenerateSchema(typeof(Microsoft.AspNetCore.Mvc.ProblemDetails), context.SchemaRepository);

        private static string ReasonFor(string statusCode, string current) => statusCode switch
        {
            "400" => "The request is invalid. 'errors' maps each invalid field to its messages.",
            "401" => "The bearer token is missing or invalid, or the credentials are wrong.",
            "404" => "The resource does not exist or is not owned by the caller.",
            "409" => "The resource already exists.",
            _ => current,
        };
    }

    private sealed class RequiredNonNullablePropertiesFilter : ISchemaFilter
    {
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            if (schema.Properties is null || schema.Properties.Count == 0 || context.Type.Namespace?.StartsWith("Microsoft.", StringComparison.Ordinal) == true)
            {
                return;
            }

            foreach (var (name, property) in schema.Properties.Where(entry => !entry.Value.Nullable))
            {
                schema.Required.Add(name);
            }
        }
    }

    private sealed class TagDescriptionsFilter : IDocumentFilter
    {
        private static readonly Dictionary<string, string> Descriptions = new()
        {
            ["Auth"] = "Accounts used only to own achievement shares.",
            ["Feedback"] = "Anonymous, opt-in feedback on recommendations and its aggregate statistics.",
            ["Places"] = "Read-only place catalogue built from Overture Places.",
            ["Shares"] = "Public achievement links.",
        };

        public void Apply(OpenApiDocument document, DocumentFilterContext context)
        {
            var used = document.Paths.Values
                .SelectMany(path => path.Operations.Values)
                .SelectMany(operation => operation.Tags)
                .Select(tag => tag.Name)
                .Distinct()
                .Order(StringComparer.Ordinal);

            document.Tags = used
                .Select(name => new OpenApiTag { Name = name, Description = Descriptions.GetValueOrDefault(name) })
                .ToList();
        }
    }
}
