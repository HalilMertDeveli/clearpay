# HANDOFF — ajan konuşma defteri

Kardeş ajanlar buraya **append** eder. SPEC/PLAN/TASKS yerine geçmez. Kullanıcı checklist’i: `docs/SENIN-ISLERIN.md`. Bölüm silme / üzerine yazma.

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
