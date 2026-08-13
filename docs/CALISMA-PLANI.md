# Çalışma planı

Bu belge **tüm ajanların** izlediği sıra ve test kapısıdır. Ürün kuralları `docs/SPEC.md`, kabul maddeleri `docs/PLAN.md`, kuyruk `docs/TASKS.md`. Ajan defteri: `docs/HANDOFF.md`. Pazar farkı (mülakat, gerçek rakip değil): `docs/FARK.md`.

**Amaç:** Demo dijital cüzdan — girişten ledger’a, 409’dan Azure URL’ye. Lisanslı banka / PSP **değil**.

**Tek ürün TASK:** Coder aynı anda bir TASK (şimdi **TASK-03**). Diğerleri çakışmayan dosyada paralel çalışabilir.

**Dosya sahipliği:** İki yazar aynı `.cshtml`’e girmez. `_Layout` aynı anda Coder + Designer yazılmaz (Designer token; Coder uygular).

**Tartışma:** `src/` öncesi `docs/TARTISMA.md` (kim / seçenekler / karar). HANDOFF status.

---

## Test omurgası (her fazda)

Test yalnızca TASK-13 değildir. Her fazın kapısı vardır. `dotnet test` kırmızıysa TASK **Done olmaz**.

| Faz / TASK | Test |
|------------|------|
| TASK-02 | Placeholder / menü / `/api/health` **200** smoke (var) |
| TASK-03 | Kayıt → giriş → özet `0,00 ₺`; validasyon fail (boş/şifre uyuşmaz); korumalı rota → login |
| TASK-04 | Ledger invariant unit: çift `+/−` toplamı 0; bakiye = satır net’i; `UPDATE Balance` yok |
| TASK-05 | Özet canlı: bakiye / ay giden-gelen / son 5 (smoke veya unit) |
| TASK-06 | Aynı `Idempotency-Key` → **409**, ikinci kesinti yok; yetersiz bakiye; freeze |
| TASK-07 / 08 | Gateway timeout → ledger **kesinleşmez**; outbox/kuyruk kaydı kalır |
| TASK-09+ | Dekont `correlation id`; admin freeze (ileride) |
| **Her PR** | `dotnet test` kırmızıysa Done yok; Actions (TASK-15) aynı kapı |

---

## Faz tablosu

| Faz | TASK | Durum | Kim | Ne |
|-----|------|-------|-----|-----|
| **0** Docs | TASK-01 | **Done** | Orchestrator | SPEC, PLAN, ajanlar |
| **1** Görünen site | TASK-02 | **Done** | Coder / Deploy | Solution, sol menü, SQL Compose |
| **1** | TASK-03 | **Done** (`4fa4648`) | **Coder** | Giriş + kayıt + boş özet. 48 test. |
| **2** Para motoru | TASK-04 | **Done** (`a4755a1`) | Coder Infrastructure | EF SQL Server. |
| **2** | **TASK-05** | **Doing** | Coder | `SqlWalletReader` + NetOf; ay in/out; son 5. Havale API yok. |
| **2** | TASK-06 | Todo | Payments + Coder | Havale + 409. **Şimdi başlama.** |
| **3** Banka + geçmiş | TASK-07 | Todo | Coder + Payments | Yükle/çek + sahte REST gateway |
| **3** | TASK-08 | Todo | Coder | SOAP, aynı sözleşme |
| **3** | TASK-09 | Todo | Coder | Hareketler, filtre, dekont |
| **4** Ops | TASK-10 | Todo | Coder | Admin: dondur, kuyruk, audit |
| **4** | TASK-11 | Todo | Payments + Coder | Outbox + Hangfire |
| **4** | TASK-12 | Todo | Coder / Deploy | Redis + Rabbit lokal Compose |
| **5** Kanıt + canlı | TASK-13 | Todo | Tester | Sertleştirme (ledger, 409, API) |
| **5** | TASK-14 | Todo | Coder | İngilizce README, Swagger, CV |
| **5** | TASK-15 | Todo | Deploy | GitHub Actions (`dotnet test`) |
| **5** | TASK-16 | Todo | Deploy + **kullanıcı** | Azure. Plan: `docs/CANLI.md`. **Şimdi hesap açma.** |

**SEO/Ads:** canlı URL sonrası (`https://clearpay.azurewebsites.net` planı). Her metinde demo disclaimer. Google Ads **harcaması yok**.

**Designer:** `TASARIM.md` / `MARKA.md` / isteğe `brand.css`. Gerçek banka markası değil.

**Sales:** `SATIS.md` + `FARK.md` — mülakat pitch; Papara/iyzico **gerçek rakip değil**.

**Senin işler:** `docs/SENIN-ISLERIN.md` — Azure aboneliği, isteğe bağlı domain, sırlar. Ajan hesap açmaz.

Lokal site: http://localhost:5153 — `dotnet run --project src/ClearPay.Web --launch-profile http`. Compose: SQL-only.

---

## Ajan sahipliği

| Ajan | Owns | Not |
|------|------|-----|
| **Orchestrator** | `docs/HANDOFF.md`, `docs/TASKS.md`, bu plan, `docs/YONETICI-RAPORU.md` | Tek ürün TASK; spawn kaydı HANDOFF’ta |
| **Coder** | `src/ClearPay.Web/**` (+ Identity Application/Infrastructure) | **Öncelik TASK-03.** Domain/Ledger ezme. Havale API yok. |
| **Payments** | `src/ClearPay.Domain/Ledger/**` | **Done** (`6e809f7` + `NetOf` `e2a5320`). Relaunch yok. |
| **Tester** | `tests/**` yeni dosyalar | TASK-03 sonrası smoke; Domain `LedgerPair` unit serbest |
| **Deploy** | compose, `.dockerignore`, `docs/DEPLOY.md`, `docs/CANLI.md` | **CANLI landed.** TASK-16 yok. |
| **SEO/Ads** | `docs/SEO.md`, `docs/ADS.md`, robots/sitemap, `seo.mdc` | Harcama yok. Razor çalma. |
| **Designer** | `docs/TASARIM.md`, `docs/MARKA.md`, isteğe `brand.css` | `_Layout`’u Coder ile aynı anda yazma |
| **Sales** | `docs/SATIS.md`, `docs/FARK.md` | Pazar araştırması = mühendislik farkı; PSP değil |
| **Architect** | `docs/ARCHITECTURE.md` + Application portları (`IBankGateway`, `IWalletReader`, `ITransferExecutor`) | **Kapı:** SOLID/DIP. PageModel’de ledger yok. Domain durur. |
| **Öğrenme** | `docs/OGRENME.md`, `docs/SENIN-ISLERIN.md` | **Done** origin. |

---

## Yasak (ajan)

- TASK-06 havale API’si TASK-03 bitmeden
- TASK-16 Azure hesabı / DNS açmak
- Google Ads harcaması
- Ledger’ı `UPDATE Balance` ile “düzeltmek”
- LED teknik destek reposuna yazmak
- ClearPay’i gerçek banka / Papara alternatifi diye satmak
