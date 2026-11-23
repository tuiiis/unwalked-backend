using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Unwalked.Api.OpenApi;

/// <summary>
/// Adds the recorded request and response examples to every operation of the OpenAPI document.
/// </summary>
internal sealed class ExamplesFilter : IOperationFilter
{
    private const string Json = "application/json";
    private const string ProblemJson = "application/problem+json";

    private const string ServerError = """
        {"type":"https://tools.ietf.org/html/rfc9110#section-15.6.1","title":"An unexpected error occurred.","status":500,"traceId":"0HNQ6B3DSL1FA:00000001"}
        """;

    private const string MissingToken = """
        {"type":"https://tools.ietf.org/html/rfc9110#section-15.5.2","title":"Unauthorized","status":401,"traceId":"0HNQ6B3DSL1FB:00000001"}
        """;

    private const string Places = """
        [{"id":8382,"externalId":"9c022ea4-de07-496f-86bc-222ef8873252","name":"Cat Cafe","category":"cafe","confidence":0.997,"lat":54.68656777,"lon":25.28320642,"h3Cell":"8b1f4030c6e0fff"},
         {"id":7794,"externalId":"80a2b819-d5b8-4745-afae-b59d2232ac52","name":"Vero Cafe","category":"cafe","confidence":0.989,"lat":54.68815471,"lon":25.27488586,"h3Cell":"8b1f4030c786fff"}]
        """;

    private const string Share = """
        {"id":"185affbe-18ad-4510-a624-8945c4c100ff","achievementName":"10 new streets in November","achievedOn":"2026-11-21","imageUrl":"/api/v1/shares/185affbe-18ad-4510-a624-8945c4c100ff/image","createdAt":"2026-11-21T23:10:29.5148956+00:00"}
        """;

