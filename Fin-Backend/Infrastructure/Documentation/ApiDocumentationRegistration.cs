using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Fin_Backend.Infrastructure.Documentation
{
    public static class ApiDocumentationRegistration
    {
        public static IServiceCollection AddApiDocumentation(this IServiceCollection services)
        {
            // Register the Swagger generator, defining 1 or more Swagger documents
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "FinMFB Banking API",
                    Description = "A comprehensive API for the FinMFB Banking Application",
                    TermsOfService = new Uri("https://www.finmfb.com/terms"),
                    Contact = new OpenApiContact
                    {
                        Name = "FinMFB API Support",
                        Url = new Uri("https://www.finmfb.com/support"),
                        Email = "api-support@finmfb.com"
                    },
                    License = new OpenApiLicense
                    {
                        Name = "FinMFB License",
                        Url = new Uri("https://www.finmfb.com/license")
                    }
                });

                // Add a swagger document for each version
                options.SwaggerDoc("v2", new OpenApiInfo
                {
                    Version = "v2",
                    Title = "FinMFB Banking API v2",
                    Description = "The next generation of the FinMFB Banking API"
                });

                // Set the comments path for the Swagger JSON and UI
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                if (File.Exists(xmlPath))
                {
                    options.IncludeXmlComments(xmlPath);
                }

                // Enable JWT authentication in Swagger
                options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
                {
                    Description = "Standard JWT Authorization header using the Bearer scheme. Example: \"bearer {token}\"",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey
                });

                options.OperationFilter<SecurityRequirementsOperationFilter>();

                // Add API versioning support
                options.OperationFilter<SwaggerDefaultValues>();

                // Document filters
                options.DocumentFilter<SwaggerPathPrefixFilter>();

                // Enable annotations
                options.EnableAnnotations();

                // Use fully qualified schema names to prevent conflicts
                options.CustomSchemaIds(type => type.FullName);

                // Group APIs by controller
                options.TagActionsBy(api => new[] { api.GroupName ?? api.HttpMethod });

                // Sort operations by method then path
                options.OrderActionsBy(apiDesc => $"{apiDesc.HttpMethod}_{apiDesc.RelativePath}");

                // Enable examples
                options.ExampleFilters();
            });

            // Register examples
            services.AddSwaggerExamplesFromAssemblyOf<Startup>();

            // Register versioning options
            services.AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new Microsoft.AspNetCore.Mvc.ApiVersion(1, 0);
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true;
            });

            services.AddVersionedApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true;
            });

            return services;
        }
    }

    public class SwaggerDefaultValues : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var apiDescription = context.ApiDescription;
            operation.Deprecated |= apiDescription.IsDeprecated();

            if (operation.Parameters == null)
                return;

            foreach (var parameter in operation.Parameters)
            {
                var description = apiDescription.ParameterDescriptions.First(p => p.Name == parameter.Name);
                parameter.Description ??= description.ModelMetadata?.Description;
                parameter.Required |= description.IsRequired;
            }
        }
    }

    public class SwaggerPathPrefixFilter : IDocumentFilter
    {
        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            // Replace /api/{version} with the actual version
            var replacements = new Dictionary<string, string>
            {
                { "/api/v{version}", $"/api/{swaggerDoc.Info.Version}" }
            };

            var paths = new OpenApiPaths();
            foreach (var path in swaggerDoc.Paths)
            {
                var newKey = path.Key;
                foreach (var replacement in replacements)
                {
                    newKey = newKey.Replace(replacement.Key, replacement.Value);
                }
                paths.Add(newKey, path.Value);
            }
            swaggerDoc.Paths = paths;
        }
    }

    public class Startup
    {
        // This class exists only to serve as a reference point for examples
    }
}
