using AutoMapper;
using Microsoft.OpenApi.Models;
using System.Reflection;

namespace Server.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddPresentation(this IServiceCollection services)
    {
        services.AddControllers();

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.EnableAnnotations();

            c.SwaggerDoc("AdminAPI", new OpenApiInfo
            {
                Version = "v1",
                Title = "Test API",
                Description = "This API is to demo how the jwt bearer event works.",
            });

            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                Description = "To access this API, provide your access token."
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme()
                    {
                        Reference = new OpenApiReference
                        {
                            Id = "Bearer",
                            Type = ReferenceType.SecurityScheme,
                        },
                        Scheme = "Bearer",
                        Name = "Bearer",
                        In = ParameterLocation.Header,
                    },
                    []
                }
            });
        });

        // auto-mapper service.
        services.AddAutoMapper(Assembly.GetExecutingAssembly());

        services.AddAuthentication();
        services.AddAuthorization();

        return services;
    }
}

public static class AutoMapperManager
{
    public static WebApplication AddAutoMapperValidation(this WebApplication app)
    {
        var scope = app.Services.CreateScope();
        var mapper = scope.ServiceProvider.GetRequiredService<IMapper>();
        mapper.ConfigurationProvider.AssertConfigurationIsValid();

        return app;
    }
}
