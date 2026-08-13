# CANLI — ClearPay Q1 public URL

Kaynak: `docs/CALISMA-PLANI.md` **Faz 5** (Actions + Azure). Bu belge **plan**; şimdi kod publish edilmez. Azure / DNS / ödeme hesabı **kullanıcı** açar; ajan talimat yazar.

**Orchestrator:** TASK-16 **başlatma** — kullanıcı Azure aboneliği yokken. Sıra: TASK-15 (GitHub Actions `dotnet test`) → abonelik + RG onayı → TASK-16 (açık URL). `dotnet test` kırmızıysa Done yok.

SEO/Ads: canlı URL **sonrası**; demo disclaimer (gerçek banka değil).

İnsan Azure/DNS checklist’i **bu dosyada**. `docs/SENIN-ISLERIN.md` başka ajanın (öğrenme); üzerine yazılmadı.

## Nerede

| Parça | Q1 | Q2 (sonra) |
|--------|----|------------|
| Web | Azure App Service **Linux**, .NET 8 | aynı |
| Veri | **Azure SQL** (ledger + Identity; SQLite canlıda yok) | aynı |
| CI/CD | GitHub Actions: `main` → build/test; TASK-16’da publish | aynı |
| Kuyruk | **Hangfire in-process** + outbox tablosu | Redis + Rabbit / CloudAMQP |
| Cache | yok | Azure Cache for Redis |

**Bölge:** **West Europe** (TR gecikmesi düşük). Alternatif: Canada Central (LED sitesi oradaydı; tutarlılık). Tercih West Europe.

Kaynak grubu önerisi: `rg-clearpay-weu`.

## Güzel URL (üç kademe)

### 1) Q1 ücretsiz / ucuz (şimdilik hedef)

App Service adı sırayla dene (küresel tekil):

1. `clearpay` → **https://clearpay.azurewebsites.net**
2. Doluysa `clearpay-wallet` → `https://clearpay-wallet.azurewebsites.net`
3. Doluysa `hm-clearpay` → `https://hm-clearpay.azurewebsites.net`

Path’ler **küçük harf**, query/path çirkinliği yok:

| Path | Ne |
|------|----|
| `/` | Cüzdan özeti (auth sonrası). Anonim → `/giris` |
| `/giris` | Giriş |
| `/kayit` | Kayıt |
| `/havale` | Havale |
| `/yukle-cek` | Yükle / çek |
| `/hareketler` | Hareketler |
| `/admin` | Admin (rol `Admin`) |
| `/api/...` | JSON API (`/api/health`, sonra `/api/transfers`) |
| `/swagger` | Swagger; **prod’da kapatılabilir** |

HANDOFF (Coder TASK-03): bugün giriş `/Account/Login`, kayıt `/Account/Register`. Canlı path’ler `/giris` ve `/kayit` olmalı — **Coder** `@page` ekler. Deploy Razor’a dokunmaz. `/havale` `/yukle-cek` `/hareketler` zaten küçük harf.

### 2) Özel domain (kullanıcı satın alır)

Öneri sırası:

1. `clearpay.app`
2. `pay.halilm.dev`
3. `clearpay.<onun-domain>`

DNS (kullanıcı paneli):

- `www` (veya `pay` / `clearpay` alt alanı): **CNAME** → `<app>.azurewebsites.net`
- Apex (`clearpay.app`): Azure **A / ALIAS** (registrar destekliyorsa ALIAS; değilse Azure’un verdiği IP / Traffic Manager değil, App Service custom domain sihirbazı)

HTTPS: **Azure managed certificate** (App Service TLS). Let’s Encrypt elle gerekmez.

Ajan domain satın almaz, registrar hesabı açmaz.

### 3) Path tasarımı (sabit)

- Site kökü = cüzdan özeti (cookie sonrası).
- Login `/giris`, kayıt `/kayit`.
- Admin `/admin` + rol.
- API `/api/...`. Swagger `/swagger` — Production’da `ASPNETCORE_ENVIRONMENT=Production` ile kapalı tutulabilir (TASK-14/16).

## Senin yapacakların vs ajan

**Kullanıcı (hesap / para / sır):**

1. Azure aboneliği: öğrenci veya pay-as-you-go. **F1** denemek serbest.
2. Kaynak grubu onayı: West Europe, ad `rg-clearpay-weu`.
3. Domain (isteğe bağlı kademe 2): satın al + DNS CNAME/ALIAS.
4. Portal → App Service → **Configuration / App Settings** (veya Connection strings) — sen yapıştırırsın. Repo’ya yazılmaz.
5. GitHub repo zaten `HalilMertDeveli/clearpay`. Azure’a publish için GitHub bağlantısı / publish profile **sen** onaylarsın (TASK-16).

**Ajan (kod / yaml; hesap açmaz):**

- TASK-15: `.github/workflows/` — `dotnet restore` → `build` → `test`. Secret yok.
- App Settings **slot isimleri** (aşağıda). TASK-16’da publish workflow + Linux App Service talimatı.
- Hangfire Q1: aynı process, SQL storage (Azure SQL). Redis/Rabbit canlı bağ **Q2**.

## App Settings isimleri (değer yok)

Portal’da oluştur; **değerleri git’e koyma**.

| Ad | Ne |
|----|----|
| `ConnectionStrings__ClearPay` | Azure SQL (ledger + Identity) |
| `Jwt__SigningKey` | API JWT (32+ rastgele bayt) |
| `ASPNETCORE_ENVIRONMENT` | `Production` |
| `Hangfire__WorkerEnabled` | Q1 `true` (in-process) |

Connection string Azure SQL’den kopyalanır (`...database.windows.net`; `ClearPay_Dev1!` kullanılmaz).

## Maliyet (uyarı)

| Kaynak | SKU | Not |
|--------|-----|-----|
| App Service | **F1** ücretsiz | Always On **yok** — ilk istek soğuk (10–30 sn). Demo için kabul. |
| App Service | **B1** | Always On var; ücretli. Mülakat demosu F1 yeter. |
| Azure SQL | **Basic** veya **Serverless** | Serverless idle’da ucuz; ilk sorgu uyanır. |

F1 + soğuk başlama: tarayıcıda ilk açılış yavaş görünebilir; `/api/health` ile ısındır.

Aylık kaba (2026, West Europe, F1 + SQL Basic/Serverless düşük kullanım): web **0 ₺**, SQL birkaç dolar mertebesi. Kesin rakam portal fiyatlandırıcıda.

## Sıra (şimdi deploy yok)

```
TASK-03 … TASK-14   site + ledger + test + README/Swagger
TASK-15             GitHub Actions: restore/build/test (publish yok)
Kullanıcı           Azure aboneliği + RG onayı
TASK-16             App Service Linux + Azure SQL + https://….azurewebsites.net
(isteğe bağlı)      özel domain + managed cert
Q2                  Redis + Rabbit — ayrı karar; TASK-16 şartı değil
```

TASK-16 kabul: tarayıcıda açık URL, giriş, boş/canlı özet. Redis şart değil.

## Yasak

- Ajan Azure / DNS / domain / ödeme hesabı açmaz.
- Repo’ya connection string, JWT, SQL SA, publish profile.
- TASK-16’yı abonelik yokken başlatmak.
- Canlıda lokal `ClearPay_Dev1!` veya SQLite `App_Data`.
