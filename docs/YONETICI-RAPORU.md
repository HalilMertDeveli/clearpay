# Yönetici raporu — 2026-08-13

Kaynak: `docs/CALISMA-PLANI.md`, `docs/HANDOFF.md`, `docs/TASKS.md`, `docs/FARK.md`, origin/main.

**Kritik yol:** TASK-03 — Coder Identity + UI. Havale API (TASK-06) **yok**. Azure hesabı **açılmadı**.

**Tartışma:** `docs/TARTISMA.md`. `src/` maddesiz değişmez. T-009: Identity SQLite. **T-011:** cüzdan/pay (WePay hissi), sahte banka uygulaması değil; gateway = yükle/çek stub.

**Sales wedge (FARK, origin):** *Her kuruşun +/− satırı ve correlation id sizin defterinizde; onlar “bakiye güncellendi” der.* Papara tüketici yerine geçmiyoruz.

**Architect kapısı:** Application portları (`IBankGateway`, `IWalletReader`, `ITransferExecutor`, DIP) yapı kapısıdır. Coder PageModel’e ledger/havale koymaz. Payments Domain durur.

Lokal: http://localhost:5153 — TASK-03 commit + `dotnet run --project src/ClearPay.Web --launch-profile http`.

---

## RAG

| İz | Renk | Gerçek |
|----|------|--------|
| **Coder TASK-03** | **Sarı** | Login/kayıt/özet diskte; origin’de henüz yok. Kritik yol. |
| **Payments Domain** | **Yeşil** | `6e809f7` + `LedgerPair.NetOf` `e2a5320`. `UPDATE Balance` yok. Rewrite yok. |
| **Architect / SOLID** | **Sarı (kapı)** | ARCHITECTURE `62bbddd`. Portlar Application’da ekleniyor. TASK-06 öncesi kapı. |
| **Öğrenme** | **Yeşil** | `OGRENME.md` + `SENIN-ISLERIN.md` (`a4f9400`, `739c801`). |
| **Deploy / CANLI** | **Yeşil (plan)** | `CANLI.md` `9ca5676`. Q1 `https://clearpay.azurewebsites.net` West Europe. TASK-16 yok. Compose SQL-only. |
| **SEO/Ads** | **Yeşil** | `SEO.md` + `ADS.md` landed. Ads yalnızca canlı URL sonrası. SC/GA4/Ads **kullanıcı** açar. Harcama yok. |
| **Sales** | **Yeşil** | Wedge origin FARK/SATIS. Papara yerine geçmiyoruz. Relaunch yok. |
| **Designer** | **Yeşil** | TASARIM/MARKA/`brand.css` `666dd32`. Relaunch yok. |
| **Tester** | **Yeşil (ledger)** | 8/8 `LedgerPairTests` origin. Tam solution Coder Identity derlenene kadar kırmızı olabilir. Relaunch yok. |
| **PR** | **Yeşil** | `docs/PR.md` landed. Demo kelimeler; Papara/havale #1 yok. |
| **Pazarlama** | **Yeşil** | `PAZARLAMA.md` + `pazarlama.mdc`. GitHub / LinkedIn / demo URL. SEO/ADS/PR kopyalanmadı. Papara rakibi ads yok. |
| **Ürün / Destek / İK / Finans** | **Sarı / kardeş** | Org haritası: `docs/ORGANIZASYON.md`. Coder’a iş yok. |
| **Çalışma planı** | **Yeşil** | `docs/CALISMA-PLANI.md` yazıldı. |

---

## Rol tablosu

| Rol | Landed | Durum | Sıradaki |
|-----|--------|-------|----------|
| **Coder** | Disk: Account, Identity cookie, özet `0,00 ₺`. Origin: iskelet. | **WIP / kritik** | **Yalnızca TASK-03.** Org ajanları kesmez. Ledger/havale PageModel’de yok. |
| **Payments** | Origin Domain Ledger + `NetOf`. | **Done / yeşil** | Relaunch yok. EF login sonrası. Havale TASK-06. |
| **Architect** | `ARCHITECTURE.md`. Portlar Application (DIP). | **Kapı** | Canvas + SOLID. SPEC değişmeden ekran ekleme. |
| **Tester** | Origin 8 `LedgerPairTests`. | **Done (unit) / yeşil** | Relaunch yok. Tam test Coder Identity sonrası. |
| **Deploy** | CANLI + SQL Compose. | **Plan yeşil** | TASK-16 yok. |
| **SEO/Ads** | SEO + ADS. | **Yeşil** | Kampanya canlı URL sonrası. Razor çalma. |
| **Designer** | Origin `666dd32`: TASARIM, MARKA, `brand.css`. | **Done / yeşil** | Relaunch yok. Coder `brand.css` bağlar. |
| **Sales** | SATIS + FARK. | **Yeşil** | Wedge sabit. PSP değil. |
| **Pazarlama** | `PAZARLAMA.md` (GitHub, LinkedIn, demo URL). | **Yeşil** | SEO/ADS/PR kopyalama yok. Launch TASK-16 sonrası. |
| **Öğrenme** | Origin. | **Yeşil** | CALISMA-PLANI link. |
| **Orchestrator** | Bu rapor + plan + HANDOFF. | — | Tek öncelik TASK-03. |

---

## Karar

1. Coder **yalnızca TASK-03** — org ajanları kesmez, ekstra iş yok.
2. Sales / Tester / Designer relaunch yok.
3. Org: `docs/ORGANIZASYON.md` land edince HANDOFF işaret eder.
4. TASK-06 / Azure / Ads harcama **şimdi değil**.
