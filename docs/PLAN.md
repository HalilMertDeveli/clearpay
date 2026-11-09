# PLAN — ClearPay iş planı

Bu belge Orchestrator’a verilir. Komut: «sıradaki işi yap» / «devam». **Tek seferde tek TASK.**

Kaynak: kariyer planı (ClearPay kilit) + ekran listesi. Kod LED sitesine yazılmaz; bu repo ayrıdır.

## Stack (değişmez)

| Katman | Ne |
|--------|-----|
| Dil / runtime | C# 12, .NET 8 |
| Web | ASP.NET Core: Razor Pages + Controllers (JSON API), tek host |
| Mimari | Clean Architecture: `Web` / `Application` / `Domain` / `Infrastructure` |
| Veri | SQL Server, EF Core + Dapper + T-SQL (SP, view, indeks) |
| Kimlik | Identity + cookie (site) + JWT (API) |
| Doğrulama | FluentValidation + ProblemDetails |
| Log | Serilog + correlation id + `AuditLog` |
| Test | xUnit, FluentAssertions, WebApplicationFactory |
| Q1 kuyruk | Outbox tablosu + Hangfire retry (tasarım gün 1, işleyen TASK-11) |
| Q2 kuyruk | Redis, RabbitMQ (Compose gün 1 ayakta; canlı bağ Q2) |
| Q2 istemci | Flutter JWT (`mobile/clearpay`); C# motor durur; slnx dışı (T-061) |
| Entegrasyon | Sahte `BankGateway`: REST sonra SOAP |
| CI / canlı | GitHub Actions; Azure App Service (Linux) + Azure SQL |

Lokal her gün: Docker Compose (`web`, `sql`, sonra `redis`, `rabbitmq`).

## Fazlar

```
Faz 0  Docs + ajanlar          ← bu oturum (TASK-01 Done)
Faz 1  Görünen site            TASK-02 … TASK-03   giriş, kayıt, boş özet
Faz 2  Para motoru             TASK-04 … TASK-06   ledger, özet canlı, havale
Faz 3  Banka + geçmiş          TASK-07 … TASK-09   yükle/çek, SOAP, hareket/dekont
Faz 4  Ops + dağık fikir       TASK-10 … TASK-12   admin, outbox, Redis/Rabbit
Faz 5  Kanıt + canlı           TASK-13 … TASK-16   test, README, CI, Azure
```

## Task kabul kriterleri

### TASK-01 — Repo + MD + ajanlar
**Done bu oturum.** `docs/*`, `.cursor/rules/*`, İngilizce README iskeleti.

### TASK-02 — Solution iskeleti + layout
- Solution: `src/ClearPay.{Domain,Application,Infrastructure,Web}`, `tests/ClearPay.Tests`
- Docker Compose: SQL Server
- Razor layout: sol menü (Özet, Havale, Yükle/Çek, Hareketler; Admin gizli)
- Placeholder sayfalar 200 dönsün; henüz Identity yok
- `dotnet build` temiz
- **Ajan:** Architect onay → Coder → Tester

### TASK-03 — Giriş + Kayıt + boş cüzdan özeti
İlk ekran işi. Mockup’a yakın, kesin Figma değil.
- Giriş: e-posta, şifre, “Hesap oluştur”
- Kayıt: ad, e-posta, şifre, tekrar
- Identity cookie; rol `Musteri`
- Giriş sonrası **Cüzdan özeti:** bakiye `0,00 ₺`, bu ay giden/gelen `0`, son hareketler boş liste
- Validasyon sunucu tarafı
- **Ajan:** Coder → Tester

### TASK-04 — SQL model + ledger iskeleti
Tablolar: `User` (Identity), `Wallet`, `LedgerEntry`, `Transfer`, `IdempotencyRecord`, `AuditLog`, `OutboxMessage`
- EF migration; bakiyeye unique wallet (1 user = 1 wallet)
- İndeks: `LedgerEntry(WalletId, CreatedAt)`, `IdempotencyRecord(Key)` unique
- Çift kayıt kuralı Domain’de yazılı; henüz havale API yok
- **Ajan:** Architect (şema) → Coder → Payments gözden geçirir

### TASK-05 — Cüzdan özeti canlı
- Bakiye ledger’dan (veya denormalize `Wallet.Balance` + invariant test)
- Bu ay giden/gelen aggregate
- Son 5 hareket
- Donduk/aktif rozeti
- **Ajan:** Coder → Tester

