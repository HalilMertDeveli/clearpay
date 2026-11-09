# OBSERVABILITY — bir ödemeyi uçtan uca izlemek

İlan maddesi: *"canlı ortamda hata analizi"*. Bu doküman ClearPay'de bir ödemenin log'unu, trace'ini ve
metriğini nasıl bulduğunu ve bir hata geldiğinde hangi sırayla bakılacağını anlatır.

> **Dürüstlük notu.** Yığın (Seq, OpenTelemetry Collector, Tempo, Prometheus, Grafana) **lokal Docker**'da
> çalışır; Azure'da bağlı bir Seq/Grafana yok. Banka gateway'i hâlâ sahte (`RestBankGateway` / `SoapBankGateway`).
> Kimlik doğrulaması kapalı (Seq `NOAUTHENTICATION`, Grafana anonim admin) — sadece geliştirme içindir.

## Ne eklendi

| Parça | Nerede | Ne yapar |
|-------|--------|----------|
| `AddClearPayObservability()` / `UseClearPayObservability()` | `src/ClearPay.Web/Observability/ObservabilityExtensions.cs` | Program.cs'te iki satır. Serilog + OpenTelemetry + decorator kaydı burada toplanır. |
| Serilog | aynı dosya | Console + Seq sink. Her HTTP isteği için **tek** yapılandırılmış satır (`UseSerilogRequestLogging`). |
| `TraceEnricher` | `Observability/TraceEnricher.cs` | Her log olayına `TraceId`, `SpanId` ekler. |
| `CorrelationIdMiddleware` | `Observability/CorrelationIdMiddleware.cs` | Yanıta `X-Correlation-Id` ve `X-Trace-Id` yazar. Gelen `X-Correlation-Id` güvenliyse (≤64, `[A-Za-z0-9-_.]`) korunur; değilse TraceId kullanılır. |
| `SensitiveDataEnricher` | `Observability/SensitiveDataEnricher.cs` | `Password`, `Token`, `CardNumber`, `Cvv` … adlı property'ler `***`; metin içindeki Luhn-geçerli 13–19 haneli sayı (PAN) `****1111` olur. |
| Kullanıcı alanları | request log | Sadece opak `UserId` (Identity GUID) ve varsa `merchant_id` claim'i. E-posta, ad, TC kimlik, IP **loglanmaz**. |
| OpenTelemetry tracing | `ObservabilityExtensions` | ASP.NET Core, HttpClient, SqlClient (EF Core SQL Server komutları) + `ClearPay.Payments`, `ClearPay.Messaging` kaynakları. OTLP gRPC ile Collector'a. |
| OpenTelemetry metrics | `ClearPayTelemetry` | `clearpay.payments` counter ve `clearpay.payments.duration` histogram (tag: `payment.kind`, `payment.outcome`) + ASP.NET Core HTTP metrikleri. |
| Ödeme span'leri | `src/ClearPay.Infrastructure/Observability/TracedPaymentExecutors.cs` | `ITransferExecutor` / `IFundingExecutor` için **decorator**. Ledger kodu OpenTelemetry bilmez. Span'de alıcı e-postası, açıklama, IBAN yok. |
| RabbitMQ trace yayılımı | `src/ClearPay.Infrastructure/Observability/MessageTraceContext.cs` | `TraceContextPropagator` ile W3C `traceparent` header'ını mesaja yazar / okur. `StartPublish` (Producer span) ve `StartConsume` (Consumer span, parent = header). RabbitMQ.Client header'ları `byte[]` döndürdüğü için ikisi de desteklenir. **Henüz outbox publisher'a bağlanmadı** — RabbitMQ işi ayrı dalda; o kod bu yardımcıyı çağıracak. |

Ayarlar (`appsettings*.json` → `Observability`):

