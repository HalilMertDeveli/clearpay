# HANDOFF — ajan konuşma defteri

Kardeş ajanlar buraya **append** eder. SPEC/PLAN/TASKS yerine geçmez. Kullanıcı checklist’i: `docs/SENIN-ISLERIN.md`. Bölüm silme / üzerine yazma. Tartışma ve karar: `docs/TARTISMA.md` (buraya yazılmaz).

## 2026-08-13 — Deploy (Azure + Compose Redis/Rabbit)

- **OWN:** `.github/workflows/ci.yml`, `azure-deploy.yml`, `infra/main.bicep`, `infra/q2.bicep`, `infra/deploy.ps1`, `docker-compose.yml`, `docs/CANLI.md`, `docs/DEPLOY.md`, `AddClearPayIdentity` Production SQL.
- TASK-15 Done (CI yaml). TASK-16 Todo — açık URL yok; ajan abonelik açmadı.
- Lokal Compose: SQL + Redis + Rabbit. Uygulama Redis/Rabbit’e bağlı değil (TASK-12). Q2: Redis Bicep + CloudAMQP talimatı.
- Production Identity `UseSqlServer(ClearPay)`; Development SQLite. Cookie `LoginPath` `/giris`. Secret git’te yok.
- **Kullanıcı:** `az login` → `.\infra\deploy.ps1` → GitHub `AZURE_WEBAPP_PUBLISH_PROFILE` + `AZURE_WEBAPP_NAME`.
- LED repo / Kafka / AWS / GCP yok.

## 2026-08-13 — Destek (demo FAQ)

- **OWN:** `docs/DESTEK.md`, `.cursor/rules/destek.mdc`. `docs/AGENTS.md` ve `src/` dokunulmadı. TASKS yok (ürün TASK değil).
- **DESTEK.md:** dürüst — banka yardım masası değil. FAQ: nasıl kayıt, demo para (gerçek TL yok), 409 (TASK-06’da HTTP), timeout (TASK-07/11), Azure yokken lokal (`:5153`).
- README Docs’a DESTEK satırı. Coder/Payments’a iş emri yok; FAQ kodu takip eder.
- LED teknik destek reposuna dokunulmadı. Ads/Azure hesabı yok.

## 2026-08-13 — İK (aday Halil)

- **OWN:** `docs/IK.md`, `.cursor/rules/ik.mdc`. `docs/AGENTS.md` ve `src/` dokunulmadı.
- İşe alım yok. Aday paketi: README İngilizce CV 3 madde (kopya), 15/30 dk script, 409/tx/outbox cevap, kapı Intertech/Softtech/Bileşim/TAV/İGA/uni BT/sanayi/Turkcell .NET; **hayır** Trendyol/THY/Garanti Java, ASELSAN C++.
- TASK-06/11 bitmeden “kanıtladım” yok. Sales `SATIS.md` pitch durur; İK ezber + firma filtresi.
- README Docs’a İK linki. `SENIN-ISLERIN` mülakat maddesi `IK.md` gösterir.

## 2026-08-13 — Architect (SOLID ports)

- **OWN:** `src/ClearPay.Application/Ports|Wallets|Transfers|Banking`, `src/ClearPay.Infrastructure/{DependencyInjection,Time,Persistence,Banking}`. Domain/Ledger ve Razor dokunulmadı. Havale API yok.
- Portlar: `IWalletReader`, `ITransferExecutor`, `IIdempotencyStore`, `IClock`, `IBankGateway`. Stub’lar `NotImplementedException` (TASK-05/06/07/08). `SystemClock` gerçek. Default gateway: `RestBankGateway`; `SoapBankGateway` kayıtlı, bağlanmamış.
- **Coder:** `Program.cs` — `using ClearPay.Infrastructure.DependencyInjection;` ve `builder.Services.AddClearPay();` (Architect Program.cs yazmadı). PageModel’e para kuralı koyma; `IWalletReader` / `ITransferExecutor` enjekte et.
- `docs/ARCHITECTURE.md` SOLID haritası güncellendi.

## 2026-08-13 — Architect

- `docs/ARCHITECTURE.md` origin/main’de (`62bbddd`): katmanlar, SPEC ekran haritası, tek host, cookie sonra JWT, ledger Web dışında, Q1 Hangfire/outbox vs Q2 Redis/Rabbit.
- TASK-04 Coder/Payments: 1 user = 1 wallet; Identity SQLite ayrı; ledger SQL Server; `IdempotencyRecord.Key` unique; `LedgerEntry(WalletId, CreatedAt)` indeks.
- Çatışma: pull temizdi; Architect commit push edildi. Sonra Deploy `3df4c57` HANDOFF’u overwrite etti (Architect bölümü silindi — burada geri eklendi). `stash pop` README/DEPLOY/compose yüzünden durdu; Coder TASK-03 WIP `stash@{0}` (`wip-other-agents-before-architect-pull`). Architect ikinci push yapmadı (kirli ağaç + Deploy commit).

## 2026-08-13 — docs-ogrenme

- **OWN:** `docs/OGRENME.md`, `docs/SENIN-ISLERIN.md`; `docs/AGENTS.md` ve `README.md` Docs’ta tek satır link.
- `src/`, `tests/`, `docker-compose`, `ARCHITECTURE.md` dokunulmadı.
- Öğrenme defteri: ev → kilit → para motoru; 409/tx/outbox henüz yok (bilinçli). HANDOFF burada ajan defteri olarak geçiyor.
- README Docs satırları OGRENME + SENIN-ISLERIN; Coder README’yi TASK-03 ile commit ediyorsa bu iki maddeyi koru.
- Dosyalar yazıldı: `docs/OGRENME.md` (Senin yapman gerekenler önde), `docs/SENIN-ISLERIN.md` (VS checklist). src/tests/compose’a dokunulmadı.

## 2026-08-13 — Deploy

- **OWN:** `docker-compose.yml`, `.dockerignore`, `docs/DEPLOY.md` (lokal netleştirme). Azure hesabı açılmadı. Razor/CSS/Domain dokunulmadı.
- `.dockerignore`: `bin/`, `obj/` (ve git/IDE/secret).
- Compose: SQL Server aynı (1433, volume, SA env). Web Compose servisi değil — host’ta `dotnet run --project src/ClearPay.Web --launch-profile http` (5153). Redis/Rabbit TASK-12.
- Bu makinede `docker` PATH’te yok; YAML SQL-only bırakıldı.
- Follow-up: `docker-compose.yml` + `docs/DEPLOY.md` bu commit’te. HANDOFF’ta Architect / Payments / Tester bölümleri korundu.

## 2026-08-13 — Payments (TASK-04 Domain)

- **OWN:** `src/ClearPay.Domain/Ledger/**` only. Web/Razor/CSS, tests, compose, TASKS, EF/Infrastructure dokunulmadı. `DomainAssembly.cs` ve `Identity/AppRoles.cs` Coder’da — bekletildi, yeni dosya eklendi.
- POCOs: `Wallet` (1 user = 1 wallet, `IsFrozen`, bakiye kolonu yok), `LedgerEntry` (signed Amount, `PairId`), `Transfer`, `IdempotencyRecord` (`Key` unique → 409), `AuditLog`, `OutboxMessage`.
- Invariant: `LedgerPair.Create` +/− çift; `CreateRefund` ters kayıt. `MoneyTransaction.RequiredInserts` = debit, credit, Transfer, Idempotency, Audit, Outbox — tek SQL transaction. `UPDATE Balance` helper yok.
- Coder EF (SQL Server, Identity SQLite değil): unique `Wallet.UserId`, unique `IdempotencyRecord.Key`, index `LedgerEntry(WalletId, CreatedAt)`. İsimler `LedgerSchema`. Duplicate key → 409. Havale API TASK-06.
- net8.0 Domain derlenmeli. Commit: Domain Ledger + bu HANDOFF bölümü.

## 2026-08-13 — Tester

- **OWN:** `tests/ClearPay.Tests/AuthOrUiTests.cs` only. `PlaceholderPagesTests.cs` dokunulmadı. `src/**` yok.
- Identity/login henüz yok. Mevcut menü + `/api/health` 200. `/Account/Login` 404 olunca giriş/kayıt/cüzdan assert no-op; Coder login basınca aynı testler 200 + korumalı rota redirect + kayıt sonrası `0,00 ₺` bekler.
- TASK-06 409 testi `[Fact(Skip=...)]`. Identity `AuthorizeFolder` gelince `PlaceholderPagesTests` anonim 200 kırılır — Coder o dosyayı silsin veya Tester’a bırakın.
- `dotnet test` sonucu bir sonraki nota.

## 2026-08-13 — Payments (Domain files)

- `src/ClearPay.Domain/Ledger/**` yazıldı; `dotnet build` ClearPay.Domain net8.0 yeşil. EF/Web yok. Coder `AppRoles` / `DomainAssembly` dokunulmadı.

## 2026-08-13 — Coder (Web) TASK-03

- **OWN:** `src/ClearPay.Web/**` (+ bu bölüm). Identity UI ve cookie host Web’de.
- Giriş `/Account/Login` (200 anonim): wordmark ClearPay, E-posta, Şifre, **Giriş**, link **Hesap oluştur**.
- Kayıt `/Account/Register`: Ad, e-posta, şifre, şifre tekrar, **Hesap oluştur** → `Musteri` + cookie → `/`.
- Özet `/` (cookie): bakiye **0,00 ₺**, bu ay giden/gelen **0,00 ₺**, son hareketler boş, CTA Havale / Yükle / Çek.
- Form kabukları (TASK notu yok): `/havale`, `/yukle-cek` (`#yukle` `#cek`), `/hareketler`. Gönder/Yükle/Çek disabled (bakiye 0).
- Sol menü: Özet, Havale, Yükle/Çek, Hareketler. Admin gizli. Footer: Demo — sahte banka gateway. Navy `#1B2A4A`, Inter, gölge/gradient yok. Mobil sidebar kapanır.
- `Program.cs`: `AddClearPayIdentity`, `UseAuthentication`, `AuthorizeFolder("/")`, Account + Error anonymous. SQLite `App_Data/identity.db`.
- Tester: anonim GET `/` `/havale` `/yukle-cek` `/hareketler` → 302 `/Account/Login`. GET login/register/health → 200.
- Not: Web, Infrastructure Identity + Application FluentValidation’a bağlanır. Site: http://localhost:5153

## 2026-08-13 — Designer (TASARIM / MARKA)

- **OWN:** `docs/TASARIM.md`, `docs/MARKA.md`, `.cursor/rules/designer.mdc`, `src/ClearPay.Web/wwwroot/css/brand.css` (ek token). Coder Razor **yeniden yazılmadı**. `site.css` dokunulmadı.
- **Landed:** TASARIM + MARKA evet. One-liner: ClearPay — demo cüzdan, sahte banka. Disclaimer duruyor.
- Sales `FARK.md` / `SATIS.md` fold: MARKA’ya 2 çizgi (çift kayıt + 409; outbox + REST/SOAP). Papara rakibi yok.
- **Coder: link brand.css** — `_Layout.cshtml` ve `_AuthLayout.cshtml` içinde `site.css` satırının hemen altına:
  `<link rel="stylesheet" href="~/css/brand.css" asp-append-version="true" />`
- **Coder: sınıflar / metin** (Razor’u Designer yazmaz):
  1. `.brand-name` yanına isteğe `wordmark` (aynı span).
  2. `_AuthLayout` tagline: `Dijital cüzdan` → **Demo cüzdan**.
  3. Özet / hareketler boş satır: `empty-cell` içinde `<p class="empty-title">Henüz hareket yok</p><p class="empty-hint">İlk havaleniz veya yüklemeniz burada görünür.</p>` (hareketler tablosunda title: `Bu dönemde hareket bulunmuyor.`).
  4. Identity/havale işi yok; Gönder disabled kalsın.
- AGENTS.md Designer satırı eklendi. Google Ads açılmadı.

## 2026-08-13 — Sales

- **OWN:** `docs/SATIS.md`, `docs/FARK.md`, `.cursor/rules/sales.mdc`. `src/` / CSS / TASKS dokunulmadı. Ads harcaması yok.
- **SATIS.md:** 15–30s + 2 dk pitch, CV maddeleri (EN/TR), README/demo copy, Q&A. Ses = hiring manager; Papara GTM yok. Kilit: Demo — sahte banka gateway.
- **FARK.md:** tablo — Papara/Tosla/Paycell (cüzdan), iyzico/PayTR (PSP), banka havale UX, öğrenci ASP.NET CRUD. Sonra 6 fark: ledger, 409, outbox, REST+SOAP mock, audit/freeze/correlation, dürüst demo. Mimari ASCII. FAST/BOA/POS/pazaryeri yok.
- **AGENTS.md:** Sales satırı eklendi.
- Designer / SEO: `FARK.md` § One-liner’lar. Coder: isteğe bağlı footer **Neden ClearPay** sonra (pazaryeri yok). README Docs’a SATIS/FARK linki bu commit’te.

## 2026-08-13 — SEO/Ads