    private static readonly Dictionary<string, OperationExamples> Catalogue = new()
    {
        ["register"] = new(
            Requests: [new("valid", "New account", """{"email":"ona@example.test","password":"Unwalked-test-1"}""")],
            Responses: new()
            {
                ["201"] = [new("created", "Account created", """{"userId":"5551756e-6bdc-4b6e-8a4a-4b0f3e6c2a11"}""")],
                ["400"] =
                [
                    new("invalidEmail", "The email is not an address", """{"type":"https://tools.ietf.org/html/rfc9110#section-15.5.1","title":"One or more validation errors occurred.","status":400,"errors":{"email":["Email address is not valid."]},"traceId":"0HNQ6B3DSL1ET:00000001"}"""),
                    new("shortPassword", "The password is shorter than 10 characters", """{"type":"https://tools.ietf.org/html/rfc9110#section-15.5.1","title":"One or more validation errors occurred.","status":400,"errors":{"password":["Password must be 10-128 characters long."]},"traceId":"0HNQ6B3DSL1EU:00000001"}"""),
                ],
                ["409"] = [new("emailTaken", "The email is already registered", """{"type":"https://tools.ietf.org/html/rfc9110#section-15.5.10","title":"Conflict.","status":409,"detail":"An account with this email already exists.","traceId":"0HNQ6B3DSL1F7:00000001"}""")],
            }),
        ["login"] = new(
            Requests: [new("valid", "Test account from the seed data", """{"email":"ona@example.test","password":"Unwalked-test-1"}""")],
            Responses: new()
            {
                ["200"] = [new("token", "Token valid for 30 days", """{"accessToken":"eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI1NTUxNzU2ZS02YmRjLTRiNmUtOGE0YS00YjBmM2U2YzJhMTEifQ.signature","tokenType":"Bearer","expiresAt":"2026-12-21T23:10:29+00:00"}""")],
                ["400"] = [new("missingPassword", "The password field is absent", """{"type":"https://tools.ietf.org/html/rfc9110#section-15.5.1","title":"One or more validation errors occurred.","status":400,"errors":{"Password":["The Password field is required."]},"traceId":"0HNQ6B3DSL1EV:00000001"}""")],
                ["401"] = [new("wrongCredentials", "Wrong email or wrong password", """{"type":"https://tools.ietf.org/html/rfc9110#section-15.5.2","title":"Authentication failed.","status":401,"detail":"Email or password is incorrect.","traceId":"0HNQ6B3DSL1EQ:00000001"}""")],
            }),
        ["findPlacesNear"] = new(
            Requests: [],
            Responses: new()
            {
                ["200"] =
                [
                    new("found", "Two nearest places within 300 m of Akropolis", """
                        [{"id":8253,"externalId":"350917ff-dbcf-46dc-a0d4-1a9b41a1199a","name":"Triumph Lingerie Partner - UAB MEKTA, SC Akropolis","category":"lingerie_store","confidence":0.850,"lat":54.687243,"lon":25.27966,"h3Cell":"8b1f4030c610fff","distanceMeters":5.4},
                         {"id":8290,"externalId":"fa01feb7-83e4-48bd-b898-a679deb51e4f","name":"Sixt","category":"car_rental_service","confidence":0.800,"lat":54.687156677246094,"lon":25.279651641845703,"h3Cell":"8b1f4030c613fff","distanceMeters":5.7}]
                        """),
                    new("empty", "Nothing in the radius", "[]"),
                ],
                ["400"] = [new("latitudeOutOfRange", "Latitude above 90", """{"type":"https://tools.ietf.org/html/rfc9110#section-15.5.1","title":"One or more validation errors occurred.","status":400,"errors":{"lat":["Latitude must be between -90 and 90."]},"traceId":"0HNQ6B3DSL1EP:00000001"}""")],
            }),
        ["findPlacesInBoundingBox"] = new(
            Requests: [],
            Responses: new()
            {
                ["200"] = [new("cafes", "Cafes in the old town, category=cafe&limit=2", Places)],
                ["400"] = [new("boxTooLarge", "A one by one degree box", """{"type":"https://tools.ietf.org/html/rfc9110#section-15.5.1","title":"One or more validation errors occurred.","status":400,"errors":{"bbox":["The bounding box must not exceed 0.25 square degrees."]},"traceId":"0HNQ6B3DSL1F9:00000001"}""")],
            }),
        ["findPlacesByCells"] = new(
            Requests:
            [
                new("oneCell", "One resolution 11 cell", """{"cells":["8b1f4030c610fff"],"limit":2}"""),
                new("coarseCellWithCategory", "A resolution 9 cell, restaurants only", """{"cells":["891f4030c63ffff"],"category":"restaurant","limit":20}"""),
            ],
            Responses: new()
            {
                ["200"] =
                [
                    new("found", "Places in the cell", """
                        [{"id":8247,"externalId":"a46a077e-cdc8-4751-af2b-f3c12b6564f1","name":"Katpėdėlė","category":"restaurant","confidence":0.963,"lat":54.68741925,"lon":25.27925319,"h3Cell":"8b1f4030c610fff"},
                         {"id":8253,"externalId":"350917ff-dbcf-46dc-a0d4-1a9b41a1199a","name":"Triumph Lingerie Partner - UAB MEKTA, SC Akropolis","category":"lingerie_store","confidence":0.850,"lat":54.687243,"lon":25.27966,"h3Cell":"8b1f4030c610fff"}]
                        """),
                ],
                ["400"] = [new("invalidCell", "A string that is not an H3 index", """{"type":"https://tools.ietf.org/html/rfc9110#section-15.5.1","title":"One or more validation errors occurred.","status":400,"errors":{"cells":["'abc' is not an H3 cell."]},"traceId":"0HNQ6B3DSL1FC:00000001"}""")],
            }),
        ["submitFeedback"] = new(
            Requests:
            [
                new("accepted", "The user opened a nearby cafe", """{"eventId":"7d1f9a52-3c1e-4b8a-9a43-2f0c6b1e5d10","recommendationType":"nearby_new_place","action":"accepted","category":"cafe"}"""),
                new("dismissed", "The user tapped Not now on a walking route", """{"eventId":"0b6c2e7a-91d4-4f3e-8f0a-6d2b9c3e4a21","recommendationType":"walking_route","action":"dismissed"}"""),
            ],
            Responses: new()
            {
                ["400"] = [new("unknownType", "A recommendation type that does not exist", """{"type":"https://tools.ietf.org/html/rfc9110#section-15.5.1","title":"One or more validation errors occurred.","status":400,"errors":{"recommendationType":["'teleport' is not one of: nearby_new_place, along_habitual_route, unvisited_cell, walking_route."]},"traceId":"0HNQ6B3DSL1F6:00000001"}""")],
            }),
        ["getFeedbackStats"] = new(
            Requests: [],
            Responses: new()
            {
                ["200"] =
                [
                    new("mixed", "Some types have feedback, one has none", """
                        {"from":"2026-10-01","to":"2026-11-22","types":[
                          {"recommendationType":"nearby_new_place","accepted":14,"dismissed":6,"visited":9,"acceptanceRate":0.7,"visitRate":0.6428571428571429},
                          {"recommendationType":"along_habitual_route","accepted":8,"dismissed":8,"visited":3,"acceptanceRate":0.5,"visitRate":0.375},
                          {"recommendationType":"unvisited_cell","accepted":2,"dismissed":10,"visited":1,"acceptanceRate":0.16666666666666666,"visitRate":0.5},
                          {"recommendationType":"walking_route","accepted":0,"dismissed":0,"visited":0}]}
                        """),
                ],
                ["400"] = [new("rangeTooLong", "More than 366 days", """{"type":"https://tools.ietf.org/html/rfc9110#section-15.5.1","title":"One or more validation errors occurred.","status":400,"errors":{"to":["The range must not exceed 366 days."]},"traceId":"0HNQ6B3DSL1FD:00000001"}""")],
            }),
        ["createShare"] = new(
            Requests: [new("png", "A 1x1 PNG, base64", """{"achievementName":"10 new streets in November","achievedOn":"2026-11-21","image":{"contentType":"image/png","data":"iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAIAAACQd1PeAAAADElEQVR4nGP4//8/AAX+Av4N70a4AAAAAElFTkSuQmCC"}}""")],
            Responses: new()
            {
                ["201"] = [new("created", "The Location header points at the share", Share)],
                ["400"] = [new("contentMismatch", "The bytes are not the declared PNG", """{"type":"https://tools.ietf.org/html/rfc9110#section-15.5.1","title":"One or more validation errors occurred.","status":400,"errors":{"image.contentType":["Image must be a PNG, JPEG or WebP file matching its declared content type."]},"traceId":"0HNQ6B3DSL1FE:00000001"}""")],
                ["401"] = [new("noToken", "No Authorization header", MissingToken)],
            }),
        ["getShare"] = new(
            Requests: [],
            Responses: new()
            {
                ["200"] = [new("found", "A public share", Share)],
                ["404"] = [new("unknown", "No share with this id", """{"type":"https://tools.ietf.org/html/rfc9110#section-15.5.5","title":"Resource not found.","status":404,"detail":"Share '00000000-0000-0000-0000-000000000001' was not found.","traceId":"0HNQ6B3DSL1F8:00000001"}""")],
            }),
        ["getShareImage"] = new(
            Requests: [],
            Responses: new()
            {
                ["404"] = [new("unknown", "No share with this id", """{"type":"https://tools.ietf.org/html/rfc9110#section-15.5.5","title":"Resource not found.","status":404,"detail":"Share '00000000-0000-0000-0000-000000000001' was not found.","traceId":"0HNQ6B3DSL1F8:00000001"}""")],
            }),
        ["deleteShare"] = new(
            Requests: [],
            Responses: new()
            {
                ["401"] = [new("noToken", "No Authorization header", MissingToken)],
                ["404"] = [new("notOwned", "The share belongs to someone else or does not exist", """{"type":"https://tools.ietf.org/html/rfc9110#section-15.5.5","title":"Resource not found.","status":404,"detail":"Share '185affbe-18ad-4510-a624-8945c4c100ff' was not found.","traceId":"0HNQ6B3DSL1FF:00000001"}""")],
            }),
    };

    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (operation.OperationId is null || !Catalogue.TryGetValue(operation.OperationId, out var examples))
        {
            return;
        }

        if (operation.RequestBody?.Content.TryGetValue(Json, out var requestMedia) == true)
        {
            Attach(requestMedia, examples.Requests);
        }

        foreach (var (status, samples) in examples.Responses)
        {
            if (operation.Responses.TryGetValue(status, out var response) && response.Content.Count > 0)
            {
                Attach(response.Content.Values.First(), samples);
            }
        }

        if (operation.Responses.TryGetValue("500", out var serverError) && serverError.Content.TryGetValue(ProblemJson, out var serverMedia))
        {
            Attach(serverMedia, [new("unexpected", "An unhandled failure, quote the traceId", ServerError)]);
        }
    }

    private static void Attach(OpenApiMediaType media, IReadOnlyList<Sample> samples)
    {
        foreach (var sample in samples)
        {
            media.Examples[sample.Name] = new OpenApiExample
            {
                Summary = sample.Summary,
                Value = OpenApiAnyFactory.CreateFromJson(sample.Json),
            };
        }
    }

    private sealed record Sample(string Name, string Summary, string Json);

    private sealed record OperationExamples(IReadOnlyList<Sample> Requests, Dictionary<string, IReadOnlyList<Sample>> Responses);
}
