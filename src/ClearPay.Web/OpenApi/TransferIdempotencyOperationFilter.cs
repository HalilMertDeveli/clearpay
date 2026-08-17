using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ClearPay.Web.OpenApi;

/// <summary>Documents Idempotency-Key and HTTP 409 on POST /api/transfers (TASK-14).</summary>
public sealed class TransferIdempotencyOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var path = context.ApiDescription.RelativePath ?? string.Empty;
        var isMoneyPost = path.Equals("api/transfers", StringComparison.OrdinalIgnoreCase)
            || path.Equals("api/topup", StringComparison.OrdinalIgnoreCase)
            || path.Equals("api/withdraw", StringComparison.OrdinalIgnoreCase);
        if (!isMoneyPost
            || context.ApiDescription.HttpMethod is not { } method
            || !method.Equals("POST", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        operation.Parameters ??= [];
        if (operation.Parameters.All(p => p.Name != "Idempotency-Key"))
        {
            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "Idempotency-Key",
                In = ParameterLocation.Header,
                Required = true,
                Schema = new OpenApiSchema { Type = "string" },
                Description = "Same key = same intent. Replay returns 409 Conflict; the wallet is not charged twice."
            });
        }

        operation.Responses["409"] = new OpenApiResponse
        {
            Description = "Conflict — this transfer was already processed. No second debit. Example: { \"title\": \"Conflict\", \"status\": 409, \"detail\": \"This transfer was already processed. The wallet was not charged again.\" }"
        };
        operation.Responses["201"] = new OpenApiResponse
        {
            Description = "Created. Body includes transferId and correlationId."
        };
    }
}