- **OWN:** `docs/SEO.md`, `docs/ADS.md`, `.cursor/rules/seo.mdc`, `wwwroot/robots.txt`, `wwwroot/sitemap.xml`; `docs/AGENTS.md` SEO satırı; `docs/SENIN-ISLERIN.md` madde 10. Razor/CSS/TASARIM/MARKA dokunulmadı.
- **SEO.md:** title/description şablonları (her metinde Demo), h1 hiyerarşisi, CANLI path `/giris` `/kayit`, sitemap URL’leri, robots kuralları, `SoftwareApplication` JSON-LD + demo disclaimer, Search Console + GA4 adımları **kullanıcı** açar. Host placeholder: `https://clearpay.azurewebsites.net`.
- **ADS.md:** kampanya **yalnızca** canlı Azure URL sonrası. Anahtar: `ASP.NET Core cüzdan demo` / portföy — `ucuz havale` yok. 3 reklam metni, başlıkta Demo. Negatif: kredi, gerçek IBAN, Papara alternatif. Hesap/harcama yok.
- Ses: `docs/MARKA.md` (Designer; dosya henüz yok) — kilit cümle footer: Demo — sahte banka gateway. TASARIM’a savaş yok.
- **Coder (layout dokununca):** `_Layout.cshtml` + `_AuthLayout.cshtml` — meta description + canonical + OG. Tags: `docs/SEO.md` “Coder’a meta”. `@page "/giris"` / `"/kayit"` CANLI’da Coder; SEO Pages/*.cshtml ezmez. Cookie sayfaları `noindex`.
- Ads/Search Console/Analytics hesabı açılmadı. TASK-16 URL yokken kampanya yok.

## 2026-08-13 — Coder status (yönetici)

- **Identity:** kodda VAR (cookie, rol `Musteri`, SQLite `App_Data/identity.db`). Henüz origin/main’de commit yok; working tree kirli.
- **Sayfalar:** `/giris` + `/Account/Login`, `/kayit` + `/Account/Register`, `/` özet (0,00 ₺), `/havale`, `/yukle-cek`, `/hareketler`, `/api/health`. Admin yok.
- **Stash:** `stash@{0}` `wip-other-agents-before-architect-pull` — Coder pop etmedi (kardeş ajan WIP).
- **Bloklar:** `docs/TASARIM.md` / `brand.css` henüz yok; layout `brand.css` bekliyor. Designer token gelince uygulanacak, Identity silinmeyecek. `dotnet test` bu nottan sonra.
- **Sıradaki:** TASK-03 yeşil build+test; TASARIM gelirse token; commit Web+Identity+test smoke.

## 2026-08-13 — Deploy status (CANLI + compose)

- **CANLI.md:** yazıldı (`docs/CANLI.md`). Plan only — publish yok. West Europe, App Service Linux + Azure SQL, Hangfire in-process Q1. URL: `https://clearpay.azurewebsites.net` (yedek `clearpay-wallet` / `hm-clearpay`). Path: `/` `/giris` `/kayit` `/havale` `/yukle-cek` `/hareketler` `/admin` `/api/...`. Azure hesabı **açılmadı**.
- **Orchestrator:** TASK-16 **başlatma** — kullanıcı Azure aboneliği yokken. TASK-15 Actions önce. `dotnet test` kırmızıysa Done yok.
- **Compose:** SQL-only ayakta (1433, volume, SA env). Web Compose servisi değil — host `dotnet run` :5153. Redis/Rabbit TASK-12. `.dockerignore` `bin/`/`obj/` origin’de.
- **DEPLOY.md:** CANLI’ye işaret ediyor. `SENIN-ISLERIN.md` öğrenme ajanının; Azure/DNS checklist CANLI içinde. Razor dokunulmadı (`/Account/Login` → canlı `/giris` Coder).

## 2026-08-13 — Öğrenme (status)

- **Durum: done (origin/main).** `docs/OGRENME.md` + `docs/SENIN-ISLERIN.md` push edildi: `a4f9400` (içerik), `739c801` (README Docs + `docs/AGENTS.md` link).
- **OGRENME.md:** neden-defteri (SPEC/PLAN kopyası değil). Önde **Senin yapman gerekenler** (8 madde). Kararlar: ayrı repo, public GitHub, tek TASK, ev→kilit→para motoru, cookie sonra JWT, 409/tx/outbox henüz yok (bilinçli). Mülakat üçlüsü, dosya haritası, yol `ClearPay.slnx` / localhost:5153. HANDOFF ajan defteri olarak geçiyor.
- **SENIN-ISLERIN.md:** kısa VS checklist (9 madde) — VS yolu, Docker Desktop, :5153, GitHub hazır, sır git’e yok, Azure/DNS sen, App Settings sen, kod/TASK senin işin değil.
- **Bekleyen:** `docs/CALISMA-PLANI.md` henüz diskte yok; OGRENME dosya haritasına link **eklenmedi**. Plan land edince tek satır eklenir. `src/` `tests/` compose dokunulmadı.

## 2026-08-13 — Payments (CALISMA-PLANI / TASK-04)

- Kaynak: `docs/CALISMA-PLANI.md` (henüz diskte yok). TASK-04 test omurgası: ledger invariant unit (sum +/− = balance). POCOs yeniden yazılmadı.
- Eklendi: `LedgerPair.NetOf` — bakiye = imzalı satır toplamı. Tester `tests/` unit (EF yok). `UPDATE Balance` yok. Havale API yok (TASK-06).
- Coder `AppRoles` / `DomainAssembly` / Web dokunulmadı.

## 2026-08-13 — SEO/Ads status (yönetici)

- **SEO.md:** yazıldı. Title/description (her metinde Demo), h1, CANLI `/giris` `/kayit`, robots/sitemap URL, JSON-LD `SoftwareApplication` + demo disclaimer, Search Console + GA4 adımları kullanıcıya. Host: `https://clearpay.azurewebsites.net` (değişir).
- **ADS.md:** yazıldı. Kampanya **yalnızca** canlı Azure URL sonrası. Portföy kelimeleri (`ASP.NET Core cüzdan demo`); `ucuz havale` yok. 3 metin, başlıkta Demo. Negatif: kredi, gerçek IBAN, Papara alternatif. Hesap/harcama yok.
- **Dosyalar:** `.cursor/rules/seo.mdc`, `wwwroot/robots.txt`, `wwwroot/sitemap.xml`, AGENTS SEO satırı, SENIN-ISLERIN madde 10. Razor/TASARIM/MARKA dokunulmadı (`MARKA.md` henüz yok; ses = footer).
- **Coder’a:** layout’a meta description + canonical (`docs/SEO.md`). `/giris` `/kayit` Coder. Ads/SC/GA4 hesabı açılmadı.

## 2026-08-13 — Sales

- **OWN:** `docs/SATIS.md`, `docs/FARK.md`, `.cursor/rules/sales.mdc`. `src/` / CSS / TASKS dokunulmadı. Ads harcaması yok.
- **SATIS.md:** yazıldı. 15–30s + 2 dk pitch, CV maddeleri (EN/TR), README/demo copy, Q&A. Ses = hiring manager; Papara GTM yok. Kilit: Demo — sahte banka gateway.
- **FARK.md:** yazıldı. Tablo — Papara/Tosla/Paycell (cüzdan), iyzico/PayTR (PSP), banka havale UX, öğrenci ASP.NET CRUD. Sonra 6 fark: ledger, 409, outbox, REST+SOAP mock, audit/freeze/correlation, dürüst demo. Mimari ASCII. FAST/BOA/POS/pazaryeri yok.
- **AGENTS.md:** Sales satırı eklendi. README Docs: SATIS + FARK link.
- Designer / SEO: `FARK.md` § One-liner’lar. Coder: isteğe bağlı footer **Neden ClearPay** sonra (pazaryeri yok).

## 2026-08-13 — Designer status (yönetici)

- **TASARIM.md:** landed (`docs/TASARIM.md`). Navy `#1B2A4A`, Inter, gölge/gradient/emoji yok. Giriş / özet / havale kompozisyon + mobil + boş durum.
- **MARKA.md:** landed. One-liner: **ClearPay — demo cüzdan, sahte banka.** Disclaimer duruyor. FARK/SATIS’ten 2 çizgi fold (çift kayıt+409; outbox+REST/SOAP). Papara rakibi yok.
- **brand.css:** additive tokens `wwwroot/css/brand.css`. `site.css` / Razor dokunulmadı. Coder: layout’a link (yukarıdaki Designer HANDOFF).
- Coder status’taki “TASARIM / brand.css henüz yok” **eski**; dosyalar diskte. Identity silinmedi. Google Ads yok.

## 2026-08-13 — Orchestrator (yönetici)

- Kaynak: `docs/CALISMA-PLANI.md`. Değerlendirme: `docs/YONETICI-RAPORU.md`.
- **Sales wedge (FARK):** Çift kayıt, 409, outbox. Demo — sahte banka gateway.
- **SEO Done:** `docs/SEO.md` + `docs/ADS.md`. Ads yalnızca canlı URL. SC/GA4/Ads kullanıcı. Coder `_Layout` düzenlerken meta + canonical (yeni TASK değil).
- **Payments yeşil:** Domain + `LedgerPair.NetOf` (`e2a5320`). Rewrite yok. Havale TASK-06.
- **Architect kapısı:** Application portları (`IBankGateway`, `IWalletReader`, `ITransferExecutor`, DIP). Canvas + SOLID. Coder TASK-03 Identity/UI devam; **PageModel’de ledger/transfer yok.** Domain durur.
- Kritik yol: **TASK-03**. TASK-16 / Azure / Ads harcama yok.

## 2026-08-13 — Yönetici değerlendirmesi

Yönetici değerlendirmesi: `docs/YONETICI-RAPORU.md`

## 2026-08-13 — Sales (wedge)

- **OWN:** `docs/FARK.md` (asıl), `docs/SATIS.md`, `.cursor/rules/sales.mdc`. Razor yok. Ads yok. FAST lisansı uydurulmadı.
- **Tek tercih sebebi:** mutabakat-öncelikli defter. Cümle (FARK üstü + SATIS başlık): her kuruşun +/− satırı ve correlation id’si sizin defterinizde — Papara/iyzico/FAST/kart “bakiye güncellendi” der.
- Kapalı devre (yemek/avans) sahne; pazaryeri TASK yok. Developer API = aynı motorun mülakat cümlesi, FAST yerine ürün değil.
- **FARK.md:** kimin sorunu / bugün / neden / ne değiliz; tek tablo onlar/biz/mülakat (cüzdan + PSP + FAST + 3DS + CRUD). ininal + Stripe kategoride.
- **SATIS.md:** 15s pitch wedge ile. Designer/SEO: FARK one-liner. Coder: isteğe bağlı **Neden ClearPay** footer sonra.
- Tüketici Papara yerini almıyoruz. README Docs’a SATIS/FARK linki bu commit’te.

## 2026-08-13 — Designer DONE

- Origin `666dd32`: `docs/TASARIM.md`, `docs/MARKA.md`, `wwwroot/css/brand.css`. Relaunch yok.
- Coder TASK-03: `_Layout` / `_AuthLayout`’a `brand.css` link (tek iş). Razor’u Designer yazmaz.
- Rapor: Designer **yeşil** (`docs/YONETICI-RAPORU.md`).

## 2026-08-13 — Öğrenme (KRONIK)

- **OWN:** `docs/KRONIK.md` (asıl okuma), `docs/OGRENME.md` (üstte KRONIK link), `docs/SENIN-ISLERIN.md` + `docs/ODEME-SENIN.md` (ödeme nasıl). README/AGENTS birer satır. `src/` yok.
- Kullanıcı istedi: başından her şey tek dokümanda. KRONIK 13 bölüm (ne/neden/öğren) + okuma sırası. TASK-03 **WIP** diye yazıldı.
- Ödeme: lisans/Papara/FAST yok; demo akış Docker → 5153 → 409 sonra. Ads “ucuz havale” yok.
- Commit: yalnızca bu docs.

## 2026-08-13 — Tester (NetOf / TASK-04)

- **OWN:** `tests/ClearPay.Tests/LedgerPairTests.cs`, `tests/ClearPay.Tests/ClearPay.Tests.csproj` (Domain project ref). Domain/Web dokunulmadı.
- **Geçti:** 8/8 `LedgerPairTests` — `NetOf` bakiye = imzalı satır toplamı (40 − 15 → from −25 / to +25; yabancı cüzdan ve boş liste 0). `Create` +/− toplam 0; `CreateRefund` ters; `WouldGoNegative`; frozen `CanDebit`.
- **Kanıt:** havale 40 ₺ + ters 15 ₺ → `NetOf(from)=-25`, `NetOf(to)=25`, iki net toplamı 0. `UPDATE Balance` yok.
- **Kaldı (Web, Coder WIP):** tam `dotnet test` Application Identity + FluentValidation / TestHost `/api/health` PipeWriter (.NET 10 testhost) yüzünden kırmızı olabilir. Ledger unit yeşil. TASK-06 409 skip duruyor.
- Coder: `appsettings.json` duplicate `ConnectionStrings:Identity` olursa host ayağa kalkmaz (daha önce görüldü; şu an dosya tek key).

## 2026-08-13 — PR (yeni)

- **OWN:** `docs/PR.md` only. Razor/CSS/Domain/TASKS yok. Ads harcaması yok. Azure hesabı yok.
- Konu: internette yayın (CANLI planı) + dürüst sıralama. Anahtar: ClearPay **demo** / ASP.NET cüzdan / ledger 409 — **Papara alternatif / ucuz havale #1 yok**.
- Coder **TASK-03** durmaz. SEO.md ile çakışma: PR yayın hikâyesi, SEO title şablonları.
- Rapor: PR **sarı** (`docs/YONETICI-RAPORU.md`).

## 2026-08-13 — PR (playbook landed)

- **OWN:** `docs/PR.md` (asıl), `.cursor/rules/pr.mdc`. `src/` / Razor / CSS / TASKS / SEO.md / ADS.md **dokunulmadı**. Google/Azure hesabı açılmadı.
- **Dürüst sıra:** “havale” / “Papara” #1 **yok** (ücretli tüketici araması + lisans). Kazanılır: canlı URL, `ClearPay ASP.NET`, `idempotent wallet .NET`, repo, isim+ClearPay.
- **Beş kapı:** (1) `https://clearpay.azurewebsites.net` + HTTPS, TASK-15→16 kullanıcı Azure (`CANLI.md`) (2) SC/sitemap/robots/title/GA4 — ayrıntı `SEO.md` (3) README EN + 1 LinkedIn/Medium + GitHub topics (4) GitHub, LinkedIn aday, isteğe Ads yalnızca URL sonrası başlıkta Demo (`ADS.md`; “ucuz havale” yok) (5) checklist: SC **kullanıcı** doğrular; meta/sitemap ajan; Coder layout meta.
- **Launch günü:** 1 yeşil build 2 Azure URL 3 Search Console 4 LinkedIn 5 isteğe Ads.
- **AGENTS.md** PR satırı güncellendi. `SENIN-ISLERIN.md` “Canlı + Google” maddeleri. README Docs’a PR link.
- Coder: Razor yok; meta hâlâ `SEO.md` HANDOFF. Deploy: TASK-16 hesap yokken başlama. SEO ajanı: SEO.md yeniden yazılmadı.

## 2026-08-13 — Sales DONE / Tester DONE (yönetici)

- **Sales:** wedge origin `FARK.md` / `SATIS.md` — her kuruşun +/− satırı ve correlation id sizin defterinizde; onlar bakiye güncellendi der. Papara tüketici yerine geçmiyoruz. Relaunch yok.
- **Tester:** 8/8 `LedgerPairTests` `NetOf` origin. Tam solution Coder Identity derlenene kadar kırmızı olabilir. Relaunch yok.
- Coder **TASK-03 only** — ekstra iş yok.
- Rapor RAG güncellendi: `docs/YONETICI-RAPORU.md`.

## 2026-08-13 — Organizasyon (kullanıcı)

Kullanıcı org: Yönetim, Ürün, Yazılım, Tasarım, Kalite, Destek, Satış, Pazarlama, İK, Finans.

| Org | Ajanlar | Owns (beklenen) |
|-----|---------|-----------------|
| **Yönetim** | Orchestrator | HANDOFF, TASKS, CALISMA-PLANI, YONETICI-RAPORU |
| **Ürün** | Product (yeni) | `docs/ORGANIZASYON.md` / ürün notu — SPEC ekranı eklemez |
| **Yazılım** | Coder, Architect, Payments | Web / ARCHITECTURE+ports / Domain Ledger |
| **Tasarım** | Designer | TASARIM, MARKA, brand.css — **Done** |
| **Kalite** | Tester | tests — LedgerPair 8/8 origin |
| **Destek** | Destek (yeni) | destek docs; lisanslı banka değil |
| **Satış** | Sales | FARK, SATIS — **Done** (wedge origin) |
| **Pazarlama** | SEO, PR, Pazarlama (yeni) | SEO/ADS, PR.md; Ads harcama yok |
| **İK** | IK (yeni) | IK docs; Coder’a iş açmaz |
| **Finans** | Finans (yeni) | finans docs; gerçek PSP değil |
| **Öğrenme** | Öğrenme | OGRENME, SENIN-ISLERIN, KRONIK |

`docs/ORGANIZASYON.md` **landed** — harita orada. Coder **TASK-03** kesilmedi. Deploy mevcut (CANLI). Yeni ajanlar kendi glob’unda; aynı cshtml yok.

## 2026-08-13 — Sales / Tester RAG (yönetici)

- Sales **yeşil**: wedge origin — her kuruşun +/− satırı ve correlation id sizin defterinizde; onlar bakiye güncellendi der. Relaunch yok.
- Tester **yeşil (ledger)**: 8 `LedgerPairTests` origin. Tam solution Coder Identity sonrası. Relaunch yok.

## 2026-08-13 — Product (Ürün)

- **OWN:** `docs/URUN.md`, `.cursor/rules/product.mdc`. `docs/AGENTS.md` / `src/` / `tests/` / TASKS / SPEC **dokunulmadı**. ORGANIZASYON.md yazılmadı (Yönetim).
- **URUN.md:** SPEC’teki 8 ekran kilitli. US-01…08 + kabul (giriş/kayıt/özet/havale/yükle-çek/hareket/dekont/admin). Path CANLI: `/giris` `/kayit` `/havale` `/yukle-cek` `/hareketler` `/admin`. Dekont menüde yok.
- **Kapsam dışı:** gerçek POS / 3DS / kart, FAST/BOA, satıcı paneli (Q2), Kafka UI. Sahte BankGateway var; sanal POS yok.
- **Architect:** 9. ekran yok. **Coder:** TASK-03 hikâyesi US-01/02/03; PageModel’de ledger yok. **Payments:** US-04/05. **Tester:** kayıt → 0,00 ₺; TASK-06 409.
- Coder TASK-03 kesilmedi. Ads/POS/Azure ürün olarak açılmadı.

## 2026-08-13 — Org (ORGANIZASYON landed)

- **OWN:** `docs/ORGANIZASYON.md`, `docs/AGENTS.md`. `src/` yok. Kardeş dosyalar yazılmadı: `URUN.md`, `DESTEK.md`, `PAZARLAMA.md`, `IK.md`, `FINANS.md`.
- DEMO şirket; gerçek banka İK değil. Harita: Yönetim=Orchestrator, Ürün=Product (yeni), Yazılım=Coder+Architect+Payments, Tasarım=Designer, Kalite=Tester, Destek=Support (yeni), Satış=Sales, Pazarlama=Marketing+SEO+PR, İK=`IK.md` (Halil CV, işe alım yok), Finans=`FINANS.md` (ledger mülakat).
- **Razor tek yazar = Coder.** Designer/SEO/PR/Marketing/Product/Support `.cshtml` yazmaz.
- `AGENTS.md` birim sütunu + yeni ajan satırları. Coder TASK-03 kesilmedi.

## 2026-08-13 — Finans (öğretme)

- **OWN:** `docs/FINANS.md`, `.cursor/rules/finans.mdc`, bu bölüm. `docs/AGENTS.md` ve `src/` **dokunulmadı**. TASKS / Razor / Domain rewrite yok.
- **FINANS.md:** çift kayıt (imzalı +/−, `PairId`, `NetOf`); neden `UPDATE Balance` yok (iz, yarış, iade, kısmi yazma); correlation id = mutabakat (PairId ≠ CorrelationId; dekont/audit/outbox/gateway); demo ≠ gerçek para; finans mülakat Q&A (mizan/GL yok, 409, timeout, freeze, kuruş).
- Kod gerçeği: `Wallet` bakiye kolonu yok; `LedgerPair.Create` / `CreateRefund` / `MoneyTransaction.RequiredInserts`. Payments `UPDATE Balance` helper eklemez.
- **Coder:** TASK-09 dekontta correlation id (yeni TASK değil). **Payments:** Domain durur. Sales wedge aynı: her kuruş +/− ve correlation defterde.
- ORGANIZASYON’daki “FINANS.md henüz yok” **eski**; dosya bu commit’te. Ads/lisans/PSP yok.

## 2026-08-13 — Product (Ürün landed)

- **OWN:** `docs/URUN.md`, `.cursor/rules/product.mdc`. `docs/AGENTS.md` / `src/` / `tests/` / TASKS / SPEC **dokunulmadı**.
- ORGANIZASYON’daki “URUN.md henüz yok” **eski** — ürün notu bu commit. SPEC ekranı eklenmedi (8 sabit).
- US-01…08 + kabul. Path: `/giris` `/kayit` `/` `/havale` `/yukle-cek` `/hareketler` `/admin`. Dekont menüde yok.
- **Kapsam dışı:** gerçek POS / 3DS / kart, FAST/BOA, satıcı paneli. Sahte BankGateway var.
- **Coder:** TASK-03 = US-01/02/03; PageModel’de ledger yok. **Payments:** US-04/05. **Tester:** 0,00 ₺; sonra 409. TASK-03 kesilmedi.

## 2026-08-13 — Pazarlama (kanallar)

- **OWN:** `docs/PAZARLAMA.md`, `.cursor/rules/pazarlama.mdc`. `docs/AGENTS.md` / `src/` / SEO.md / ADS.md / PR.md **dokunulmadı** (kopyalama yok).
- Üç kanal: **GitHub** (About + Website TASK-16’da + topics PR’de), **LinkedIn** (taslak A repo / B launch), **demo URL** (yalnızca TASK-16 tarayıcıda açık). Papara rakibi ads yok.
- Koordine: title/SC → SEO.md; Search metin → ADS.md; dürüst sıra → PR.md; pitch → SATIS/FARK. Launch sırası PR §3; Pazarlama atlamaz.
- **Kullanıcı tıklar:** GitHub Settings, LinkedIn yayın, Azure/SC/Ads. Ajan hesap/harcama yok. TASK-16 yokken “yayındayız” yok.
- **Coder:** TASK-03 kesilmedi; meta `SEO.md`. **Orchestrator:** AGENTS Pazarlama satırı (bu ajan AGENTS yazmadı). RAG: Pazarlama **yeşil** (kanal playbook); org grubundan ayır.
- ORGANIZASYON’daki “PAZARLAMA.md henüz yok” **eski**; dosya bu commit’te.

## 2026-08-13 — Tartışma (Yönetim)

- **OWN:** `docs/TARTISMA.md`, `.cursor/rules/tartisma.mdc` (`alwaysApply`). AGENTS bir paragraf işaret. Razor / `src/` yok. TASKS yok.
- **Protokol:** `src/` veya OWN değişmeden önce TARTISMA bloğu (Kim, Konu, Seçenekler, Karar, Neden, Sonra hangi dosya). HANDOFF yalnızca append status — overwrite yok.
- **Tohum:** T-001 LED ayrı repo … T-008 TASK-03 tek kapı; T-009 Identity SQLite (ledger SQL sonra).
- Coder TASK-03 kesilmedi. PR açılmadı. Ads/Azure yok.

## 2026-08-13 — T-011 cüzdan (Yönetim)

- Tartışma: `docs/TARTISMA.md` **T-011**. ClearPay = WePay benzeri **cüzdan/pay** (canvas). Sahte banka uygulaması **değil**.
- `IBankGateway` yalnızca yükle/çek stub. Coder UI: navy, Özet / Havale / Yükle-Çek — retail bank yok.
- Product/SPEC güncelleniyor. Coder TASK-03 kesilmedi (Identity + boş özet).

## 2026-08-13 — README EN/TR/FR + MIT (T-010)

- **OWN:** `README.md`, `README.tr.md`, `README.fr.md`, `LICENSE`. Razor / `src/` yok. TASK-14 Swagger ayrı (Done değil).
- GitHub varsayılan İngilizce; üstte dil linkleri. Build rozeti placeholder. Sahte screenshot yok. Papara rakibi iddiası yok.
- Durum dürüst: TASK-03 cookie Identity + boş özet; JWT/ledger SQL/409 HTTP/Azure plan.
- **Landed:** üç README eşit (ürün, 8 ekran, mermaid katman, yığın tablosu, 409/tx/outbox, Compose + `:5153`, docs indeksi). MIT zaten vardı. Push `origin/main`.

## 2026-08-13 — Coder TASK-03 kilit (T-012)

- **OWN:** `src/ClearPay.Web/**` + TARTISMA **T-012**. PageModel’de ledger/havale yok. `AddClearPay()` Program.cs’te duruyor.
- Ürün: WePay benzeri **dijital cüzdan**. Footer/sidebar: **Demo — yükleme için sahte gateway**. Asla BankaX / şube / IBAN core.
- Giriş `/giris` `/Account/Login`, kayıt `/kayit` → cookie `Musteri` → `/` bakiye **0,00 ₺**, ay giden/gelen 0, boş hareket.
- Yükle/Çek: kicker Cüzdan; bağlı hesap; Gönder/Yükle/Çek disabled.
- Tester: 41 geçti, 1 skip (409 = TASK-06). Site: http://localhost:5153
- **Sıradaki:** TASK-04 SQL model + ledger iskeleti. TASK-03 Done.

## 2026-08-13 — Designer (tarifler + motion)

- **OWN:** `docs/TASARIM.md` genişledi. cshtml **yok**. `site.css` yok.
- Ürün: WePay benzeri **cüzdan/pay**, retail bank şubesi değil. Motion: 150–250ms ease; `prefers-reduced-motion: reduce` → `transition: none`. Gölge/lift yok.
- Tarifler: giriş kartı, özet `.balance-hero`, havale `.stack-form`, hareket `.data-table` (spacing + tip ölçeği + empty).
- **Coder (`brand.css` after site.css — Razor sınıfı zaten varsa ekle):**
  1. `:root { --motion: 180ms ease; }`
  2. `.sidebar { transition: transform var(--motion); }` `.nav-backdrop { transition: opacity 180ms ease; }`
  3. `.btn, .btn-ghost { transition: background-color 160ms ease, border-color 160ms ease, color 160ms ease; }`
  4. `.stat-card, .panel, .balance-hero, .auth-card { transition: border-color 180ms ease; }` hover `border-color: #1B2A4A` (opacity 0.4 ok, box-shadow yok)
  5. `.nav-link { transition: background-color 150ms ease, color 150ms ease; }` `.field-input { transition: border-color 150ms ease; }`
  6. `@media (prefers-reduced-motion: reduce)` listed selectors `transition: none`
  7. Hareket tutar: `th.num, td.num { text-align: right; font-variant-numeric: tabular-nums; }` — tablo kolonuna `num` TASK-09’da
  8. `/hareketler` boş: `empty-title` + `empty-hint` (TASARIM tarif 4); placeholder tek `.empty` paragrafını buna çek
- Layout’ta `brand.css` zaten linkli. Identity/havale işi yok.

## 2026-08-13 — Satış / Orchestrator (T-013 para kazanma)

- **OWN:** `docs/GELIR.md` + TARTISMA **T-013**. `src/` ve `TASKS.md` dokunulmadı. SPEC ekran yok.
- Araştırma: kendi ÖK/e-para lisansı **kapalı** (TCMB 20 / 40 / 105 milyon TL, 30 Haz 2026). Açık yollar: kariyer (en hızlı nakit), white-label defter (lisans alıcıda), 6493 sınırlı ağ kapalı devre (50 milyon TL/12 ay → Ocak bildirim).
- Banka/iş yeri 15s + İstanbul gün 1–2 / işletme / Ankara-İzmir rota `GELIR.md` §4–5. Pitch Papara rakibi değil.
- **Sıradaki kod:** TASK-04. **Sıradaki ticari:** 15s ezber; “satın alın” TASK-06/11 sonrası. Avukat 6493 kullanıcı.

## 2026-08-13 — Coder: paralel Architect + en robust (T-016 / T-017)

- **OWN:** TARTISMA T-016 + T-017; `.cursor/rules/orchestrator.mdc`, `architect.mdc`, `coder.mdc`; `docs/AGENTS.md`. `src/` yok. **TASKS.md değişmedi** (sıradaki hâlâ TASK-04).
- **Coder:** Birden fazla Architect aynı anda karar üretebilir (dilimler: a SQL/şema, b ekran-akış SPEC, c port/DIP/gateway). Sen TARTISMA bitmeden Razor/şema yazma. Bitince **tek** HANDOFF satırındaki **kazanan** OWN glob’lara bak; paralel taslakları birleştirme / kaybedeni kodlama.
- SPEC 8 ekran şişmez. PageModel’de ledger yok. Portlar Application’da. `UPDATE Balance` yok.
- Orchestrator kazananı TARTISMA’da yazar (**en robust**): çift kayıt / 409 / freeze / iade=ters / outbox aynı tx; DIP; tek host + sahte gateway. Eşitlikte ledger+idempotency > kolay UI.
- HANDOFF overwrite yok. İki ajan aynı dosyayı ezmez. Coder hepsini tek seferde uygulamaz; TASK-04 kabul kriterine göre.

## 2026-08-13 — T-015 sahte APP vs sahte GATEWAY (Ürün / Orchestrator)

- **OWN:** TARTISMA **T-015**. SPEC ürün paragrafı, `URUN.md`, `MARKA.md`, `FARK.md`, `SATIS.md`, `ARCHITECTURE.md` (bir satır), README EN/TR/FR pitch. Domain ledger **yok**. Razor banka teması **yok**. TASKS yok (TASK-03 cüzdan login + boş özet durur).
- **Karar:** ürün = WePay benzeri dijital cüzdan **sitesi**. Sahte olan yalnızca `BankGateway` (REST+SOAP stub, yükle/çek timeout/retry). Şube / IBAN çekirdeği / BankaX yok. Gerçek POS/FAST/BOA durur.
- One-liner “demo cüzdan, sahte banka” düşer → **ClearPay — demo dijital cüzdan (WePay benzeri).** Footer: **Demo — yükleme için sahte gateway** (entegrasyon; ürün kimliği değil).
- **Designer:** `MARKA.md` + `designer.mdc` one-liner. Razor’u banka şubesine çevirme.
- **Sales:** wedge = cüzdan/ledger UX (409, çift kayıt, outbox). “Biz sahte bankayız” yok. `SATIS.md` / `FARK.md` / `sales.mdc`.
- **Coder:** TASK-03 = giriş/kayıt/boş özet. Yeni banka ekranı yok.

## 2026-08-13 — Coder (TASARIM tarif + T-015 footer)

- **OWN:** `src/ClearPay.Web/**` (append). Identity durur. Havale API yok.
- Footer / sidebar / auth: **Demo — yükleme için sahte gateway** (7e95ca8). “Sahte banka uygulaması” yok.
- Tarif: giriş kartı 420px, özet hero + empty-block, havale stack-form (kicker Transfer, Gönder disabled), hareket `filter-row` + 6 kolon + empty-hint, `brand.css` `--motion: 180ms ease`, `motion.css` 200ms fade, reduced-motion.
- `Program.cs`: `AddClearPayIdentity` + `AddClearPay()`. Özet `0,00 ₺` (UserManager; IWalletReader yok).
- Site: http://localhost:5153

## 2026-08-13 — Architect (T-019 Onion + n-tier eşleme)

- **OWN:** TARTISMA **T-019**; `docs/ARCHITECTURE.md`; `Program.cs` / PageModels / `EmptyWalletReader`; `tests/ClearPay.Tests/ArchitectureTests.cs` + `WalletReaderPortTests.cs`. README / docker-compose.databases / Azure **dokunulmadı**. TASK-04…16 Done değil.
- **Karar:** derleme kuralı Onion/Clean (Domain merkez, DIP içeri). n-tier = aynı projelerin adı (Web=sunum, Application=BLL, Infrastructure=DAL). Hexagonal port/adapter aynı soğan. İkinci BLL/DAL ağacı yok.
- **Kod:** `SqlOptions` `AddClearPay(configuration)` içinde. Özet/Havale/Yükle-Çek `IWalletReader` okur; PageModel ledger net yok. TASK-03 adapter `EmptyWalletReader` (0,00 ₺). `ITransferExecutor` / gateway hâlâ stub.
- **Kalan sızıntı (sonraki TASK):** Login/Register `ApplicationUser` + `UserManager` (Identity host). Havale API / SQL reader / `RestBankGateway` gerçek çağrı TASK-06/05/07. Identity SQLite ayrı (T-009).
- **Sıradaki ürün:** TASK-04 SQL model + ledger iskeleti.

## 2026-08-13 — TASK-03 Done → TASK-04 (Yönetim)

- Coder TASK-03 origin `4fa4648`. Site http://localhost:5153 `/giris`. 48 test yeşil. Rapor: Coder **yeşil**.
- `docs/TASKS.md`: TASK-03 Done; **Doing TASK-04**. Tartışma **T-021**.
- TASK-04: EF SQL Server — Wallet, LedgerEntry, Transfer, IdempotencyRecord, AuditLog, OutboxMessage. Unique UserId, unique Idempotency Key, indeks WalletId+CreatedAt.
- Identity SQLite **kalır** (T-009). Domain POCOs **rewrite yok**. Havale API **yok** (TASK-06).
- Coder (4173e7a0): Infrastructure DbContext + migration. Payments Domain durur.





## 2026-08-13 - Docker engine henuz yok (T-020 devam)

- com.docker.service Automatic + Running, client 29.7.2. Linux engine **ayaga kalkmadi**: backend log `Virtual Machine Platform not enabled`. WSL2: virtualization component reboot bekliyor (DISM 3010). Whale / WSL first-run kullanici.
- Native: SQL Server `localhost` (Windows auth) + LocalDB `MSSQLLocalDB` Running. MySQL84 `127.0.0.1:3306` (root / `.env.example`). Oracle **yok** (1521 kapali).
- Compose: `docker compose -f docker-compose.databases.yml up -d` reboot + `docker info` sonrasi. `docker-compose.yml` dokunulmadi.
- Cok-DB uygulama **sonra**. Azure yok.

## 2026-08-13 — TASK-04 landed (T-024)

- **OWN:** TARTISMA T-021/T-022/T-023/T-024. Coder `src/ClearPay.Infrastructure/Persistence/**` (`ClearPayDbContext`, Fluent, `InitialLedger`). Payments Domain ekleme: `OutboxStatus`, `Wallet.EnsureCanDebit` (LedgerPair rewrite yok). Tester model + mevcut testler.
- Kazanan: SQL Server ledger EF; Identity SQLite ayrı; 1 user=1 wallet unique; `LedgerEntry(WalletId, CreatedAt)`; `IdempotencyRecord.Key` unique; **Balance kolonu yok**; outbox tablosu şimdi (worker TASK-11). `EmptyWalletReader` durur. Havale API yok. PageModel ledger yok.
- Site: `docker compose up -d` → `dotnet run --project src/ClearPay.Web --launch-profile http` → http://localhost:5153 `/giris`. SQL yoksa Identity yine çalışır; migrate atlanır.
- **Sıradaki:** TASK-05 cüzdan özeti canlı (ledger net, `IWalletReader` SQL).

## 2026-08-13 — Coder TASK-04 EF (Infrastructure + Program DI)

- **OWN:** `ClearPayDbContext` + Fluent configs + `InitialLedger` + `AddDbContext` SQL Server. `LedgerDatabase.EnsureMigratedAsync` (SQL yoksa Identity devam). Tests: `ClearPay:ApplyLedgerMigrations=false`.
- Unique `UX_Wallet_UserId`, `UX_IdempotencyRecord_Key`, `IX_LedgerEntry_WalletId_CreatedAt`. Balance kolonu yok. `POST /api/transfers` yok.
- Site: http://localhost:5153. Sıradaki TASK-05.



## 2026-08-13 — Operasyon kimliği (T-025)

- **OWN:** `docs/SENIN-ISLERIN.md`, `docs/CANLI.md`, `docs/TASKS.md` not, `docs/TARTISMA.md` T-025. `src/` / compose / ledger / LED **dokunulmadı**.
- Kimlik: **`halilmertdeveliii@gmail.com`**. Parola/KEY sorulmadı; secret git’e yok.
- **Doğrulandı:** `gh` = `HalilMertDeveli` (keyring); `gh api user/emails` primary `halilmertdeveliii@gmail.com` verified. Remote `https://github.com/HalilMertDeveli/clearpay.git` **public**. Gmail MCP `mcp_auth` → ready; kutu aynı Gmail (GitHub mail `to:` bu adres).
- **Doğrulanmadı:** `az` bu PATH’te yok — abonelik listesi yok. Gmail’de Azure “account is ready” (2026-05-11) + eski deneme uyarı (2025-04). Abonelik **uydurulmadı**. Search Console maili yok.
- **Gmail işi:** etiket `ClearPay` (`Label_2`); 3 CI fail thread etiketlendi (`9dd576c`, `4fa4648`, `2c36974`). Papara maili yok. Ads harcama yok. LED thread’e dokunulmadı.
- **Yapılmadı:** push, Azure/SC/Ads hesabı, TASK-16 deploy, TASK-04 / D: bind ezme.
- **Sıradaki ürün:** TASK-04 (Doing). TASK-16 şimdi değil.

## 2026-08-13 — Error-fixer (T-026, CI 31701150300)

- **OWN:** `tests/ClearPay.Tests/{AuthPages,AuthOrUi,PlaceholderPages}Tests.cs` Location satırı; `.github/workflows/ci.yml`; `.cursor/skills/clearpay-error-fixer/SKILL.md`; TARTISMA **T-026**. Domain / Persistence / compose **dokunulmadı** (TASK-04 + Docker ajanları).
- CI kırmızı: 12 test `Location` `/giris` iken `/Account/Login` arıyordu. Cookie `LoginPath` zaten `/giris` (doğru). Assert hizalandı.
- SO: https://stackoverflow.com/questions/60019975 https://stackoverflow.com/questions/39206489 — docs: https://learn.microsoft.com/en-us/aspnet/core/security/authentication/cookie
- `ci.yml`: `checkout@v5` + `setup-dotnet@v5` (Node 20 deprecation; https://github.com/actions/setup-dotnet/releases/tag/v5.0.0).
- Lokal MSB3027: `ClearPay.Web` Debug kilitli — process öldürülmedi; Release test. Docker PATH yok (kullanıcı Desktop).
- **Sıradaki:** push sonrası Actions yeşil beklenir. TASK-04 ajanı devam.

## 2026-08-13 — TASK-04 Done (Orchestrator / Coder / Payments / Tester)

- **OWN:** T-024 kazanan. `ClearPayDbContext` + `InitialLedger`. Domain: `OutboxStatus`, `Wallet.EnsureCanDebit` (LedgerPair durur). `EmptyWalletReader` + stub executor. Razor/havale API yok.
- Tester: `dotnet test` yeşil (409 HTTP skip = TASK-06). Identity SQLite; ledger SQL Server. Docker PATH yok — site Identity ile açılır; SQL gelince migrate uygulanır.
- `docs/TASKS.md` TASK-04 Done. **Sıradaki:** TASK-05.

## 2026-08-13 — Push: CI `/giris` + TASK-04 EF (compose yok)

- **OWN:** T-026 test Location + `ci.yml` v5; T-024 Persistence/`InitialLedger` + Domain freeze/outbox status. `docker-compose.yml` / `docker-compose.databases.yml` / `db-smoke.ps1` **dokunulmadı**.
- Lokal Release: 50 geçti, 1 skip (409 = TASK-06). CI 31701150300 kırmızıydı (`/Account/Login` assert).
- Docker Linux engine 500 / Oracle :1521 kapalı — kullanıcı Desktop/WSL. Native SQL + MySQL diğer ajan. Azure: `infra/deploy.ps1` durur; `az login` yok.
- **Sıradaki:** origin push sonrası Actions; ürün TASK-05.

## 2026-08-13 — TASK-05 Doing (Yönetim)

- TASK-04 Done `a4755a1`. `docs/TASKS.md` Doing **TASK-05**. Tartışma **T-028**.
- Coder: `IWalletReader` → `SqlWalletReader` (`AddClearPay`). Bakiye `LedgerPair.NetOf`; ay giden/gelen; son 5; freeze rozeti. Boş = 0,00 ₺. PageModel math yok. `POST /api/transfers` yok.
- SQL down: `docker compose up -d` (sql) **veya** CanConnect → sıfır özet (500 yok). Identity SQLite durur.
- `SqlWalletReader.cs` diskte; DI hâlâ `EmptyWalletReader` — kayıt değiştir. Domain rewrite yok.


## 2026-08-13 - Deploy (T-021 MSSQL/MySQL/Oracle data on D:)

- **OWN:** TARTISMA **T-021**; `docker-compose.yml` (sql bind `D:\ClearPay\data\mssql`); `docker-compose.databases.yml` (mysql `D:\ClearPay\data\mysql`, oracle `D:\ClearPay\data\oracle`); `docs/DEPLOY.md`; `.env` gitignored (MySQL/Oracle random local secrets); `.env.example` placeholder. `src/` yok. TASK-04 migration ezilmedi. TASK-05 baslamadi.
- **Karar:** Uc motor lokal Compose. ClearPay Web ledger **yalnizca MSSQL** (:1433). MySQL :3306 + Oracle :1521 yan servis. Identity SQLite durur. C: named volume `clearpay-sql` silinmedi.
- **Disk:** D: ~940 GB bos -> `D:\ClearPay\data\...`. C: ~19 GB; AutoCAD/ss/sss/Test kullanilmadi.
- **Blok:** Docker Linux engine kalkmadi: Virtual Machine Platform. `wsl --install --no-distribution` OK; CBS reboot pending. `docker compose up` reboot sonrasi. Native MySQL84 su an 127.0.0.1:3306 (C:\ProgramData\MySQL\...); Compose 3306 icin durdur, data silme. Native MSSQLSERVER calisiyor (TCP 1433 kayitli).
- **Site:** http://localhost:5153. Azure/DNS/LED yok.
- **Siradaki:** TASK-04 ledger iskeleti (o ajan). Deploy: reboot -> `docker compose up -d` + databases compose; `docker compose ps`.

---

## 2026-08-13 — Coder UI dilleri TR/EN/DE/FR (T-027)

- **OWN:** TARTISMA **T-027**. `src/ClearPay.Web/**` (RequestLocalization, cookie `.AspNetCore.Culture` `c=tr|uic=tr`, `.resx`, `_Layout` / `_AuthLayout` dil seçici). SPEC dar: kapsam dışı «çok dilli UI» kalktı; varsayılan Türkçe; 9. ekran yok. Domain / Infrastructure Persistence **ezilmedi**. TASKS.md yeni TASK yok. TASK-05 **başlamadı** (bu ajan).
- Diller: Türkçe, English, Deutsch, Français. Chrome: sol menü + üst çubuk + giriş kartı üstü. Ads/Papara çevrilmez; demo disclaimer her dilde.
- Para: `MoneyDisplay` kültüre göre `0,00 ₺` / `0.00 ₺`; çift kayıt / 409 / ₺ kuralı durur. PageModel ledger yok.
- Tester: `dotnet build` + `LocalizationTests` (seçici dil değiştirir). Site: http://localhost:5153 `/giris` → Türkçe / English / Deutsch / Français.
- **Sıradaki ürün:** TASK-05 (ayrı ajan). Bu iş dil chrome.

## 2026-08-13 - Deploy (T-029 numarasi; T-021 carpismasi)

- TARTISMA bind-mount maddesi **T-029** (T-021 ledger EF / Identity SQLite diger ajana ait). Compose D: path ayni.
- TASK-04 o ajan bitirdi. Bu Deploy TASK-05 koduna dokunmadi; `src/` yok.

## 2026-08-13 — TASK-05 Done (Coder / Payments / Tester)

- **OWN:** T-028. `SqlWalletReader` + `AddClearPay` kaydı. Bakiye `LedgerPair.NetOf`; ay giden/gelen; son 5; freeze rozeti. `EmptyWalletReader` silindi. PageModel math yok. Havale API yok.
- SQL yoksa CanConnect → 0,00 ₺ (500 yok). İlk okumada 1 user = 1 wallet.
- Tester: `dotnet test -c Release` 62 geçti, 1 skip (409 = TASK-06). Debug kilitli (ClearPay.Web PID); process öldürülmedi.
- `docs/TASKS.md` TASK-05 Done. **Sıradaki:** TASK-06 havale + 409.

## 2026-08-13 — UI canlı animasyon (T-034)

- **OWN:** T-034. `motion.css` ambient orb/shimmer/pulse, bakiye count-up, sparkline, canlı rozet + saat. 8 ekran. Domain / SqlWalletReader yok. TASK-06 yok.
- Reduced-motion kapatır. Site: http://localhost:5153 giriş sonrası Özet.
- **Sıradaki ürün:** TASK-06.

## 2026-08-13 — README görsel + DE (T-030)

- **OWN:** T-030. `README.md` (EN varsayılan), `README.tr.md`, `README.de.md` (yeni), `README.fr.md`. SVG: `docs/assets/clearpay-layers.svg`, `docs/assets/clearpay-ledger.svg`. `src/` yok. TASK-14 Swagger **Done değil**.
- İçerik TASK-05 gerçeğine çekildi: canlı özet, TR/EN/DE/FR UI, SqlWalletReader; 409 HTTP / Azure URL iddiası yok.
- **Sıradaki ürün:** TASK-06. README push kullanıcı isterse.

## 2026-08-13 — Öğrenme (Google/Apple OAuth insan)

- **OWN:** `docs/GIRIS-SOSYAL.md` (yeni; Coder yazmamıştı), `docs/SENIN-ISLERIN.md` bölüm, OGRENME/AGENTS/README birer satır. `src/` yok.
- Kullanıcının tek işi: Google Cloud OAuth Web client + redirect `http://localhost:5153/signin-google`; secret **user-secrets** (`Authentication:Google:*`). Git yok. Apple isteğe bağlı (ücretli Developer).
- Coder buton/callback yazar; ajan Client ID üretmez.

## 2026-08-13 — Error-fixer (T-031, VS MSB3027)

- **OWN:** `launchSettings.json` + TARTISMA **T-031**. Domain / Persistence / compose yok.
- VS Error List: `MSB3027` locked by `ClearPay.Web (29448)`. `https` artık yalnız `:7133` (5153 paylaşılmaz).
- SO: https://stackoverflow.com/questions/47977927 https://stackoverflow.com/questions/55143246
- Temiz restart: stop → Debug build → `dotnet run --launch-profile http`. Site `http://localhost:5153`.
- CI latest yeşil (31702028873). 409 skip = TASK-06. Oracle/VMP kullanıcı reboot.

## 2026-08-13 — Oturum planı doküman + Notion (T-033)

- **OWN:** `docs/OTURUM-PLAN.md` (yeni). README Docs birer satır (EN/TR/FR/DE). TARTISMA **T-033**. `src/` yok. Azure hesap yok.
- İçerik: 8 ekran demo, Onion=derleme / n-tier=isim, lokal `:5153/giris`, CI yeşil `31702028873`, Azure şablon var URL yok, Redis/Rabbit compose bağlı değil, 409 = TASK-06, VMP reboot kullanıcı.
- Notion **yeni sayfa** (overwrite yok): [ClearPay — yapılan işlemler (adım adım)](https://www.notion.so/3bb31a8b18e481d3887ce44090ec42d0) (`3bb31a8b18e481d3887ce44090ec42d0`). MCP’te Share/Publish aracı yok; logged-out HTML Notion kabuğu (og:title “Notion”, içerik yok). Public kanıt = bu markdown GitHub’da.
- **Sen tıklarsın:** Notion’da Share → Publish → Publish to web; çıkan `notion.site` URL’yi README satırına koy. Sıradaki ürün: TASK-06.

## 2026-08-13 — Google/Apple Identity src (T-035)

- **OWN:** T-035. `AddClearPayExternalLogin` + `SqlWalletReader`/ledger **dokunulmadı**. `docs/GIRIS-SOSYAL.md` / `SENIN-ISLERIN.md` rewrite yok.
- NuGet: `Microsoft.AspNetCore.Authentication.Google` 8.0.21, `AspNet.Security.OAuth.Apple` 8.3.0. Callback `/signin-google` `/signin-apple`.
- Giriş/kayıt: “Google ile giriş” / “Apple ile giriş”. Secret yoksa challenge “yapılandırılmadı”. E-posta/şifre durur.
- Placeholders `appsettings.json` boş. UserSecretsId Web csproj. Secret git’te yok.
- Tester: Release 66 geçti, 1 skip (409 = TASK-06).

## 2026-08-13 — Docker engine blok (T-037)

- **OWN:** T-037. Desktop 4.86 kurulu; CLI PATH yoktu (User PATH’e `resources\bin` eklendi). Engine `docker info` takılıyor: VMP Enabled ama **CBS.RebootPending**. WSL distro yok.
- Native `MSSQLSERVER` Running; TCP/NP **Enabled=0**; 1433 kapalı. Shared memory + `-E` çalışıyor. `ClearPay` DB oluşturuldu. `appsettings.Development.json` Integrated Security `Server=localhost` (1433 Docker’a bırakıldı).
- `scripts/docker-up.ps1` reboot sonrası compose. `docker-compose.yml` ezilmedi. MySQL native `:3306` duruyor.
- **Kullanıcı:** Windows’u yeniden başlat → Docker Desktop aç → `powershell -File scripts/docker-up.ps1`. Ajan reboot etmez.
- Site şimdi native SQL ile migrate edebilir (`dotnet run --launch-profile http`).

## 2026-08-13 — Oturum planı yeni Notion (T-036)

- **OWN:** `docs/OTURUM-PLAN.md`, README Docs satırı (EN/TR/FR + DE URL hizası). TARTISMA **T-036**. `src/` yok.
- Önceki ajan fail; **yeni** sayfa: [ClearPay — oturum adımları (13 Ağu 2026)](https://www.notion.so/3bb31a8b18e4816bb34ffa405b4dec5d) (`3bb31a8b18e4816bb34ffa405b4dec5d`). Eski `3bb31a8b18e481d3887ce44090ec42d0` silinmedi.
- MCP Publish yok → sayfa varsayılan **private**. Public kanıt = GitHub `docs/OTURUM-PLAN.md`. Halil: Share → Publish → Publish to web.
- Gerçek: lokal `:5153/giris` (VS `http` profili; `https` = `:7133`), Identity SQLite, Docker giriş için şart değil; VMP özellik açık, firmware VT ON, **reboot kullanıcıda**; CI `/giris`; 409 skip TASK-06; Azure şablon var, açık URL yok.
- **Sıradaki ürün:** TASK-06.

## 2026-08-13 — Error-fixer (build/test yeşil, Compose engine kırmızı)

- **OWN:** Tester düzeltme. TASK-04 **Done yapılmadı**. LED yok. Secret git’te yok. Web MSSQL’de kaldı.
- **Kıran:** `ClearPay.Web` kilit (MSB3027); kayıt `_ExternalLoginButtons` modelsiz → `/kayit` 500; sosyal test HTML entity; test host SQL migrate/timeout; Docker Linux engine API 500.
- **Düzeltildi:** lock process durdu; partial `model="(string?)null"`; `SocialLoginTests` HtmlDecode; `ApplyLedgerMigrations=false` + Connect Timeout test factory; `SqlWalletReader` 3s; Compose D:\ bind + `${VAR:-ClearPay_Dev1!}`; `.env` gitignore. Ledger/migration ezilmedi.
- **Kanıt:** `dotnet build ClearPay.slnx -warnaserror` → 0 Warning / 0 Error. `dotnet test` → Passed **66**, Failed **0**, Skipped **1** (409 = TASK-06).
- **compose ps:** yok. Client 29.7.2; `dockerDesktopLinuxEngine` 500. VMP+WSL+Hyper-V Enabled, CBS RebootPending. Native MySQL84 `:3306` açık (reboot sonrası Compose MySQL için durdur, veriyi silme).
- **Sen tıklarsın:** Windows restart → Docker Desktop → `docker compose up -d` ve `docker compose -f docker-compose.databases.yml up -d` → `docker compose ps`.

## 2026-08-13 — Coder (T-035 Google/Apple src + T-032/T-034 canlı UI)

- **OWN:** T-035 + T-032/T-034. `docs/GIRIS-SOSYAL.md` / `SENIN-ISLERIN.md` rewrite yok. Domain ledger rewrite yok. `SqlWalletReader` yalnızca 3s CanConnect timeout (T-037 test host).
- Google/Apple: `AddClearPayExternalLogin`; NuGet Google 8.0.21 + Apple 8.3.0. Callback `/signin-google` `/signin-apple`. `/giris` `/kayit`: **Google ile giriş** / **Apple ile giriş**. Secret yoksa buton durur, POST “yapılandırılmadı”. E-posta/şifre Identity cookie durur. `appsettings.json` boş placeholder; UserSecretsId Web. Secret git’te yok.
- UI: navy+teal+ılık, kart elevation, hero/auth cam+gradient, boş cüzdan `empty-mark`, 150–250ms + ambient 6–14s, `prefers-reduced-motion`. Menü Özet / Havale / Yükle-Çek / Hareketler (Admin = TASK-10). Sosyal butonlar silinmedi (`.btn-google` / `.btn-apple`).
- Tester: `dotnet test -c Release` **68 geçti**, 1 skip (409 = TASK-06).
- **Kullanıcı:** Google Cloud OAuth + user-secrets (`docs/GIRIS-SOSYAL.md`). Apple isteğe bağlı. TASK-16 URL yok.
- **Sıradaki ürün:** TASK-06.

## 2026-08-13 — Yönetici (T-039 Alipay envanter, SPEC genişlemez)

- **OWN:** `docs/YONETICI-CALISMA.md` (yeni), TARTISMA **T-039**, `docs/YONETICI-RAPORU.md` bir satır. `src/` / `brand.css` / `_Layout` yok (T-038 Coder). README dokunulmadı.
- Katalog **57** Alipay tüketici+web özelliği: **9 Q1** (SPEC 8 analog + admin freeze), **5 Q2**, **43 never**. Papara/Alipay rakip GTM yok. 409 skip = TASK-06. Azure URL yok.
- **Sıradaki ürün:** TASK-06. İnsan listesi: «sıradaki işi yap» → havale; Docker reboot kullanıcıda; TASK-16 şimdi değil.

## 2026-08-13 — Alipay cüzdan düzeni (T-038)

- **OWN:** T-038. Designer `docs/TASARIM.md` + `docs/MARKA.md` (Alipay değiliz) + `brand.css`. Coder Razor: `_Layout`, `_AuthLayout`, `Index.cshtml`. `site.css` / `motion.css`. Domain / SqlWalletReader / compose yok. Docker reboot hikâyesi durur.
- Kopyalanan **yapı** (marka değil): Alipay ev = navy bant + büyük bakiye + 4 işlem dairesi (Gönder / Yükle / Çek / Hareketler) + örtüşen beyaz tabaka (ay özeti + son hareketler). Auth = üst navy şerit + ortalı opak kart (e-posta/şifre). Logo / QR / `#1677FF` yok. Ürün **ClearPay**. Footer **Demo — yükleme için sahte gateway**. 8 ekran.
- **Sen tıklarsın:** `http://localhost:5153/giris` → giriş → Özet. Kaynak: https://www.alipay.com/ (hızlı giriş), miniprogram ızgara, Chinability home sözlüğü.
- **Sıradaki ürün:** TASK-06.

## 2026-08-14 — Coder/Designer (T-040 UI sıkılaştırma + CSS motion)

- **OWN:** T-040. Coder `wwwroot/css/{site,brand,motion}.css`, `wwwroot/js/site.js`, `_Layout` / `_AuthLayout`, `Index.cshtml`. Designer `docs/TASARIM.md` görsel bar. SPEC görsel satırı. `resx` ezilmedi. PageModel / ledger / `SqlWalletReader` / EF yok. TASK-06 başlamadı. npm/GSAP/Framer yok.
- **Karar:** Bootstrap yok. Mevcut 8 ekran: tipografi/ritim/hiyerarşi sıkı; navy `#1B2A4A` düz zemin; **gölge/gradient yok**. T-038 yapı (bant + 4 işlem + örtüşen tabaka) durur. Dil TR/EN/DE/FR. Footer **Demo — yükleme için sahte gateway**.
- **Animasyon:** `motion.css` 150–250ms opacity/translate giriş; buton/menü/dil color-border; stat/tablo hover gölgesiz. `prefers-reduced-motion` kapatır. Sonsuz orb/shimmer/pulse yok. `site.js` count-up ~240ms + menü drawer.
- **Sen tıklarsın:** `http://localhost:5153/giris` → giriş → Özet; Havale / Yükle-Çek / Hareketler. Mobil: hamburger (dil sol menüde).
- **Sıradaki ürün:** TASK-06.

## 2026-08-14 — Coder (T-041 Redis özet cache; kasa SQL)

- **OWN:** TARTISMA **T-041**. Application `IWalletSummaryCache`; Infrastructure `CachedWalletReader` + Redis/NoOp; `AddClearPay` bağlar. PageModel / Domain / Razor yok. `UPDATE Balance` yok. Compose Redis servisi ezilmedi.
- **Karar:** Özet DTO cache (`clearpay:wallet-summary:{userId}`, TTL 60s). Kaynak `SqlWalletReader` / `LedgerPair.NetOf`. Redis yok veya düşer → SQL. SQL-down `WalletId == Guid.Empty` cache’lenmez. Identity SQLite durur.
- **Kanıt:** `GET /api/health` → `redis`: `up` / `down` / `off`. Test host Redis boş = `off`. Havale invalidate portu hazır; `POST /api/transfers` yok.
- **Ertelendi:** RabbitMQ (TASK-12), MySQL Identity, Azure yedek/canlı, TASK-06 havale/409.
- **Sen tıklarsın:** Docker Desktop → `docker compose up -d` → Redis `:6379` → site `:5153` → `/api/health` `redis=up`.
- **Sıradaki ürün:** TASK-06. TASK-12 Todo (Rabbit).

## 2026-08-14 — TASK-06 Done (Payments + Coder + Tester; T-042)

- **OWN:** TARTISMA **T-042**. `SqlTransferExecutor` + `SqlIdempotencyStore` + `IUserDirectory`. `POST /api/transfers` JWT + `Idempotency-Key` → **201 / 409**. Razor `/havale` cookie form → aynı executor. Unique `UX_IdempotencyRecord_Key` otorite. Tek SQL tx: −/+ `LedgerEntry`, Transfer, Idempotency, Audit, Outbox. `UPDATE Balance` yok. Redis `InvalidateAsync` gönderen+alıcı. Treasury yok (TASK-07). PageModel ledger yok.
- **Kanıt:** `dotnet test -c Release` **78 geçti**, 0 skip. `Duplicate_transfer_returns_409`: aynı key ikinci POST **409**, bakiye bir kez düşer.
- **Sen tıklarsın:** `http://localhost:5153/giris` → iki Musteri kayıt → (bakiye TASK-07 yükleme) → `/havale` veya `POST /api/token` + `POST /api/transfers` aynı `Idempotency-Key`. Debug `ClearPay.Web` kilitliyse Release kullan.
- **Sıradaki ürün:** TASK-07 yükle/çek + sahte REST BankGateway.

## 2026-08-14 — TASK-07 Done (Payments + Coder + Tester; T-043)

- **OWN:** TARTISMA **T-043**. `IFundingExecutor` / `SqlFundingExecutor` + `RestBankGateway`. Yükle: treasury − / müşteri +. Çek: müşteri − / treasury +. Hesap alanında `TIMEOUT` → ledger yok, `OutboxMessage` Pending. Freeze çekemez, yükleme olabilir. `UPDATE Balance` yok. Transfer satırı yok. SOAP TASK-08.
- **Kanıt:** `dotnet test -c Release` **81 geçti**. Timeout testi: 0 ledger satırı, 1 Pending outbox.
- **Sen tıklarsın:** `/yukle-cek` tutar + hesap (ör. TR00) → Özet bakiyesi artar. `TIMEOUT` yaz → ledger değişmez.
- **Sıradaki ürün:** TASK-08 SOAP aynı `IBankGateway`.

## 2026-08-14 — TASK-08 Done (Coder + Tester; T-044)

- **OWN:** TARTISMA **T-044**. `SoapBankGateway` aynı timeout/FAIL/başarı modeli. `BankGateway:Strategy=SOAP|REST` (varsayılan REST). WCF/gerçek banka yok. 8 ekran.
- **Kanıt:** `dotnet test -c Release` **83 geçti**. REST ve SOAP `TIMEOUT` → TimedOut, başarıda `REST-` / `SOAP-` referans.
- **Sıradaki ürün:** TASK-09 hareketler + dekont.

## 2026-08-14 — TASK-09 Done (Coder + Tester; T-045)

- **OWN:** TARTISMA **T-045**. `IActivityReader` / `SqlActivityReader`. `/hareketler` filtre+sayfa. `/dekont/{correlationId}` taraflar, tutar, correlation id, zaman. Yalnız kendi cüzdan. Treasury etiketi. PageModel ledger yok.
- **Kanıt:** `dotnet test -c Release` **85 geçti**. Receipt stranger → null.
- **Sen tıklarsın:** yükle/havale sonrası `/hareketler` → Dekont.
- **Sıradaki ürün:** TASK-10 Admin.

## 2026-08-14 — TASK-10 Done (Coder + Tester; T-046)

- **OWN:** TARTISMA **T-046**. `IAdminPanel` / `SqlAdminPanel`. `/admin` rol Admin. Freeze = `IsFrozen` + audit (`UPDATE Balance` yok). Failed outbox → kuyruğa al = Pending. Dev seed `admin@clearpay.test` / `Deneme123`. Production seed yok. Menü role gizli.
- **Kanıt:** `dotnet test -c Release` **88 geçti**.
- **Sen tıklarsın:** Development giriş `admin@clearpay.test` / `Deneme123` → `/admin`.
- **Sıradaki ürün:** TASK-11 Hangfire outbox.

## 2026-08-14 — TASK-11 Done (Payments + Coder + Tester; T-047)

- **OWN:** TARTISMA **T-047**. `SqlOutboxProcessor` + Hangfire recurring (minutely). Publisher TASK-11 log. Dashboard yok. Test `Hangfire:Enabled=false`. Dev MemoryStorage. Production SQL storage (`Hangfire:Enabled=true`, memory false). Timeout satırı DB’de kalır, worker Failed işaretler.
- **Kanıt:** `dotnet test -c Release` **90 geçti**. Pending → Sent; publisher fail → Failed, satır durur.
- **Sıradaki ürün:** TASK-12 Rabbit bind (Redis T-041 landed).

## 2026-08-14 — TASK-12 Done (Coder + Tester; T-048)

- **OWN:** TARTISMA **T-048**. `RabbitOutboxPublisher` queue `clearpay.outbox` when `ConnectionStrings:RabbitMq` set. Yok veya düşer → `LoggingOutboxPublisher` (Hangfire yedek). Redis T-041 durur. Ledger SQL. CloudAMQP hesabı açılmaz. `docker-compose.yml` ezilmedi.
- **Kanıt:** `dotnet test -c Release` **91 geçti**. Test host Redis+Rabbit boş → `/api/health` `redis=off` `rabbit=off`. Null connection publish throw etmez.
- **Sen tıklarsın:** Docker Desktop → `docker compose up -d` → site `:5153` → `GET /api/health` `redis=up` `rabbit=up`. Rabbit düşerse `rabbit=down`, Hangfire hâlâ outbox işler.
- **Sıradaki ürün:** TASK-13 xUnit sertleştirme (409 API, freeze, yetersiz bakiye, ledger invariant).

## 2026-08-14 — TASK-13 Done (Tester; T-049)

- **OWN:** TARTISMA **T-049**. `src/` yok. `TransferApiTests`: 409 replay, aynı key farklı tutar 409 (ikinci kesinti yok), freeze **403**, Idempotency-Key yok **400**, dual-entry toplam 0. `TransferExecutorTests` payload mismatch. `LedgerInvariantTests`: `Wallet.Balance` yok, çift kayıt, freeze debit.
- **Kanıt:** `dotnet test -c Release` **98 geçti**, 0 skip. `Duplicate_transfer_returns_409`.
- **Sıradaki ürün:** TASK-14 İngilizce README + Swagger + CV üç cümle.

## 2026-08-14 — TASK-14 Done (Coder + Tester; T-050)

- **OWN:** TARTISMA **T-050**. Swashbuckle `/swagger` + `/swagger/v1/swagger.json`. `POST /api/transfers` **409** örneği + `Idempotency-Key` header. JWT bearer. README EN/TR/DE/FR ekran tablosu güncel; PLAN 3 mülakat cümlesi. Ads yok. 9. ekran yok. Canlı URL yok.
- **Kanıt:** `dotnet test -c Release` **99 geçti**. `OpenApi_documents_transfer_409_and_idempotency_key`.
- **Sen tıklarsın:** `http://localhost:5153/swagger` → Try POST `/api/transfers` (önce `/api/token`).
- **Sıradaki ürün:** TASK-16 Azure — sen tıklarsın.

## 2026-08-14 — TASK-16 infra hazır, açık URL **blok Halil** (Deploy; T-051)

- **OWN:** TARTISMA **T-051**. Ajan Azure/DNS/hesap açmaz, `azurewebsites.net` uydurmaz. `infra/main.bicep` App Setting **`Hangfire__Enabled=true`** + `Hangfire__UseMemoryStorage=false` (eski `Hangfire__WorkerEnabled` kodda yoktu). Production Identity = Azure SQL. `docs/CANLI.md` tıklama sırası.
- **Sen tıklarsın (sırayla):**
  1. Azure CLI kur → **`az login`**
  2. Repo kökü: **`.\infra\deploy.ps1 -SqlAdminPassword (Read-Host -AsSecureString)`** (isim doluysa `-WebAppName hm-clearpay`)
  3. Portal App Settings: `ConnectionStrings__ClearPay`, `Jwt__SigningKey`, `ASPNETCORE_ENVIRONMENT=Production`, `Hangfire__Enabled=true`, `Hangfire__UseMemoryStorage=false`
  4. Publish profile → GitHub secret `AZURE_WEBAPP_PUBLISH_PROFILE` + variable `AZURE_WEBAPP_NAME`
  5. Actions **Azure deploy** (ajan `git push` etmez)
  6. Script’in yazdığı `https://<app>.azurewebsites.net/api/health` → `/giris`
- **Q2 (şart değil):** `ConnectionStrings__Redis`, CloudAMQP `ConnectionStrings__RabbitMq` — sen yapıştırırsın.
- **Sıradaki ürün:** yok (büyüme yolu ajan tarafı bitti). TASK-16 Done = sen URL’yi açınca.

## 2026-08-14 — Havale asılı kalmasın (T-052; Payments + Coder + Tester)

- **OWN:** TARTISMA **T-052**. Kök: Hangfire statik `RecurringJob` boot’ta `JobStorage.Current` yok → site düştü (`Hangfire__Enabled=false` yama). Redis Docker kapalıyken `AbortOnConnectFail=false` GET/SET/DEL havale POST’unu saniyelerce bekletiyordu. Ledger SQL; Redis/Rabbit/Hangfire düşse de transfer SQL’e karşı biter veya hızlı fail.
- **Landed:** `IRecurringJobManager` (boot kırılmaz). Redis `IsConnected` yoksa atla + 1s connect / 2s op bütçesi. SQL `CommandTimeout=8`; ulaşılamaz → `TransferResultKind.Unavailable` (sayfa mesajı / API 503). `UPDATE Balance` yok. PageModel ledger yok. Razor markup yok.
- **Kanıt:** `dotnet test -c Release` filter Transfer/cache/outbox/degrade **21 geçti**, 0 fail. `Duplicate_transfer_returns_409` durur. `MapClearPayHangfire_without_JobStorage_does_not_throw`.
- **Sen tıklarsın:** `http://localhost:5153/giris` → `/havale`. Docker Desktop engine şu an yanıt vermiyor (`:1433`/`:6379`/`:5672` kapalı); Development ledger `lpc:localhost` native SQL. Redis/Rabbit down → SQL devam. SQL de yoksa sayfada net hata, asılı kalmaz.
- **Sıradaki ürün:** TASK-16 Azure URL (blok Halil). Compose için Docker Desktop’ı sen açarsın.

## 2026-08-14 — Mobil bankacılık arayüzü (T-053; Designer + Coder + Tester)

- **OWN:** TARTISMA **T-053** (T-040 derinlik maddesi güncellendi; blok silinmedi). Designer `wwwroot/css/brand.css` + `docs/TASARIM.md` + `docs/MARKA.md`. Coder `Pages/Shared/_Layout.cshtml`, `_AuthLayout.cshtml`, `wwwroot/css/{site,motion}.css`, `wwwroot/js/site.js`, `Pages/{Index,Havale,YukleCek,Hareketler,Dekont,Admin}.cshtml`. Domain / Application / Infrastructure / migration / resx **yok**. 8 ekran, rotalar, footer **Demo — yükleme için sahte gateway** aynı.
- **Landed:** ≤800px sabit alt sekme çubuğu (`.tabbar`, mevcut `Nav*` resx anahtarları); sidebar drawer + hamburger + backdrop **kaldırıldı** (JS sadeleşti). Masaüstü içerik ortalanmış 560px; Hareketler/Admin `ViewData["Wide"]` ile 1040px. Derinlik token’ları `--elev-1..3`, bakiye kartı `--hero-grad`, tabaka `--radius-lg`. İşlem tabloları mobilde `.data-cards--mv` / `--tx` kart satırı, masaüstünde tablo; gelen tutar `.amount-in` teal. Havale/Yükle-Çek büyük tutar girişi + `.form-actions--sticky`. Yükle|Çek `:target` sekmesi (JS yok, `#yukle`/`#cek` durur). Dekont fiş düzeni. Admin tabloları `.table-scroll`. Dil seçici mobilde footer’a taşındı. Admin için ayrı `nav-ico--admin` glifi.
- **Kanıt:** `dotnet build -c Release` 0 uyarı; `dotnet test -c Release` **101 geçti**, 0 skip. Giriş sonrası `/`, `/havale`, `/yukle-cek`, `/hareketler`, `/admin` **200** ve tabbar render ediyor; `/api/health` 200 (`redis`/`rabbit` down — Docker kapalı).
- **Sen tıklarsın:** `http://localhost:5153/giris` → tarayıcı penceresini daralt (≤800px) → alt sekme çubuğu, işlem kartları, Yükle|Çek sekmesi. Geniş ekranda sol menü + ortalanmış kolon durur.
- **Sıradaki ürün:** TASK-16 Azure URL (blok Halil).

## 2026-08-14 — Giriş hero + anime.js (T-054; Coder)

- **OWN:** TARTISMA **T-054**. `_AuthLayout.cshtml` split sahne; `wwwroot/js/vendor/anime.min.js` (3.2.2 MIT, CDN runtime yok); `wwwroot/js/auth-hero.js`; `site.css` `.auth-stage` / `.auth-hero`; `motion.css` `.auth-motion`. Yapı Kredi **düzeni**; logo/mavi kimlik/fotoğraf yok. 8 ekran. Footer **Demo — yükleme için sahte gateway**. Ledger yok.
- **Landed:** Sol navy hero + sağ giriş kartı. anime.js tek sefer 180–240ms stagger. `prefers-reduced-motion` JS no-op. npm/GSAP yok.
- **Sen tıklarsın:** http://localhost:5153/giris
- **Sıradaki ürün:** TASK-16 Azure URL (blok Halil).

## 2026-08-14 — Demo kayıtlı kart (T-055; Coder + Tester)

- **OWN:** TARTISMA **T-055**. Domain `LinkedInstrument` (Last4 + Label; PAN/CVV yok). Port `ILinkedInstrumentStore` / `SqlLinkedInstrumentStore`. EF tablo `LinkedInstrument` + `UX_LinkedInstrument_UserId_Last4`. Coder `YukleCek.cshtml(.cs)` `#kart` paneli, `SharedResource` TR/EN/DE/FR, `brand.css` `.demo-card`. Funding `IFundingExecutor` + sahte gateway **değişmez**; seçim yalnız `AccountHint` (`****1234`). 9. ekran yok. PageModel ledger yok. `UPDATE Balance` yok.
- **Landed:** Yükle/Çek’te navy kart yüzü + son 4 hane formu (tam numara istenmez, en fazla 5). Chip seçince Yükle/Çek hesap alanı dolar. Migration `20260814144500_AddLinkedInstrument` SQL’e uygulandı.
- **Kanıt:** `dotnet test -c Release` **107 geçti**, 0 skip. `Add_stores_last4_without_pan`, `Add_rejects_full_pan_and_non_digits`, `YukleCek_shows_demo_card_panel_without_ninth_screen`. Site `/yukle-cek` 200: `#kart`, `.demo-card`, Kart ekle; 1234 eklenince flash + `****1234`.
- **Sen tıklarsın:** http://localhost:5153/giris (`admin@clearpay.test` / `Deneme123`) → `/yukle-cek` → son 4 hane yaz → Kart ekle → Yükle. Gerçek Visa yok.
- **Sıradaki ürün:** TASK-16 Azure URL (blok Halil).

## 2026-08-17 — GitHub taraması cüzdan cilâsı (T-056; Coder + Tester)

- **OWN:** TARTISMA **T-056**. `gh api user` = HalilMertDeveli. LED repo dokunulmadı. 8 ekran. Google/Apple durur. Ledger / `UPDATE Balance` yok.
- **İlham (kopya değil):** IdentityCourse flash + AccessDenied; TaskManagement Beni hatırla; BankApp gönderim boşluğu → busy + bakiye 0’da Gönder kapalı; dekont kopyala/yazdır (rapor/fiş).
- **Red:** LED ürünü; BankApp kasa UPDATE; profil/bildirim/9. ekran; Flutter pasta/Firebase; Darky landing + wow.js; SameSite Strict (OAuth).
- **Landed:** `/erisim-yok` empty-block; giriş `RememberMe`; havale/yükle/çek başarı → `/dekont/{id}`; `site.js` busy + clipboard + print; `@media print` chrome gizler.
- **Kanıt:** `dotnet test -c Release` **111 geçti**, 0 skip. `Access_denied_is_error_chrome_not_ninth_screen`, `Havale_send_is_disabled_when_wallet_is_empty`, girişte Beni hatırla + Google/Apple.
- **Sen tıklarsın:** http://localhost:5153/giris → Beni hatırla. Boş cüzdanda `/havale` Gönder soluk. Para hareketinden sonra fişte **Kopyala** / **Yazdır**.
- **Sıradaki ürün:** TASK-16 Azure URL (blok Halil).

## 2026-08-17 — Kamu cüzdan örnekleri (T-057; Coder + Payments + Designer + Tester)

- **OWN:** TARTISMA **T-057**. TASK-16 Todo durur. 8 ekran. Google/Apple, Beni hatırla, dekont kopyala/yazdır durur. `UPDATE Balance` yok. PageModel ledger yok. Kafka/PSP/KYC/QR/9. ekran yok.
- **İlham (kopya değil):** wepayui onay; Papara listLedgers start+end + işlem no; naira-ledger freeze; ArifCore/Digital_Wallet_System idempotency; paylite geçmiş filtre.
- **Red:** Kafka, Paystack/Flutterwave, KYC+QR+SignalR+CSV ekranı, satıcı POS, IBAN çekirdeği, 2FA sayfası, PWA, sonsuz shimmer.
- **Landed:** `/havale` incele → onay (aynı sayfa); Replay → mevcut dekont; hareketler Bitiş + corr kopyala; özet corr; dekont ****son4; Admin Çöz; empty-block CTA; `aria-busy` düz iskelet.
- **Kanıt:** `dotnet test -c Release` **114 geçti**, 0 skip. `Unfreeze_clears_flag_without_updating_balance`, `List_to_date_excludes_later_rows_and_receipt_shows_last4_hint`, `Hareketler_has_to_date_filter_and_empty_cta`. 409 API durur.
- **Sen tıklarsın:** http://localhost:5153/giris → `/havale` Gönder = onay; `/hareketler` Bitiş; admin e-posta **Çöz**.
- **Sıradaki ürün:** TASK-16 Azure URL (blok Halil).

## 2026-08-17 — Lokal Identity SQL Server (T-058; Coder)

- **OWN:** TARTISMA **T-058** (Identity SQL; kamu cüzdan T-057 ayrı). Development Identity = Windows SQL Server `ClearPay` (AspNet* + ledger aynı DB). Test factory `ClearPay:UseSqliteLedger=true` SQLite kalır. Docker Compose / `D:\ClearPay\data` ezilmedi. TASK-16 Todo.
- **Landed:** `AddClearPayIdentity` SQL unless test flag; `InitialIdentity` migration; history `__EFMigrationsHistoryIdentity`. `identity.db` artık runtime değil.
- **Kanıt:** `dotnet test -c Release` **114 geçti**, 0 skip. SQL `ClearPay`: AspNetUsers/Roles + Wallet/LedgerEntry. Admin `admin@clearpay.test` rolleri Admin+Musteri.
- **Sen tıklarsın:** siteyi yeniden başlat → http://localhost:5153/giris (`admin@clearpay.test` / `Deneme123`). Eski SQLite hesapları taşınmadı; yeniden kayıt.
- **Sıradaki ürün:** TASK-16 Azure URL (blok Halil).

## 2026-08-17 — Yol haritası (T-059)

- **OWN:** `docs/YOL.md`, TARTISMA **T-059**. `src/` yok. TASK-16 Todo durur (URL uydurulmadı; bu makinede `az` yok).
- Q1 = mülakat demosu (409/tx/outbox kanıt TASK-06/11 Done). İlk nakit = kariyer kapısı. Kendi lisans kapalı. Q2 kapalı devre/white-label **park** (avukat + 9. ekran onayı).
- İşaret: `GELIR.md`, `IK.md`, `SENIN-ISLERIN.md`, `CANLI.md`, `TASKS.md` not, README Docs.
- **Sen tıklarsın:** [Azure CLI](https://aka.ms/installazurecliwindows) → `az login` → `.\infra\deploy.ps1`. Sonra LinkedIn taslak B (`PAZARLAMA.md`) + 5–10 .NET kapısı (`IK.md`).
- **Sıradaki ürün:** TASK-16 (Halil).

## 2026-08-17 — Gelecek senaryoları (T-060; Orchestrator)

- **OWN:** TARTISMA **T-060**. Dört masa (Product, Architect, Payments, Sales/İK) tartıştı. `docs/GELECEK.md` açılmadı; katalog [`docs/YOL.md`](YOL.md) (T-059 durur). `src/` yok. SPEC 8 değişmez. TASK-16 Todo.
- **Kazanan 12 ay:** tek host + HTTPS URL + Yol **B** (mülakat maaşı). Q2 C/D ve satıcı paneli **park**. Yol A / Papara / Kafka / ikinci kart bakiyesi / 9. ekran **kapalı**.
- **Sen tıklarsın:** [`docs/YOL.md`](YOL.md) senaryo tabloları; canlı için [`CANLI.md`](CANLI.md) (`az login` + `.\infra\deploy.ps1`).
- **Sıradaki ürün:** TASK-16 Azure URL (blok Halil).

## 2026-08-17 — Flutter JWT istemci (T-061; Orchestrator + Coder)

- **OWN:** TARTISMA **T-061**. `GET /api/wallet|movements|receipts/{id}`, `POST /api/topup|withdraw` + CORS. `mobile/clearpay` Dart. Domain’e Dart yok. `ClearPay.slnx` Flutter içermez. TASK-16 Todo durur.
- **Landed:** Razor cookie + Flutter JWT → aynı Application portları → tek SQL. Pull-to-refresh = Q2.1. Hive / SignalR 9. ekran yok. Footer: Demo — sahte banka gateway. Navy `#1B2A4A`. TR varsayılan.
- **Kanıt:** `dotnet test -c Release` WalletApi (401, net 80→55, top-up 409, CORS localhost). Flutter `widget_test` giriş + demo footer.
- **Sen tıklarsın:** site `:5153` ayaktayken `cd mobile/clearpay` → `flutter run` (Android `10.0.2.2:5153`). Kayıt siteden `/kayit`. Mağaza hesabı / Azure URL yok.
- **Sıradaki ürün:** TASK-16 Azure URL (blok Halil).

## 2026-08-17 — Flutter site işlemleri (T-062; Coder)

- **OWN:** TARTISMA **T-062**. `POST /api/register`, `GET/POST /api/cards`, JWT admin. Flutter kayıt/havale/yükle/hareket/dekont/admin. Windows platform. Coder `mobile/**/*.dart`. TASK-16 Todo.
- **Landed:** Uygulama içi kayıt (cookie SignIn yok). Ajan kuralı Flutter OWN; workspace repo + `mobile/clearpay`. Hive yok.
- **Sen tıklarsın:** site `:5153` + `cd mobile/clearpay` → `flutter run -d windows` (veya emülatör). Demo `admin@clearpay.test` / `Deneme123`.
- **Sıradaki ürün:** TASK-16 Azure URL (blok Halil).

## 2026-08-17 — Flutter aynı git repo (T-063)

- **OWN:** TARTISMA **T-063**. `ClearPay.code-workspace` (ClearPay + ClearPay Flutter). İç içe `git init` yok. `ClearPay.slnx` Flutter içermez.
- **Sen tıklarsın:** `ClearPay.code-workspace` aç. Flutter klasörü aynı GitHub repo.
- **Sıradaki ürün:** TASK-16 Azure URL (blok Halil).

## 2026-08-17 — README web+mobil (T-064)

- **OWN:** TARTISMA **T-064**. Kök README 8 işlem tablosu (site + Flutter). Mobil README kök stili. `src/` yok. TASK-16 Todo.
- **Sen tıklarsın:** GitHub `README.md` üstü; `mobile/clearpay/README.md`.
- **Sıradaki ürün:** TASK-16 Azure URL (blok Halil).

## 2026-08-17 — Flutter Firebase core (T-065; Coder)

- **OWN:** TARTISMA **T-065**. `firebase_core` + `initClearPayFirebase` (eksik options JWT’yi kesmez). Auth/Firestore kasa yok. Ajan Firebase projesi açmaz. TASK-16 Todo.
- **Landed:** `lib/firebase/bootstrap.dart` + stub `firebase_options.dart`; Android `google-services` yalnızca json varsa; `tool/configure-firebase.cmd`. `flutter test` giriş+footer geçti.
- **Sen tıklarsın:** [Firebase console](https://console.firebase.google.com/) (aynı Gmail) → proje → Command Prompt: `npm install -g firebase-tools` → `firebase login` → `mobile\clearpay\tool\configure-firebase.cmd`. Windows plugin için Geliştirici Modu.
- **Sıradaki ürün:** TASK-16 Azure URL (blok Halil).

## 2026-08-17 — Flutter Android emülatör (T-061; Coder)

- **OWN:** TARTISMA **T-061** (10.0.2.2 + debug HTTP). `src/` yok. TASK-16 Todo.
- **Landed:** Android cleartext: `usesCleartextTraffic` + `network_security_config` (10.0.2.2). `flutter run -d emulator-5554` APK yüklendi, Giriş açıldı. Firebase yok → JWT durur.
- **Sen tıklarsın:** site `:5153` ayaktayken cmd: `cd /d C:\Users\clt\Projects\clearpay\mobile\clearpay` → `flutter run -d emulator-5554`. Demo `admin@clearpay.test` / `Deneme123`. AVD kapalıysa Android Studio Device Manager → Pixel 10 Pro XL Play.
- **Sıradaki ürün:** TASK-16 Azure URL (blok Halil).

## 2026-08-17 — Flutter sol çekmece (T-066; Coder)

- **OWN:** TARTISMA **T-066**. Sol `NavigationDrawer` + özet bakiye kartı / kısayol karoları. YK piksel kopyası değil (logo, altın kimlik, kredi/döviz/QR yok). 8 işlem. TASK-16 Todo durur.
- **Landed:** Giriş sonrası hamburger; çekmece ClearPay + e-posta + Özet/Havale/Yükle-Çek/Hareketler/Dekont(hareketten)/Admin(JWT)/Çıkış. Özet: navy hesap kartı, Havale/Yükle/Çek/Hareketler kısayol, son hareket → dekont. Alt sekme durur. Footer demo cümlesi.
- **Sen tıklarsın:** site `:5153` + cmd `cd /d C:\Users\clt\Projects\clearpay\mobile\clearpay` → `flutter run -d emulator-5554` (veya Windows). Giriş sonrası sol menü.
- **Sıradaki ürün:** TASK-16 Azure URL (blok Halil).

## 2026-08-17 — Web + mobil + JWT parite (T-073; Orchestrator)

- **OWN:** TARTISMA **T-073**. 8 ekran. TASK-16 Todo durur.
- **Landed:** `GET /api/transfers/{id}` (sahip/alıcı; yabancı 404); JWT 401 `ProblemDetails`; movements `page`/`pageSize` (max 50). Razor: Yükle İptal; Admin topbar `RoleAdmin`. Flutter: hareket tarih+sayfa; `flutter_secure_storage` + dosya yedek; 401 → çıkış; dondurulmuşta Havale/Yükle/Çek kapalı. Designer `.pill-admin`.
- **Sen tıklarsın:** site `:5153/yukle-cek` İptal; Admin girişte rol hapı; Flutter `flutter run`. Azure hâlâ `az login` + `.\infra\deploy.ps1`.
- **Sıradaki ürün:** TASK-16 Azure URL (blok Halil).

## 2026-08-17 — Eşzamanlı çalışma belgesi (T-070; Orchestrator)

- **OWN:** TARTISMA **T-070**. `docs/ESZAMANLI.md` (üç katman: git / masalar / makine). README + README.tr Docs satırı. `src/` yok. TASK-16 Todo.
- **Landed:** Öğretici + dürüst snapshot (TASK-01…15 Done, 409 TASK-06 Done, Azure URL yok). Paralel = ayrı OWN, tek ürün TASK. Notion kopya varsa Publish Halil.
- **Sen tıklarsın:** blob `cursor/yol-haritasi-career-first` / `docs/ESZAMANLI.md`. Notion’da Share → Publish. Docker VMP reboot hâlâ senin.
- **Sıradaki ürün:** TASK-16 Azure URL (blok Halil).

## 2026-08-17 — Mobil↔web canlı bakiye (T-071; Coder + Payments)

- **OWN:** TARTISMA **T-071**. SignalR `/hubs/wallet` chrome (9. ekran değil). Ledger commit sonra `IWalletLiveNotifier`. Payload `{ reason, correlationId }` — tutar yok. Cookie Razor + JWT query Flutter. Pull-to-refresh yedek. Firestore/Hive/`UPDATE Balance` yok. TASK-16 Todo durur.
- **Landed:** Hub + executor/admin notify. `site.js` reload. Flutter `signalr_netcore`. `docs/API-ESZAMAN.md` Halil API tıkları.
- **Sen tıklarsın:** site `:5153` girişte özet açık; Flutter aynı hesap havale. Web bakiyesi yenilenmeli. Adımlar: [`docs/API-ESZAMAN.md`](API-ESZAMAN.md).
- **Sıradaki ürün:** TASK-16 Azure URL (blok Halil).

## 2026-08-17 — Flutter TC/QR ana ızgara (T-067; Coder)

- **OWN:** TARTISMA **T-067**. Coder `mobile/**/*.dart`. TASK-16 Todo durur. 9. ekran yok.
- **Landed:** Giriş sekmeleri E-posta / TC (demo). Seed `10000000146` → `admin@clearpay.test` yalnız Flutter; arkada `POST /api/token`. Özet ızgara: Havale, Yükle, Çek, QR al, QR öde, FAST→Havale («Demo P2P — TCMB FAST değil»), Piyasalar park, Daha fazla. QR al: `qr_flutter` + `clearpay://pay?to=`. QR öde: yapıştır/yaz → Havale + `POST /api/transfers` + Guid Idempotency-Key. YK/World/Jet QR wordmark yok. SignalR liveTick korundu.
- **Sen tıklarsın:** site `:5153` + cmd `cd /d C:\Users\clt\Projects\clearpay\mobile\clearpay` → `flutter run -d emulator-5554`. Demo e-posta veya TC `10000000146` / `Deneme123`.
- **Sıradaki ürün:** TASK-16 Azure URL (blok Halil).

