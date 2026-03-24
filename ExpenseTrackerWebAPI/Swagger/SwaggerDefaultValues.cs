using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Text.Json;

namespace ExpenseTracker.API.Swagger;

public class SwaggerDefaultValues : IOperationFilter
{
    /// <summary>
    /// Ensures sensible defaults and corrected metadata in the OpenAPI specification
    /// when using ASP.NET Core API Versioning with Swashbuckle.
    ///
    /// ASP.NET Core’s API Explorer provides detailed descriptions and metadata for
    /// versioned controllers and parameters, but Swashbuckle doesn’t always apply
    /// them correctly by default. This filter:
    ///   • Sets the operation as deprecated when the API version is deprecated.
    ///   • Cleans response content types so documented media types match real ones.
    ///   • Applies parameter descriptions, default values, and required flags
    ///     based on API Explorer metadata.
    ///
    /// This implementation follows the common pattern documented in the ASP.NET API
    /// Versioning community (see "Swashbuckle Integration" in the API Versioning
    /// project wiki), and fills in Swagger defaults that otherwise would be missing
    /// or incorrect for versioned APIs.
    /// 
    /// see:
    ///     ref=https://github.com/dotnet/aspnet-api-versioning/wiki/Swashbuckle-Integration
    ///     ref=https://github.com/microsoft/aspnet-api-versioning/tree/master/samples/aspnetcore/SwaggerSample
    /// </summary>
    
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var apiDescription = context.ApiDescription;

        operation.Deprecated |= apiDescription.IsDeprecated();

        foreach (var responseType in context.ApiDescription.SupportedResponseTypes)
        {
            var responseKey = responseType.IsDefaultResponse
                ? "default"
                : responseType.StatusCode.ToString();
            var response = operation.Responses[responseKey];

            foreach (var contentType in response.Content.Keys)
            {
                if (responseType.ApiResponseFormats.All(x => x.MediaType != contentType))
                {
                    response.Content.Remove(contentType);
                }
            }
        }

        if (operation.Parameters == null)
        {
            return;
        }

        foreach (var parameter in operation.Parameters)
        {
            var description = apiDescription.ParameterDescriptions
                .First(p => p.Name == parameter.Name);

            parameter.Description ??= description.ModelMetadata.Description;

            if (parameter.Schema.Default == null && description.DefaultValue != null)
            {
                var json = JsonSerializer.Serialize(
                    description.DefaultValue,
                    description.ModelMetadata!.ModelType);
                parameter.Schema.Default = OpenApiAnyFactory.CreateFromJson(json);
            }

            parameter.Required |= description.IsRequired;
        }
    }
}
