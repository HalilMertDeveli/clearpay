using Microsoft.OpenApi.Models;

namespace ClearPay.Web.OpenApi;

public static class SwaggerExtensions
{
    public static IServiceCollection AddClearPaySwagger(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "ClearPay API",
                Version = "v1",
                Description = "Demo wallet JSON API. Not a licensed e-money institution. Same Idempotency-Key → HTTP 409, no second debit. Live refresh: SignalR /hubs/wallet (JWT query access_token). Hub payload is a hint — GET /api/wallet remains the balance."
            });
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT from POST /api/token. Example: Bearer {token}",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT"
            });
            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                    },
                    Array.Empty<string>()
                }
            });
            options.OperationFilter<TransferIdempotencyOperationFilter>();
        });
        return services;
    }

    public static WebApplication MapClearPaySwagger(this WebApplication app)
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "ClearPay v1");
            options.RoutePrefix = "swagger";
            options.DocumentTitle = "ClearPay API";
        });
        return app;
    }
}