## 2026-08-17 — Splash + Bireysel/Kurumsal SQL (T-068; Coder)

- **OWN:** TARTISMA **T-068**. Identity `ApplicationUser.AccountKind` + JWT `account_kind`. Flutter splash → iki kart → giriş. **Firebase’e Bireysel/Kurumsal yazılmadı** (T-061 ikinci kasa yok; Firestore/Auth yok). Yerel `%LOCALAPPDATA%\ClearPay\account_kind.txt`. Kurumsal POS/9. ekran değil. Seed admin = Bireysel. `UPDATE Balance` yok.
- **Landed:** EF Identity migration `AddAccountKind`. Register/token round-trip. Rozet çekmece/app bar. WalletApiTests `account_kind` claim.
- **Sen tıklarsın:** soğuk açılış animasyonu → Bireysel veya Kurumsal → giriş. SQL Server Identity migrate (lokal).
- **Sıradaki ürün:** TASK-16 Azure URL (blok Halil).

## 2026-08-17 — Web internet-şube kromu (T-072; Coder + Designer)

- **OWN:** TARTISMA **T-072**. Coder `_Layout` / `Index` / `site.css` / `brand.css` / resx. Designer `TASARIM.md` + `MARKA.md`. TASK-16 Todo durur. 9. ekran yok. YK/Worldcard asset yok.
- **Landed:** Tam genişlik navy masthead (ClearPay + İnternet + kullanıcı/çıkış). Sol menü kâğıt zeminde, 8 işlem. Özet `dash-grid`: hesap kartı + Hızlı işlemler karoları. Masaüstü içerik 1120px. ≤800px tabbar durur. Auth T-054 aynı.
- **Kanıt:** `dotnet test ClearPay.slnx -c Release` **124 geçti**, 0 skip. AuthOrUi: masthead, Hızlı işlemler, İnternet; Worldcard yok.
- **Sen tıklarsın:** http://localhost:5153/giris → özet: üst navy şerit + sol menü + hesap kartı + hızlı karolar. Dar pencerede alt sekme durur.
- **Sıradaki ürün:** TASK-16 Azure URL (blok Halil).

