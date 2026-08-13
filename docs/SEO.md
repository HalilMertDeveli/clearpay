# SEO — ClearPay (demo)

ClearPay lisanslı ödeme kuruluşu **değildir**. Portföy / mülakat demosu. Her genel metin: **Demo — sahte banka gateway.**

Ses: `docs/MARKA.md` (Designer). Dosya yoksa veya çelişirse kilit cümle footer ile aynı kalır. `docs/TASARIM.md` ve CSS’e dokunulmaz.

Ads yokken bile bu belge geçerlidir. Kampanya: `docs/ADS.md` — **yalnızca canlı URL sonrası** (`docs/CANLI.md`, TASK-16).

Placeholder host: `https://clearpay.azurewebsites.net` — App Service adı doluysa `docs/CANLI.md` sırası (`clearpay-wallet`, `hm-clearpay`) veya özel domain. Sitemap/robots’taki host o zaman güncellenir.

## Title / description şablonları

`{Sayfa}` kısa; title 50–60 karakter. Description 140–160. Her description’da **Demo** geçer.

| Sayfa | Title | Meta description |
|-------|--------|------------------|
| Giriş | Giriş — ClearPay (Demo) | ASP.NET Core 8 cüzdan demosu. Giriş yapın. Demo — sahte banka gateway; lisanslı ödeme kuruluşu değil. |
| Kayıt | Kayıt — ClearPay (Demo) | Portföy cüzdanına müşteri hesabı açın. Demo — sahte banka gateway. Gerçek banka / Papara değil. |
| Özet | Cüzdan özeti — ClearPay (Demo) | Bakiye, bu ay giden/gelen, son hareketler. ASP.NET Core ledger demosu. Demo — sahte banka gateway. |
| Havale | Havale — ClearPay (Demo) | Demo P2P havale (idempotency, 409). Sahte banka gateway. Gerçek FAST / IBAN yok. |
| Yükle / Çek | Yükle / Çek — ClearPay (Demo) | Sahte banka REST/SOAP gateway. Demo — gerçek POS veya kart yok. |
| Hareketler | Hareketler — ClearPay (Demo) | Demo hareket listesi ve dekont (correlation id). Lisanslı cüzdan değil. |

OG title = sayfa title. OG description = meta description. `og:type` = `website`. `og:locale` = `tr_TR`.

## Başlık hiyerarşisi

- Tek `h1` = ekran adı (Giriş, Hesap oluştur, Cüzdan özeti…).
- `h2` = kart / bölüm (Bakiye, Son hareketler).
- Wordmark “ClearPay” `h1` olmasın (layout markası).
- Footer ve auth altı: **Demo — sahte banka gateway** (Designer / Coder mevcut metin).

## Path’ler (CANLI)

Keşif ve sitemap **küçük harf** path kullanır. Bugün kodda giriş `/Account/Login`, kayıt `/Account/Register` — Coder `@page "/giris"` / `"/kayit"` ekler (`docs/CANLI.md`). SEO Razor’a dokunmaz.

| Path | Ekran | İndeks |
|------|--------|--------|
| `/` | Özet (anonim → giriş) | evet (canonical canlı kök) |
| `/giris` | Giriş | evet |
| `/kayit` | Kayıt | evet |
| `/havale` | Havale | hayır (cookie) |
| `/yukle-cek` | Yükle / çek | hayır |
| `/hareketler` | Hareketler | hayır |
| `/admin` | Admin | hayır |
| `/api/...`, `/swagger` | API | hayır |

Eski `/Account/Login` ve `/Account/Register` canlıda `/giris` / `/kayit`’e 301 (Coder, path değişince).

## Sitemap URL’leri

Dosya: `src/ClearPay.Web/wwwroot/sitemap.xml` (placeholder host).

```
https://clearpay.azurewebsites.net/
https://clearpay.azurewebsites.net/giris
https://clearpay.azurewebsites.net/kayit
```

Auth sayfaları sitemap’te yok. Host değişince XML + `robots.txt` Sitemap satırı güncellenir.

