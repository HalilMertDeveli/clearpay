using System.Text.Json;
using System.Text.Json.Serialization;

namespace ClearPay.Contracts;

/// <summary>One serializer setting for producer and consumers: camelCase, nulls left out.</summary>
public static class ContractJson
{
    public static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public static string Serialize<T>(T value) => JsonSerializer.Serialize(value, Options);

    /// <summary>Throws <see cref="JsonException"/> when the body is not valid JSON or is the JSON literal null.</summary>
    public static T Deserialize<T>(string json) =>
        JsonSerializer.Deserialize<T>(json, Options) ?? throw new JsonException($"Body is null, expected {typeof(T).Name}.");
}