```json
"Observability": {
  "ServiceName": "clearpay-web",
  "SeqUrl": "http://localhost:5341",      // boş = Seq kapalı (sadece console)
  "OtlpEndpoint": "http://localhost:4317" // boş = OTLP export kapalı
}
```

`appsettings.json` (Production) boş değerlerle gelir; Development lokal yığını gösterir. Testler ikisini de boş geçer.
Yığın kapalıyken uygulama normal çalışır (Seq sink ve OTLP exporter sessizce düşer).

## Yığını çalıştırmak

```bash
docker compose -f docker-compose.observability.yml -p clearpay-obs up -d
dotnet run --project src/ClearPay.Web --launch-profile http
# iş bitince
docker compose -f docker-compose.observability.yml -p clearpay-obs down -v
```

```
ClearPay.Web ──Serilog──────────────▶ Seq :5341            (loglar)
     │
     └──OTLP gRPC :4317──▶ OTel Collector ──▶ Tempo         (trace)
                                   ├────────▶ Seq /ingest/otlp (trace, loglarla yan yana)
                                   └── :8889 ◀── Prometheus (metrik, scrape)
Grafana :3000 ──▶ Tempo + Prometheus (provisioned datasource + "ClearPay - Payments" dashboard)
```

| Servis | URL |
|--------|-----|
| Seq UI | http://localhost:8341 |
| Grafana | http://localhost:3000 → Dashboards → ClearPay → *ClearPay - Payments* |
| Tempo API | http://localhost:3210 (konteyner içi 3200; host'ta 3200 sık dolu olduğu için 3210) |
| Prometheus | http://localhost:9090 |

Yapılandırma dosyaları: `infra/observability/` (`otel-collector.yaml`, `tempo.yaml`, `prometheus.yml`, `grafana/`).

## Bir ödemenin trace'ini bulmak

Her yanıtta iki header var:

```
HTTP/1.1 201 Created
X-Correlation-Id: demo-ticket-1002
X-Trace-Id: a2ed33bdea8f0fd95a0cfed80b0f5476
```

İstemci (Flutter, üye işyeri, destek ekibi) bu ikisinden birini iletirse yeterli.

**1. Seq — ne oldu?** http://localhost:8341 arama kutusu:

```
TraceId = 'a2ed33bdea8f0fd95a0cfed80b0f5476'
CorrelationId = 'demo-ticket-1002'
UserId = '63b803c0-...' and StatusCode >= 400
@Level = 'Error' and RequestPath like '/api/%'
```

Request log satırı şu alanları taşır: `RequestMethod`, `RequestPath`, `StatusCode`, `Elapsed`, `Endpoint`,
`UserId`, `TraceId`, `SpanId`, `CorrelationId`, `Application`, `Environment`. Seq aynı TraceId'li span'leri de
(Collector üzerinden) gösterir; olaya tıklayıp *Trace* görünümüne geçilebilir.

API ile (otomasyon / doğrulama):

```bash
curl -G http://localhost:8341/api/events -H "Accept: application/json" \
  --data-urlencode "filter=@TraceId = 'a2ed33bdea8f0fd95a0cfed80b0f5476'"
```

**2. Tempo / Grafana — nerede zaman geçti?** Grafana → Explore → Tempo → TraceID'yi yapıştır, ya da:

```bash
curl http://localhost:3210/api/traces/a2ed33bdea8f0fd95a0cfed80b0f5476
```

Bir top-up için doğrulanmış span ağacı (lokal çalıştırmadan):

```
POST api/topup                      Microsoft.AspNetCore
└─ payment topup                    ClearPay.Payments   payment.outcome=Created, clearpay.correlation_id=…
   ├─ SELECT [IdempotencyRecord]    SqlClient
   ├─ SELECT [Wallet]               SqlClient
   └─ INSERT [AuditLog] [IdempotencyRecord] [LedgerEntry] [OutboxMessage]   ← tek SQL transaction
```

