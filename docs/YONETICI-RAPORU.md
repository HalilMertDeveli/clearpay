# Yönetici raporu — 2026-08-13

Kaynak: `docs/CALISMA-PLANI.md`, `docs/HANDOFF.md`, `docs/TASKS.md`, `docs/FARK.md`, origin/main.

**Kritik yol:** TASK-03 — Coder Identity + UI. Havale API (TASK-06) **yok**. Azure hesabı **açılmadı**.

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
| **Sales** | **Yeşil (docs)** | `SATIS.md` + `FARK.md`. Tek wedge yukarıda. |
| **Designer** | **Yeşil** | `TASARIM.md` + `MARKA.md` + `brand.css` origin `666dd32`. Relaunch yok. Coder yalnızca `brand.css` linkler. |
| **Tester** | **Sarı** | Auth/wallet testleri untracked. `LedgerPair.NetOf` unit serbest (EF yok). |
| **PR** | **Sarı** | `docs/PR.md` yazılıyor: canlıya çıkış + dürüst rank. Demo kelimeler; Papara/havale #1 yok. |
| **Çalışma planı** | **Yeşil** | `docs/CALISMA-PLANI.md` yazıldı. |

---

## Rol tablosu

| Rol | Landed | Durum | Sıradaki |
|-----|--------|-------|----------|
| **Coder** | Disk: Account, Identity cookie, özet `0,00 ₺`. Origin: iskelet. | **WIP / kritik** | TASK-03 bitir + commit. `_Layout`’a `brand.css` link + meta/canonical (SEO; yeni TASK değil). **Ledger/transfer PageModel’de yok.** Domain ezme. |
| **Payments** | Origin Domain Ledger + `NetOf`. | **Done / yeşil** | Relaunch yok. EF login sonrası. Havale TASK-06. |
| **Architect** | `ARCHITECTURE.md`. Portlar Application (DIP). | **Kapı** | Canvas + SOLID. SPEC değişmeden ekran ekleme. |
| **Tester** | Disk test WIP. | **Sarı** | Coder sonrası `dotnet test`. `LedgerPair` unit OK. Web yok. |
| **Deploy** | CANLI + SQL Compose. | **Plan yeşil** | TASK-16 yok. |
| **SEO/Ads** | SEO + ADS. | **Yeşil** | Kampanya canlı URL sonrası. Razor çalma. |
| **Designer** | Origin `666dd32`: TASARIM, MARKA, `brand.css`. | **Done / yeşil** | Relaunch yok. Coder `brand.css` bağlar. |
| **Sales** | SATIS + FARK. | **Yeşil** | Wedge sabit. PSP değil. |
| **Öğrenme** | Origin. | **Yeşil** | CALISMA-PLANI link. |
| **Orchestrator** | Bu rapor + plan + HANDOFF. | — | Tek öncelik TASK-03. |

---

## Karar

1. Coder TASK-03 → origin (Identity + boş özet + UI). Port/ledger işi PageModel’de değil.
2. Architect Application portları = para özelliği kapısı (TASK-05/06 öncesi).
3. Tester: Coder sonrası smoke; `NetOf` unit şimdi olabilir.
4. TASK-06 / Azure / Ads harcaması **şimdi değil**.
