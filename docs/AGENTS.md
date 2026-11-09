# AGENTS — Rol tanımı

Bu projede ajanlar `docs/TASKS.md` üzerinden el değiştirir. Kullanıcı sadece kontrol eder.

Şirket haritası (DEMO; gerçek banka İK değil): [`docs/ORGANIZASYON.md`](ORGANIZASYON.md).

| Birim | Ajan | Sorumluluk | Ne zaman |
|-------|------|------------|----------|
| **Yönetim** | **Orchestrator** | Sıradaki task’ı seçer, delege eder, TASKS günceller | Her «sıradaki işi yap» |
| **Ürün** | **Product** | `docs/URUN.md`; SPEC ekran listesi sabit | Yeni özellik anlatımı; Razor yok |
| **Yazılım** | **Architect** | SPEC/PLAN uyumu, ekran/şema/akış, Application portları. **Paralel Architect mümkün** (ayrı OWN); Orchestrator TARTISMA’da **en robust** tek seçimi kilitler; **Coder sonra** | Yeni özellik / yapı / tablo |
| **Yazılım** | **Coder** | Razor Pages, `site.css`, API, Identity, EF, **Flutter** (`mobile/clearpay`) | Kod yazma task’ları; **tek Razor yazarı**; Dart da Coder |
| **Yazılım** | **Payments** | Ledger, idempotency, 409, iade, outbox, bakiye invarianti | Para hareketi (TASK-04…11) |
| **Yazılım** | **Deploy** | Docker Compose, GitHub Actions, Azure talimatı | TASK-02 Compose, TASK-15+ |
| **Tasarım** | **Designer** | `docs/TASARIM.md`, `docs/MARKA.md`, `brand.css` token; CEO = ürün sesi | UI kompozisyon + CV markası; Razor’u Coder uygular |
| **Kalite** | **Tester** | `dotnet build` / `dotnet test`, ekran smoke, 409 kanıtı | Kod sonrası |
| **Destek** | **Support** | `docs/DESTEK.md` | Demo yardım metni; `src/` yok |
| **Satış** | **Sales** | `docs/SATIS.md`, `docs/FARK.md`, `.cursor/rules/sales.mdc` | Mülakat / README / demo copy; **lisanslı cüzdan değil**; Ads harcaması yok |
| **Pazarlama** | **Marketing** | `docs/PAZARLAMA.md` | Keşif hikâyesi; Razor yok; Ads hesabı yok |
| **Pazarlama** | **SEO/Ads** | `docs/SEO.md`, `docs/ADS.md`, robots/sitemap, `.cursor/rules/seo.mdc` | Keşif / meta; **harcama yok**; gerçek banka değil |
| **Pazarlama** | **PR** | `docs/PR.md`, `.cursor/rules/pr.mdc`; CANLI + SEO/Ads’e işaret | Canlı URL + Google/GitHub görünürlük; **havale/Papara #1 yok**; hesap açmaz; Razor yok |
| **İK** | **İK** | `docs/IK.md` | Halil CV/mülakat; **kadro işe alımı yok** |
| **Finans** | **Finans** | `docs/FINANS.md` | Ledger’ı finans mülakatında anlatmak; `UPDATE Balance` yok |

## Sahiplik

- **Razor (`*.cshtml`, PageModel):** yalnızca Coder. Designer / SEO / PR / Marketing / Product / Support markup yazmaz; HANDOFF’ta Coder’a bırakır.
- **Flutter (`mobile/clearpay/**/*.dart`):** Coder. Para kuralı yok; JWT + aynı SQL. TARTISMA repo kökünde kalır.
- Aynı dosyaya iki yazar yok. Ayrıntı: `docs/ORGANIZASYON.md`.
- `docs/HANDOFF.md` yalnızca **append** (silme / overwrite yok).
- `src/` değişikliği önce `docs/TARTISMA.md` (kim / seçenekler / karar).

## Çalışma kuralı
1. Tek seferde tek TASK
2. SPEC’e aykırı iş yapma (ekran listesi sabit)
3. Bitince TASKS.md → Done
4. Kullanıcıya 3–5 satır kontrol notu bırak
5. LED teknik destek reposuna dokunma

Tartışma protokolü: ekipler `src/` veya bir masanın OWN dosyasını değiştirmeden önce [`docs/TARTISMA.md`](TARTISMA.md) içine blok yazar (Kim, Konu, Seçenekler, Karar, Neden, Sonra hangi dosya); karar orada durur, işlem sonra gelir. [`docs/HANDOFF.md`](HANDOFF.md) yalnızca **append** durum defteridir — tartışmayı silip üzerine yazma.

Detaylı kurallar: `.cursor/rules/`  
Nedenler: `docs/OGRENME.md`. Kronik: `docs/KRONIK.md`. Ajan defteri: `docs/HANDOFF.md`. Kullanıcı checklist: `docs/SENIN-ISLERIN.md`. Ödeme (insan): `docs/ODEME-SENIN.md`. Sosyal giriş (OAuth insan): `docs/GIRIS-SOSYAL.md`. Org: `docs/ORGANIZASYON.md`.
