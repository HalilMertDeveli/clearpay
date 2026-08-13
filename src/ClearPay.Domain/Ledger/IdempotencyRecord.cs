namespace ClearPay.Domain.Ledger;

/// <summary>
/// Neden 409? Aynı <c>Idempotency-Key</c> aynı niyettir. İkinci HTTP 201/200 ile
/// “başarılı” görünürse istemci tekrar denemez sanır ve çift kesinti olur.
/// Unique <see cref="Key"/> ihlali → 409 Conflict; ikinci ledger yazılmaz.
/// (Aynı key + aynı sonuç gövdesi 200 da mümkün; ClearPay tercihi 409.)
/// </summary>
public sealed class IdempotencyRecord
{
    /// <summary>HTTP Idempotency-Key. Unique index (PK). Duplicate insert → 409.</summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>Use-case scope, e.g. <c>transfer</c>, so keys do not collide across APIs.</summary>
    public string Scope { get; set; } = string.Empty;

    /// <summary>Canonical request fingerprint. Same key, different body still must not debit twice.</summary>
    public string? RequestHash { get; set; }

    /// <summary>Created resource (Transfer id on success).</summary>
    public Guid? ResourceId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}
