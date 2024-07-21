using Microsoft.OpenApi.Models;
using System.Reflection;

namespace AI.Demo.Api.Swagger;

public static class SwaggerExtension
{
    public static IServiceCollection AddSwaggerGenWithAuthentication(this IServiceCollection services)
    {
        services.AddSwaggerGen(option =>
        {
            option.SwaggerDoc("v1", new OpenApiInfo { Title = "AI Demo API", Version = "v1" });

            option.IncludeXmlComments(GetXmlCommentsPath());
        });

        return services;
    }

    private static string GetXmlCommentsPath()
    {
        string xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        return Path.Combine(AppContext.BaseDirectory, xmlFile);
    }
}