## 2026-08-17 — İki platform debug (T-074; Coder)

- **OWN:** TARTISMA **T-074**. Coder Flutter + Web debug ingest. TASK-16 Todo durur. Landed yok — hipotez log’u.
- **Hipotez:** (A) API taban URL, (C) token saklama, (D) SignalR hub, (E) Windows symlink/Developer Mode, (F) `/api/token`.
- **Sen tıklarsın:** site’yi yeniden başlat (`dotnet run --project src/ClearPay.Web --launch-profile http`). Tarayıcı `:5153/giris` + Flutter `flutter run -d emulator-5554`. Windows için Geliştirici Modu (`start ms-settings:developers`) sonra `flutter run -d windows`. Demo `admin@clearpay.test` / `Deneme123`.
- **Sıradaki ürün:** TASK-16 Azure URL (blok Halil).

## 2026-08-17 — Windows firebase_core skip + iki istemci (T-075; Coder)

- **OWN:** TARTISMA **T-075**. `windows/clearpay_plugins.cmake` + `clearpay_plugin_registrant.cc`. TASK-16 Todo durur. Debug ingest durur.
- **Landed:** Developer Mode (HKLM AppModelUnlock=1). Windows `clearpay.exe` 36.9s yeşil — firebase C++ zip yok. Android `emulator-5554` APK yüklendi. Site `:5153` token 200 + hub cookie.
- **Kanıt:** `flutter run -d windows` Built clearpay.exe; `flutter run -d emulator-5554` assembleDebug; debug-021de0 `apiBase=http://localhost:5153` (windows).
- **Sen tıklarsın:** tarayıcı `:5153/giris` + açık Flutter Windows + emülatör. Demo `admin@clearpay.test` / `Deneme123`.
- **Sıradaki ürün:** TASK-16 Azure URL (blok Halil).

