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
