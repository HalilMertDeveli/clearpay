# ORGANIZASYON — ClearPay DEMO şirket

**Bu gerçek bir banka veya ödeme kuruluşu İK organigramı değildir.** İşe alım, bordro, şube kadrosu, BDDK lisansı yok. Ajan rolleri, mülakat/portföy demosu için sahte şirket birimlerine eşlenir.

Ürün: ASP.NET Core 8 cüzdan **demosu** — sahte banka gateway. Kaynak: [`SPEC.md`](SPEC.md). Ajan kartı: [`AGENTS.md`](AGENTS.md). Defter: [`HANDOFF.md`](HANDOFF.md).

## Harita

| Birim | Ajan | Durum | Ana dosya |
|-------|------|-------|-----------|
| **Yönetim** | Orchestrator | var | `docs/TASKS.md`, `docs/YONETICI-RAPORU.md` |
| **Ürün** | Product | **yeni** | `docs/URUN.md` (kardeş yazar) |
| **Yazılım** | Coder + Architect + Payments | var | `src/**` (aşağıdaki OWN) |
| **Tasarım** | Designer | var | `docs/TASARIM.md`, `docs/MARKA.md`, `brand.css` |
| **Kalite** | Tester | var | `tests/**` |
| **Destek** | Support | **yeni** | `docs/DESTEK.md` (kardeş yazar) |
| **Satış** | Sales | var | `docs/SATIS.md`, `docs/FARK.md` |
| **Pazarlama** | Marketing + SEO + PR | Marketing **yeni**; SEO ve PR var | `docs/PAZARLAMA.md` (kardeş) + `SEO.md` / `ADS.md` / `PR.md` |
| **İK** | İK (docs) | **yeni** | `docs/IK.md` — Halil CV/mülakat; kadro işe alımı yok |
| **Finans** | Finans (docs) | **yeni** | `docs/FINANS.md` — ledger’ı finans mülakatında anlatmak |

Yazılım altında ops (şirket birimi değil, mevcut ajan): **Deploy** — Compose, Actions, Azure talimatı (`docs/DEPLOY.md`, `docs/CANLI.md`).

```
                    Yönetim (Orchestrator)
                              |
        +----------+----------+----------+----------+
        |          |          |          |          |
      Ürün      Yazılım    Tasarım    Kalite     Destek
   (Product)   Coder       Designer   Tester    Support
               Architect
               Payments
               [Deploy]
        |          |          |
      Satış    Pazarlama     İK / Finans
      Sales    Marketing      (docs only)
               SEO + PR
```

## Kim var / kim yeni

**Var (kod veya docs landed / ajan kuralı duruyor):** Orchestrator, Architect, Coder, Payments, Tester, Deploy, Designer, SEO/Ads, Sales, PR.

**Yeni (bu org; içerik kardeş commit’te gelebilir):** Product (`URUN.md`), Support (`DESTEK.md`), Marketing (`PAZARLAMA.md`), İK (`IK.md`), Finans (`FINANS.md`).

İK personel tutmaz. Finans muhasebe programı açmaz. İkisi de Halil’in mülakat dosyasıdır.

## Dosya sahipliği (OWN)

Aynı dosyayı iki ajan yazmaz. Kardeş `HANDOFF`’a satır bırakır; üzerine yazmaz.

| Glob / dosya | Tek yazar | Yazmaz |
|--------------|-----------|--------|
| `docs/TASKS.md`, `docs/YONETICI-RAPORU.md` | Orchestrator | |
| `docs/SPEC.md`, `docs/PLAN.md`, `docs/ARCHITECTURE.md` | Architect | Product ekran uydurmaz; SPEC listesi sabit |
| `docs/URUN.md` | Product | SPEC’i ezmez; Razor yok |
| `src/ClearPay.Web/**/*.cshtml`, PageModel | **Coder** | Designer, SEO, PR, Marketing, Product, Support |
| `src/ClearPay.Web/wwwroot/css/site.css` | Coder | Designer `site.css` ezmez |
| `src/ClearPay.Web/wwwroot/css/brand.css` | Designer (ek token) | Coder token savaşmaz; layout **linkini** Coder atar |
| `src/ClearPay.Application/**` (portlar) | Architect | Coder PageModel’de ledger yazmaz |
| `src/ClearPay.Domain/Ledger/**` | Payments | Web/Razor yok; `UPDATE Balance` yok |
| `src/ClearPay.Infrastructure/**` (EF, gateway) | Coder veya Architect (HANDOFF’ta kim) | Payments Domain’de kalır |
| `tests/**` | Tester | `src/` yok |
| `docs/TASARIM.md`, `docs/MARKA.md` | Designer | |
| `docs/DESTEK.md` | Support | `src/` yok |
| `docs/SATIS.md`, `docs/FARK.md` | Sales | Ads harcaması yok |
| `docs/PAZARLAMA.md` | Marketing | Razor yok; Ads hesabı yok |
| `docs/SEO.md`, `docs/ADS.md`, `robots.txt`, `sitemap.xml` | SEO/Ads | `_Layout` meta’sını Coder uygular |
| `docs/PR.md` | PR | Papara/havale #1 yok; hesap açmaz |
| `docs/IK.md` | İK | iş ilanı / LinkedIn işe alım yok |
| `docs/FINANS.md` | Finans | ledger’ı Payments koduyla çeliştirmez |
| `docs/DEPLOY.md`, `docs/CANLI.md`, `docker-compose.yml` | Deploy | Azure hesabı açmaz |
| `docs/HANDOFF.md` | herkes **append** | bölüm silme / overwrite yok |
| `.cursor/rules/*.mdc` | o rolün ajanı | başkasının kuralını ezme |

`docs/AGENTS.md` + bu dosya: org ajanı. Diğer ajanlar tablo satırını HANDOFF ile ister; org belgesini yeniden yazmaz.

## Razor: tek yazar

**İki ajan aynı Razor dosyasını yazmaz.** `.cshtml` ve PageModel yalnızca **Coder**.

- Designer kompozisyon ve `brand.css` token verir; sınıf listesini HANDOFF’ta Coder’a bırakır.
- SEO/PR/Marketing title, meta, footer cümlesi verir; tag’i Coder `_Layout` / `_AuthLayout` içine koyar.
- Product / Support / Sales / İK / Finans ekran veya markup yazmaz.
- Architect port ve şema; Payments ledger; Tester test — Razor yok.

Çakışma: Coder WIP varken başka ajan `src/ClearPay.Web` açmaz. İhtiyaç HANDOFF satırı.

## İK ve Finans (ne değil)

- **İK:** `docs/IK.md` — Halil’in CV’si, mülakat soruları, “neden bu repo”. Çalışan aranmaz, maaş yazılmaz, banka HR süreci taklit edilmez.
- **Finans:** `docs/FINANS.md` — çift kayıt, 409, outbox’ı finans / iç kontrol mülakatında anlatmak. Gerçek muhasebe, fatura, vergi dairesi yok. Para kuralı kaynağı hâlâ [`SPEC.md`](SPEC.md) + Payments.

## El değiştirme

1. Tek seferde tek TASK (`docs/TASKS.md`). Org docs TASK kuyruğunu atlamaz.
2. Bitince `HANDOFF.md` **append**. Kardeş bölümünü silme.
3. SPEC ekran listesi sabit. Yeni sayfa = Architect + kullanıcı; org birimi “ekran uydurayım” demez.
4. LED teknik destek reposuna dokunulmaz.
5. Hosting / DNS / Ads hesabı kullanıcıya kalır.
