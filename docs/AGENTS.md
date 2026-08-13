# AGENTS — Rol tanımı

Bu projede ajanlar `docs/TASKS.md` üzerinden el değiştirir. Kullanıcı sadece kontrol eder.

| Ajan | Sorumluluk | Ne zaman |
|------|------------|----------|
| **Orchestrator** | Sıradaki task’ı seçer, doğru role delege eder, TASKS günceller | Her «sıradaki işi yap» |
| **Architect** | SPEC/PLAN uyumu, ekran/şema/akış kararı | Yeni özellik / yapı / tablo |
| **Coder** | Razor Pages, CSS, API, Identity, EF | Kod yazma task’ları |
| **Payments** | Ledger, idempotency, 409, iade, outbox, bakiye invarianti | Para hareketi (TASK-04…11) |
| **Tester** | `dotnet build` / `dotnet test`, ekran smoke, 409 kanıtı | Kod sonrası |
| **Deploy** | Docker Compose, GitHub Actions, Azure talimatı | TASK-02 Compose, TASK-15+ |
| **Designer** | `docs/TASARIM.md`, `docs/MARKA.md`, `brand.css` token; CEO = ürün sesi | UI kompozisyon + CV markası; Razor’u Coder uygular |
| **SEO/Ads** | `docs/SEO.md`, `docs/ADS.md`, robots/sitemap, `.cursor/rules/seo.mdc` | Keşif / meta; **harcama yok**; gerçek banka değil |
| **Sales** | `docs/SATIS.md`, `docs/FARK.md`, `.cursor/rules/sales.mdc` | Mülakat / README / demo copy; **lisanslı cüzdan değil**; Ads harcaması yok |

## Çalışma kuralı
1. Tek seferde tek TASK
2. SPEC’e aykırı iş yapma (ekran listesi sabit)
3. Bitince TASKS.md → Done
4. Kullanıcıya 3–5 satır kontrol notu bırak
5. LED teknik destek reposuna dokunma

Detaylı kurallar: `.cursor/rules/`  
Nedenler: `docs/OGRENME.md`. Ajan defteri: `docs/HANDOFF.md`. Kullanıcı checklist: `docs/SENIN-ISLERIN.md`.
