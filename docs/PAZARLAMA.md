# PAZARLAMA — üç kanal (GitHub, LinkedIn, demo URL)

ClearPay lisanslı ödeme kuruluşu **değildir**. Portföy / mülakat demosu. Her genel paylaşımın ilk satırında: **Demo — sahte banka gateway.**

Amaç: hiring manager repo’yu, LinkedIn postunu ve (TASK-16 sonrası) açık HTTPS URL’yi bulsun. Tüketici cüzdan kullanıcısı çekmek **yok**. Papara rakibi reklam **yok**.

Ses: [`MARKA.md`](MARKA.md). Pitch: [`SATIS.md`](SATIS.md) / [`FARK.md`](FARK.md) — burada yeniden yazılmaz.

---

## Bu belge ne / ne değil

| Belge | Sahip | Pazarlama |
|-------|--------|-----------|
| [`SEO.md`](SEO.md) | SEO/Ads | Title, description, sitemap, robots, JSON-LD, Search Console, GA4. **Kopyalama.** |
| [`ADS.md`](ADS.md) | SEO/Ads | Google Ads taslağı, anahtar, negatif. Kampanya **yalnızca canlı URL sonrası**. **Kopyalama.** |
| [`PR.md`](PR.md) | PR | Dürüst sıralama (havale/Papara #1 yok) + launch günü sırası. **Kopyalama.** |
| **Bu dosya** | Pazarlama | Üç kanalı **işlet**: ne zaman, kim tıklar, hazır metin. |

Launch sırasının kaynağı [`PR.md`](PR.md) §3’tür. Pazarlama o sırayı bozmaz; kanal metnini burada tutar.

---

## 1. GitHub — asıl kanıt

Repo: [HalilMertDeveli/clearpay](https://github.com/HalilMertDeveli/clearpay) — **public**, `main`. Yeni hesap yok ([`SENIN-ISLERIN.md`](SENIN-ISLERIN.md) madde 4).

**Kullanıcı tıklar** (Settings → General). Ajan GitHub UI açmaz.

| Alan | Metin / kural |
|------|----------------|
| **About** | `ASP.NET Core 8 wallet demo: double-entry ledger, idempotent P2P (409), mock bank gateway. Not a licensed wallet.` |
| **Website** | TASK-16 tarayıcıda açık olunca canlı kök. O zamana kadar **boş** (localhost koyma). |
| **Topics** | Liste [`PR.md`](PR.md) §2.3 — burada tekrar yok. Örn. `aspnetcore` `dotnet` `wallet` `ledger` `idempotency` `demo`. `papara` / `havale` topic **yok**. |

README İngilizce yüzü **TASK-14 Coder**. Pazarlama README’yi şişirmez; ilk satır disclaimer + 15s pitch Sales’ten gelir.

Yasak: sahte star, “Papara klonu” About, private’a çekmek, `src/`’ye pazarlama yorumu.

---

## 2. LinkedIn — aday sesi, tek launch postu

İş ilanı, müşteri kazanma, Papara rakibi sponsored yok. **Kullanıcı** yayınlar; ajan hesap açmaz.

### Ne zaman

| Durum | Post |
|-------|------|
| Şimdi (canlı URL yok) | Launch postu **yok**. “Yayındayız” yok. |
| TASK-14 README origin’de, URL yok | İsteğe bağlı **repo-only** (taslak A). Lokalhost linki yok. |
| TASK-16 URL tarayıcıda + Search Console gönderildi | **Tek** launch postu (taslak B). [`PR.md`](PR.md) adım 4. |
| Ads / boost | Papara / havale niyeti hedefleme **yok**. İsteğe Search: [`ADS.md`](ADS.md). |

### Taslak A — repo (URL yokken)

> ClearPay — ASP.NET Core 8 **cüzdan demosu** (sahte banka gateway). Çift kayıt defteri, aynı havale **409**, timeout’ta outbox. Lisanslı ödeme kuruluşu değil; Papara/FAST değil.  
> Repo: https://github.com/HalilMertDeveli/clearpay  
> Demo — sahte banka gateway.

### Taslak B — launch (canlı URL var)

> ClearPay **demo** canlı: {CANLI_KOK}  
> ASP.NET Core 8 cüzdan demosu — ledger, idempotency 409, sahte banka REST+SOAP. Gerçek havale / IBAN yok.  
> Kod: https://github.com/HalilMertDeveli/clearpay  
> Demo — sahte banka gateway.

`{CANLI_KOK}` = tarayıcıda açılan host ([`CANLI.md`](CANLI.md): `https://clearpay.azurewebsites.net` veya yedek). Uydurma URL yok.

İsteğe TR Medium: aynı yasaklar, uzun hali Sales 2 dk ([`SATIS.md`](SATIS.md)) — Pazarlama ikinci bir pitch uydurmaz.

---

## 3. Demo URL — paylaşılacak tek site

| Durum | Ne paylaşılır |
|-------|----------------|
| TASK-16 yok | **Hiçbir genel kanalda URL yok.** Lokal: http://localhost:5153 yalnızca senin makinen. |
| TASK-16 tarayıcıda açık | HTTPS kök + `/giris`. Path’ler [`CANLI.md`](CANLI.md). |
| Host değişince | GitHub Website + bu taslaklardaki `{CANLI_KOK}`. Sitemap/robots **SEO** günceller. |

İlk cümle her zaman Demo. “Güvenli para gönder”, “canlı cüzdan aç” yok.

---

## Takvim (PR sırasına bağlı)

Kaynak sıra: yeşil build → Azure URL → Search Console → LinkedIn → isteğe Ads ([`PR.md`](PR.md)).

```
Şimdi     GitHub public (About/topics kullanıcı)
TASK-14   README EN (Coder) → isteğe taslak A
TASK-15   Actions yeşil (Deploy) — paylaşım şartı
TASK-16   Demo URL tarayıcıda → Website alanı + taslak B
Sonra     SC/GA4 kullanıcı ([SEO.md](SEO.md)); Ads isteğe ([ADS.md](ADS.md))
```

TASK-03…14 bitmeden “lansman” yok. Azure hesabı yokken URL yok. Ads, LinkedIn’den **önce** yok.

---

## Kim tıklar

| İş | Kim |
|----|-----|
| GitHub About, Website, topics | **Kullanıcı** |
| LinkedIn / Medium yayın | **Kullanıcı** (taslak burada) |
| Azure / DNS / App Settings | **Kullanıcı** ([`CANLI.md`](CANLI.md)) |
| Search Console, GA4, Ads hesabı | **Kullanıcı** ([`SEO.md`](SEO.md), [`ADS.md`](ADS.md)) |
| Title, robots, sitemap | SEO/Ads; layout tag **Coder** |
| Bu playbook + taslak | Pazarlama |

Ajan hesap açmaz, bütçe harcamaz, hosting açmaz.

---

## Papara rakibi reklam — yasak (üç kanal + Ads)

- “Papara alternatif”, “Papara gibi”, “onlardan ucuz havale”
- LinkedIn/Ads’te Papara, Tosla, ininal, FAST, EFT, “havale at” niyeti
- Rakip logo, BDDK / lisans / gerçek IBAN
- Negatif kelime listesi ve Search metinleri **[`ADS.md`](ADS.md)** — burada çoğaltılmaz

Şüphede footer: **Demo — sahte banka gateway.**

---

## El değiştirme

- **SEO/Ads:** on-page + Ads taslağı. Pazarlama title/reklam kopyalamaz.
- **PR:** sıralama dürüstlüğü + launch sırası. Pazarlama sıra atlamaz.
- **Sales:** 15s / CV. LinkedIn taslağı oradan; yeni wedge yok.
- **Coder:** `_Layout` meta (`SEO.md`). Pazarlama `.cshtml` / `src/` yazmaz. **TASK-03 kesilmez.**
- **Deploy:** canlı host. Pazarlama Azure açmaz.
- **Orchestrator:** `AGENTS.md` Pazarlama satırı (bu ajan `AGENTS.md` yazmaz).
- **Org:** [`ORGANIZASYON.md`](ORGANIZASYON.md) Pazarlama birimi; bu dosya kanal OWN.
