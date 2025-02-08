using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Server.Application.Common.Interfaces.Authentication;
using Server.Application.Common.Interfaces.Persistence;
using Server.Application.Common.Interfaces.Services;
using Server.Infrastructure.Authentication;
using Server.Infrastructure.Persistence;
using Server.Infrastructure.Services;
using System.Net;
using System.Net.Mime;
using System.Text;

namespace Server.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
        services.AddScoped<IUserRepository, UserRepository>();

        services.AddAuthentication(configuration);

        return services;
    }

    public static IServiceCollection AddAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

        JwtSettings jwtSettings = new JwtSettings();
        configuration.Bind(JwtSettings.SelectionName, jwtSettings);
        services.AddSingleton(Options.Create(jwtSettings));

        services.AddAuthentication(options =>
        {
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        })
            .AddJwtBearer(options =>
            {
                options.IncludeErrorDetails = true;

                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret))
                };

                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        var result = string.Empty;

                        context.Response.ContentType = MediaTypeNames.Application.Json;

                        // is it token expired.
                        if (context.Exception is SecurityTokenExpiredException)
                        {
                            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;

                            result = JsonConvert.SerializeObject(new { message = "Token expired." });
                        }
                        // or internal error.
                        else
                        {
                            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                            result = JsonConvert.SerializeObject(new { message = "Internal server error." });
                        }

                        return context.Response.WriteAsync(result);
                    },

                    OnChallenge = context =>
                    {
                        context.HandleResponse();

                        // Have to check this because when authorized access api failed,
                        // asp.net core web api will redirect to the 401 page and also we send the 401 message too.
                        // This will make the asp.net throw error.
                        if (!context.Response.HasStarted)
                        {
                            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                            context.Response.ContentType = MediaTypeNames.Application.Json;

                            var result = JsonConvert.SerializeObject(new { message = "You are not authorized." });

                            return context.Response.WriteAsync(result);
                        }

                        return Task.CompletedTask;
                    },

                    OnForbidden = context =>
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                        context.Response.ContentType = MediaTypeNames.Application.Json;

                        var result = JsonConvert.SerializeObject(new { message = "You are not authorized to access these resources." });

                        return context.Response.WriteAsync(result);
                    }
                };
            });

        return services;
    }
}