## robots.txt

`src/ClearPay.Web/wwwroot/robots.txt`

- `Allow: /`
- `Disallow: /admin`, `/api/`, `/swagger`, `/Account/Logout`
- `Sitemap: https://clearpay.azurewebsites.net/sitemap.xml`

Lokalhost’u Search Console’a ekleme.

## JSON-LD (demo)

`SoftwareApplication` — `BankOrCreditUnion` / `FinancialProduct` **yasak**.

```html
<script type="application/ld+json">
{
  "@context": "https://schema.org",
  "@type": "SoftwareApplication",
  "name": "ClearPay",
  "applicationCategory": "DeveloperApplication",
  "operatingSystem": "Web",
  "description": "ASP.NET Core 8 dijital cüzdan demosu. Demo — sahte banka gateway. Lisanslı ödeme kuruluşu değildir.",
  "url": "https://clearpay.azurewebsites.net/",
  "offers": { "@type": "Offer", "price": "0", "priceCurrency": "TRY" }
}
</script>
```

Coder layout’a koyar (aşağıdaki HANDOFF). URL host ile aynı tutulur.

## Coder’a meta / canonical (layout)

`_Layout.cshtml` ve `_AuthLayout.cshtml` **dokunulunca** ekle — SEO Pages/*.cshtml savaşmaz:

```html
<meta name="description" content="@(ViewData["MetaDescription"] ?? "ASP.NET Core 8 cüzdan demosu. Demo — sahte banka gateway.")" />
<link rel="canonical" href="https://clearpay.azurewebsites.net@(ViewContext.HttpContext.Request.Path)" />
<meta property="og:title" content="@(ViewData["Title"] ?? "ClearPay") — ClearPay (Demo)" />
<meta property="og:description" content="@(ViewData["MetaDescription"] ?? "Demo — sahte banka gateway.")" />
<meta name="robots" content="index,follow" />
```

Cookie arkası sayfalarda `noindex,nofollow`. Canonical host `docs/CANLI.md` ile değişir (şimdilik placeholder).

## Search Console + GA4 (kullanıcı açar)

Ajan hesap açmaz, doğrulama meta’sını uydurmaz, reklam bütçesi harcamaz.

1. Canlı URL tarayıcıda açılsın (`docs/CANLI.md` / TASK-16). Lokal yetmez.
2. [Google Search Console](https://search.google.com/search-console) — **URL öneki** = canlı kök (`https://clearpay.azurewebsites.net` veya özel domain).
3. Doğrulama: HTML dosyası `wwwroot/` **veya** DNS TXT **veya** GA4. Token’ı sen yapıştırırsın; git’e secret koyma.
4. Sitemap gönder: `https://<host>/sitemap.xml`
5. [GA4](https://analytics.google.com) mülk oluştur; Measurement ID’yi App Settings’e sen koyarsın (`GoogleAnalytics__MeasurementId` önerilir). Repo’ya ID şart değil.
6. İsteğe bağlı: Search Console ↔ GA4 bağlantısı.

## CEO / “Google’da öne çıkarma”

Şirket CEO ofisi yok. Anlam: **CV / konumlandırma** — mülakatçı Google’da “ClearPay ASP.NET Core cüzdan demo” arayınca bu repo çıksın.

- Anahtar: `ASP.NET Core cüzdan demo`, `ClearPay wallet demo`, `.NET 8 ledger`, `idempotency 409`.
- Yasak niyet: `ucuz havale`, `Papara alternatif`, `gerçek IBAN`, `kredi`.
- README + title + JSON-LD aynı hikâye: demo, sahte gateway, lisans yok.

## Yasak

- Gerçek banka / Papara / BDDK / ödeme kuruluşu iddiası
- Ads/Search Console/Analytics hesabını ajanın açması
- Coder WIP Razor’ı ezmek; Designer TASARIM/MARKA’yı üzerine yazmak
- `BankOrCreditUnion` schema