### TASK-06 — Havale (mülakat omurgası)
- Ekran 4: alıcı, tutar, açıklama, kalan bakiye, Gönder / İptal
- `POST /api/transfers` + `Idempotency-Key`
- Aynı key tekrar → **409**; ikinci kesinti yok
- SQL transaction: − gönderen, + alıcı, transfer satırı, idempotency, audit, outbox insert
- Yetersiz bakiye / dondurulmuş / kendini / bulunamayan alıcı → 4xx, bakiye değişmez
- **Ajan:** Payments + Coder → Tester (409 ve invariant zorunlu)

### TASK-07 — Yükle / çek + sahte BankGateway REST
- Ekran 5
- Gateway: başarı / timeout (config veya query ile)
- Timeout: ledger **kesinleşmez**; outbox/iş kuyruğu kaydı kalır
- **Ajan:** Coder + Payments → Tester

### TASK-08 — SOAP gateway
- Aynı `IBankGateway` arayüzü; REST ve SOAP strategy
- Timeout/hata REST ile aynı sonuç modeline düşer
- **Ajan:** Coder → Tester

### TASK-09 — Hareketler + dekont
- Ekran 6–7: filtre, sayfalama, Dapper veya SP
- Dekont: taraflar, tutar, **correlation id**, zaman
- **Ajan:** Coder → Tester

### TASK-10 — Admin
- Ekran 8, rol `Admin`
- Kullanıcı dondur (cüzdan freeze)
- Başarısız kuyruk listesi + “kuyruğa al”
- Audit arama (kullanıcı, correlation id, tarih)
- **Ajan:** Coder → Tester

### TASK-11 — Outbox + Hangfire
- Worker: `Pending` outbox → işle → `Sent` / `Failed`
- Ledger commit olduysa mesaj kaybolmaz
- **Ajan:** Payments + Coder → Tester

### TASK-12 — Redis + RabbitMQ (lokal)
- Compose’a redis + rabbitmq
- Özet bakiyesi cache (invalidate havale sonrası)
- Outbox publisher Rabbit’e (Hangfire yedek kalabilir)
- Canlı Azure Redis/CloudAMQP **bu task’ta hesap açılmaz**
- **Ajan:** Coder → Tester

### TASK-13 — Test sertleştirme
- Ledger invariant, 409 replay, yetersiz bakiye, freeze, WebApplicationFactory API
- **Ajan:** Tester (gerekirse Coder)

### TASK-14 — İngilizce README + Swagger + CV maddesi
SPEC’teki 3 CV cümlesi README’de. Swagger’da 409 örneği.
- **Ajan:** Coder

### TASK-15 — GitHub + Actions
Build + test PR’da. Secret yok.
- **Ajan:** Deploy (kullanıcı remote/auth)

### TASK-16 — Azure App Service + Azure SQL
Açık URL. Redis/kuyruk canlısı Q2; Q1 ödeme senkron veya Hangfire in-process olabilir.
Hesap/DNS kullanıcı açar; ajan talimat yazar.
- **Ajan:** Deploy

## Mülakat hikâyesi (kod bunu kanıtlar)

| Soru | Cevap (kodda) |
|------|----------------|
| Neden 409? | Aynı `Idempotency-Key` = aynı niyet. İkinci HTTP başarı sanılırsa çift kesinti. 409 “zaten yapıldı”. |
| Neden transaction? | − ve + ayrı commit olursa bakiye bozulur. Ledger + idempotency + outbox tek commit. |
| Neden outbox? | HTTP timeout’ta client retry eder; banka/kuyruk “kayboldu” olamaz. Önce DB, sonra mesaj. |

## UI notu
Mockup’lar sohbette üretildi (giriş, özet, havale, hareketler, admin). Razor bunlara **yakın** olur; piksel kopya değil. Navy `#1B2A4A`, düz kurumsal, Türkçe.

## Q1 / Q2 canlı
- **Q1:** site + Azure SQL + Actions. Outbox tablosu var; kuyruk broker şart değil.
- **Q2:** Azure Cache for Redis + CloudAMQP. Satıcı ödemesi ayrı karar.

## Yasak
- LED repo’suna ClearPay kodu
- Scope dışı ekran (satıcı paneli, FIDS, Kafka)
- Hosting hesabını ajanın açması
- Aynı anda birden fazla TASK
- Ledger’ı “UPDATE Balance” ile audit’siz düzeltmek
