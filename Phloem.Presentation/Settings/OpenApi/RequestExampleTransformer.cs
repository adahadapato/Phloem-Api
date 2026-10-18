using System.Text.Json;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;

namespace Phloem.Presentation.Settings.OpenApi;

/// <summary>
/// Puts the example from <see cref="RequestExamples"/> on each request type's schema, so Swagger's
/// "Try it out" box starts with a realistic, valid body.
/// </summary>
/// <remarks>
/// Serialized with the app's own JSON settings (camelCase, enums as words), the same as real requests.
/// Written to <c>examples</c> (a list, the current OpenAPI format); Swagger uses the first one.
/// </remarks>
internal sealed class RequestExampleTransformer(IOptions<JsonOptions> jsonOptions) : IOpenApiSchemaTransformer
{
    private readonly JsonSerializerOptions _json = jsonOptions.Value.SerializerOptions;

    /// <inheritdoc />
    public Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context,
                               CancellationToken cancellationToken)
    {
        if (RequestExamples.ByType.TryGetValue(context.JsonTypeInfo.Type, out var example)
            && JsonSerializer.SerializeToNode(example, example.GetType(), _json) is { } node)
        {
            schema.Examples = [node];
        }

        return Task.CompletedTask;
    }
}