Son satır mülakat cümlesinin kanıtı: ledger, idempotency, audit ve outbox **aynı** batch'te.

TraceQL ile arama (Grafana Explore veya dashboard'daki tablo):

```
{ resource.service.name = "clearpay-web" && name =~ "payment.*" && span.payment.outcome != "Created" }
{ resource.service.name = "clearpay-web" && status = error }
{ resource.service.name = "clearpay-web" && duration > 500ms }
```

**3. Prometheus / dashboard — tek olay mı, trend mi?**

```promql
sum by (payment_kind, payment_outcome) (rate(clearpay_payments_total[5m]))
histogram_quantile(0.95, sum by (le, payment_kind) (rate(clearpay_payments_duration_milliseconds_bucket[5m])))
sum(rate(http_server_request_duration_seconds_count{http_response_status_code=~"5.."}[5m]))
```

## Hata analizi runbook'u

| Belirti | Önce bak | Sonra |
|---------|----------|-------|
| Müşteri "param çekildi ama yüklenmedi" diyor, elinde `X-Trace-Id` var | Seq: `TraceId = '…'` → StatusCode, Endpoint, UserId | Tempo'da aynı trace: `payment topup` span'i `payment.outcome` ne? `TimedOut` ise ledger'a satır **yazılmamış** olmalı — `INSERT … [LedgerEntry]` span'i yoksa doğru davranış. |
| Aynı istek iki kez geldi şüphesi | Seq: `RequestPath = '/api/transfers' and UserId = '…'` | İkinci istek `409` ve span'de `payment.outcome=Replay` olmalı; ikinci `INSERT [LedgerEntry]` olmamalı. |
| 5xx artışı | Dashboard "HTTP 5xx" + route paneli | Seq: `@Level = 'Error'` → exception; Tempo: `status = error` trace'i → hangi span (SQL? gateway?) hata verdi. |
| Yavaşlık | Dashboard "Payment p95 latency" | TraceQL `duration > 500ms` → en uzun child span (genelde SQL). İlgili sorgu metni span tag'inde (`db.query.text` ya da eski semconv'da `db.statement`; parametreli). |
| Gateway timeout oranı | `clearpay_payments_total{payment_outcome="TimedOut"}` | `BankGateway:SimulateTimeout` açık mı? Sahte gateway bunu kasıtlı üretir. |
| Mesaj kuyruğunda kaybolan iş | (RabbitMQ dalı birleşince) consumer span'i, producer ile **aynı TraceId**'de olmalı | `MessageTraceContext.StartPublish/StartConsume` çağrılmıyorsa trace kopar — kod incelemesinde kontrol. |

Genel sıra: **header → Seq (ne) → Tempo (nerede) → Prometheus (ne sıklıkta) → kod**.

## Kişisel veri kuralları

- Loga kart numarası, CVV, şifre, token, e-posta, TC kimlik yazılmaz. `SensitiveDataEnricher` sadece **son savunma hattı**dır; maskeleme exception mesajlarına uygulanmaz.
- Span tag'leri: `enduser.id` (opak Identity id), `payment.kind`, `payment.outcome`, `clearpay.correlation_id`, `clearpay.transfer_id`. Tutar ve alıcı span'de yok.
- Metrik etiketleri düşük kardinaliteli: sadece `payment_kind` ve `payment_outcome` (kullanıcı id'si metrikte yok).
- SQL span'lerinde EF'nin parametreli sorgu metni var, parametre değerleri yok.

## Bilinen sınırlar

- Yığın lokal; Azure App Service'te OTLP/Seq hedefi tanımlı değil (Production'da `Observability` boş).
- RabbitMQ publish/consume henüz `MessageTraceContext` kullanmıyor (ayrı dal).
- Hangfire job'ları ayrı trace olarak görünmez (Hangfire için resmi OTel instrumentation eklenmedi).
- Tempo tek process, 24 saat retention; üretim düzeni değildir.
