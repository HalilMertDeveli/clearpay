using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ClearPay.Web.OpenApi;

/// <summary>Documents Idempotency-Key and HTTP 409 on POST /api/transfers (TASK-14).</summary>
public sealed class TransferIdempotencyOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var path = context.ApiDescription.RelativePath ?? string.Empty;
        var method = context.ApiDescription.HttpMethod ?? string.Empty;
        var isMoneyPost = path.Equals("api/transfers", StringComparison.OrdinalIgnoreCase)
            || path.Equals("api/topup", StringComparison.OrdinalIgnoreCase)
            || path.Equals("api/withdraw", StringComparison.OrdinalIgnoreCase);
        if (path.Equals("api/transfers/{id}", StringComparison.OrdinalIgnoreCase)
            && method.Equals("GET", StringComparison.OrdinalIgnoreCase))
        {
            operation.Summary = "Get a transfer the caller sent or received (T-073). 404 if missing or not a party.";
            operation.Responses["200"] = new OpenApiResponse
            {
                Description = "transferId, correlationId, amount, status, createdAt."
            };
            operation.Responses["401"] = new OpenApiResponse
            {
                Description = "ProblemDetails — JWT missing or invalid."
            };
            return;
        }

        if (path.Equals("api/movements", StringComparison.OrdinalIgnoreCase)
            && method.Equals("GET", StringComparison.OrdinalIgnoreCase))
        {
            operation.Summary = "Wallet ledger lines. Query: from, to, kind, page (1+), pageSize (default 20, max 50).";
            operation.Responses["401"] = new OpenApiResponse
            {
                Description = "ProblemDetails — JWT missing or invalid."
            };
            return;
        }

        if (!isMoneyPost
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