## 2026-08-17 — Development LocalDB (T-076; Coder)

- **OWN:** TARTISMA **T-076**. Development SQL = `(localdb)\MSSQLLocalDB` / `ClearPay` (Identity + ledger, T-058 durur). Testler SQLite. Production Azure boş. Docker/`D:\` ezilmedi. TASK-16 Todo.
- **Landed:** `appsettings.Development.json` + Identity/ledger factories LocalDB. `identity.db` Development’ta yok. Migrate: AspNet* + Wallet/LedgerEntry. Seed `admin@clearpay.test` Admin+Musteri. MDF `C:\Users\clt\ClearPay.mdf`.
- **Sen tıklarsın:** VS’de siteyi durdur/yeniden başlat (eski `lpc:localhost` kilitli). Sonra http://localhost:5153/giris → `admin@clearpay.test` / `Deneme123`.
- **Sıradaki ürün:** TASK-16 Azure URL (blok Halil).

## 2026-08-17 — PC MySQL yan motor (T-077; Deploy)

- **OWN:** TARTISMA **T-077**. Windows **MySQL84** (8.4.9) Running — yeni installer yok. `ConnectionStrings:MySql` Development yan motor; `AddClearPay` / Identity **LocalDB** (T-076) durur. Flutter mysql paketi yok. `docker-compose.databases.yml` birleşmedi (T-020). TASK-16 Todo. Firebase yok.
- **Landed:** Boş şema `ClearPay` native MySQL’de. `.env.example` MYSQL_* Compose placeholder. Mobil README: JWT → C# → SQL. Pomelo/`UseMySql` yok.
- **Sen tıklarsın:** `Get-Service MySQL84` (Automatic). Durmuşsa `net start MySQL84`. Workbench: `localhost:3306` / `root` / şifre `.env.example` MYSQL_ROOT_PASSWORD (Compose varsayılanı). Compose mysql **aynı anda açma** (:3306 native). Site hâlâ LocalDB; Flutter hâlâ JWT.
- **Sıradaki ürün:** TASK-16 Azure URL (blok Halil).

## 2026-08-17 — README mermaid ERD (T-078; Orchestrator)

- **OWN:** TARTISMA **T-078**. Kök README mermaid `erDiagram` (EN varsayılan). TR/DE/FR aynı şema. 8 ekran. Papara / 9. ekran / `Wallet.Balance` yok. TASK-16 Todo.
- **Landed:** Bölüm **Relational schema (SQL Server)**. Caption: Demo — sahte banka gateway. Lisanslı e-para değil. LocalDB `(localdb)\MSSQLLocalDB` / `ClearPay`. Flutter JWT + `firebase_core` `clearpay-c0485` (Firestore kasa yok). MySQL yan motor.
- **Sen tıklarsın:** GitHub repo ana sayfa → README → **Relational schema (SQL Server)** (mermaid render). SSMS’de `Wallet` kolonlarında Balance arama — yok.
- **Sıradaki ürün:** TASK-16 Azure URL (blok Halil).

## 2026-08-17 — Flutter Chrome web JWT (T-080; Coder)

- **OWN:** TARTISMA **T-080**. `mobile/clearpay/web/**` + `lib/platform/**` (`dart:io` web’de yok). TASK-16 Todo. Debug ingest durur.
- **Landed:** `flutter build web` yeşil. `flutter run -d chrome` açıldı. Razor `:5153` `--no-build` (VS IIS Express MSB3027 kilit). `auth-hero.js` giriş kartı opacity yedek.
- **Sen tıklarsın:** http://localhost:5153/giris (Razor). IIS Express için `localhost` kullan; `127.0.0.1` 400 verebilir. Flutter Chrome açıksa Bireysel → aynı hesap. Demo `admin@clearpay.test` / `Deneme123`. VS F5 ile ikinci `dotnet run` aynı anda DLL kilitlemesin.
- **Sıradaki ürün:** TASK-16 Azure URL (blok Halil).


