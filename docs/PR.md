# PR — ClearPay internette görünsün (dürüst sıra)

Bu belge **öğretici playbook**’tur. Amaç: uygulamayı internette **canlı** ve **gerçekçi kelimelerde** bulunabilir kılmak. Kaynaklar: [`CANLI.md`](CANLI.md) (URL), [`SEO.md`](SEO.md) (teknik SEO — burada kopyalanmaz), [`ADS.md`](ADS.md) (reklam; harcama yok), [`SATIS.md`](SATIS.md) / [`FARK.md`](FARK.md) (metin).

**Kilit cümle (silinmez):** ClearPay lisanslı cüzdan değildir. Footer: **Demo — sahte banka gateway.**

---

## 1. Dürüst sıralama — ne olmayacağız

1. **“Havale”de #1 olmayız.** Bu kelime banka mobil, Papara, FAST, ücretli tüketici aramasıdır. Lisans + bütçe + marka ister. Demo cüzdan o yarışa girmez.
2. **“Papara”da #1 olmayız.** Papara lisanslı e-para / cüzdan markasıdır. Bizi “alternatif” diye konumlamak hem yalan hem Ads yasağıdır (`ADS.md`: “ucuz havale”, “Papara alternatif” yok).
3. **Neden.** Google’da üst sıra = niyet × yetki × backlink × (çoğu zaman) reklam. Tüketici “havale at” diye arar; hiring manager “ASP.NET cüzdan idempotent” diye arar. Biz ikincisine yazarız.
4. **Dürüst vaat.** Portföy demosu: açık HTTPS URL, indekslenen benzersiz title, GitHub, LinkedIn, kendi adın + ClearPay. Bu **kazanılabilir**. Tüketici cüzdan pazarı **kazanılamaz**.

Ezber: *Canlı ol, demo de, doğru kelimede görün. Havale kralı olma.*

---

## 2. Ne için görünebiliriz (beş kapı)

Aşağıdaki beş kapı **sırayla** açılır. 2’yi 1 olmadan, 5’i 2 olmadan zorlama.

### 2.1 Canlı URL (önce site açılsın)

1. Hedef Q1: Azure App Service **https://clearpay.azurewebsites.net** (doluysa `clearpay-wallet` / `hm-clearpay` — [`CANLI.md`](CANLI.md)).
2. Sonra (kullanıcı satın alır): özel domain + Azure managed certificate. HTTPS şart; HTTP kalmaz.
3. Path’ler küçük harf: `/` `/giris` `/kayit` `/havale` `/yukle-cek` `/hareketler`. Anonim kök → giriş.
4. **Sıra kodda:** TASK-15 GitHub Actions (`dotnet test` yeşil) → kullanıcı Azure aboneliği + RG → TASK-16 açık URL. Ajan Azure / DNS **açmaz**.
5. **Şimdi:** URL yoksa PR “yayındayız” demez. Playbook bekler; hesap açılmaz.

### 2.2 Google’da bulunma (indeks, kopya değil)

Ayrıntı **[`SEO.md`](SEO.md)** — PR yeniden yazmaz. Özet:

1. **Search Console** — mülkiyet doğrulaması **kullanıcı** (DNS veya HTML dosyası). Ajan hesap açmaz.
2. **sitemap.xml** + **robots.txt** — ajan / Coder (`wwwroot`); host CANLI’daki URL. `/admin` `/api/` `/swagger` Disallow.
3. **Tek title:** `ClearPay — ASP.NET Core cüzdan demo`. Her sayfada aynı ince kopya title yok; `/giris` ince yinelenen login sayfası gibi **index spam** olmasın (canonical + gerekirse noindex cookie-only).
4. **GA4** — ölçüm kimliği **kullanıcı** yapıştırır; ajan tag talimatı yazar, mülkiyet açmaz.
5. Meta / canonical / OG: layout’a **Coder** koyar (`SEO.md` “Coder’a meta”). PR Razor yazmaz.

### 2.3 Üst sıra hedefi (gerçekçi kelimeler)

Bunlarda **ilk sayfa / repo olarak görünmek** hedef; “havale” değil:

1. `ClearPay ASP.NET`
2. `idempotent wallet .NET`
3. GitHub: `HalilMertDeveli/clearpay` (public `main`)
4. Kişisel isim + ClearPay (LinkedIn, CV, README)

**İçerik (az, kaliteli):**

1. README **İngilizce** (TASK-14, Coder) — ilk satırlar: demo disclaimer + 15 sn pitch ([`SATIS.md`](SATIS.md)).
2. **Bir** LinkedIn veya TR Medium yazısı (aday sesi): 409 / çift kayıt / outbox. “Papara’yı geçtik” yok.
3. GitHub **topics** (kullanıcı repo Settings): örn. `aspnetcore`, `dotnet`, `csharp`, `wallet`, `ledger`, `idempotency`, `razor-pages`, `demo`.

### 2.4 PR kanalları

1. **GitHub public** — asıl kanıt. Private ise Google/aday göremez.
2. **LinkedIn (aday)** — launch günü tek post: canlı URL + “ASP.NET Core cüzdan **demo**” + repo. İş ilanı / müşteri kazanma tonu yok.
3. **README** — Google ve mülakatçının okuduğu yüz. Türkçe UI, İngilizce README.
4. **İsteğe TR Medium** — LinkedIn’in uzun hali; aynı yasaklar.
5. **Ads** — yalnızca **canlı Azure URL sonrası**. Başlıkta **Demo**. Kaynak [`ADS.md`](ADS.md). **“Ucuz havale” yok.** Hesap/harcama ajan açmaz; kullanıcı isterse.

### 2.5 Teknik SEO checklist — senin vs ajan

| # | İş | Kim |
|---|-----|-----|
| 1 | Azure aboneliği, App Service, DNS, App Settings | **Kullanıcı** ([`CANLI.md`](CANLI.md), [`SENIN-ISLERIN.md`](SENIN-ISLERIN.md)) |
| 2 | Search Console mülkiyet doğrula, sitemap gönder | **Kullanıcı** |
| 3 | GA4 mülkiyet + ölçüm kimliği | **Kullanıcı** |
| 4 | Google Ads hesabı / bütçe (isteğe, URL sonrası) | **Kullanıcı**; ajan yazmaz, harcamaz |
| 5 | LinkedIn / Medium paylaşımı | **Kullanıcı** (ajan taslak yazar) |
| 6 | `robots.txt`, `sitemap.xml`, title/description şablonları | **Ajan** (SEO/PR); Razor’a **Coder** |
| 7 | Layout meta + canonical | **Coder** (HANDOFF; PR cshtml ezmez) |
| 8 | GitHub topics, public repo | **Kullanıcı** (ajan liste önerir) |

Kural: **hesabı sen açarsın, meta’yı ajan hazırlar, Search Console’u sen doğrularsın.**

---

## 3. Launch günü sırası (bozma)

URL ve indeks aynı günde birbirini bekler. Sıra sabit:

1. **Build yeşil** — lokal `dotnet test` + TASK-15 Actions. Kırmızıysa Done yok, canlı yok.
2. **Azure URL** — tarayıcıda `https://….azurewebsites.net` (TASK-16). Giriş çalışır. HTTPS.
3. **Search Console** — kullanıcı doğrular, sitemap gönderir. Ajan “Google’a ekledim” diyemez.
4. **LinkedIn** — aday postu: Demo + URL + repo. Title kelimesi: ASP.NET Core cüzdan demo.
5. **İsteğe Ads** — 1–4 bittiğinde. Headline’da Demo. Negatif: kredi, gerçek IBAN, Papara alternatif.

TASK-03…14 bitmeden 2’ye atlama. Azure hesabı yokken 2 yok. Ads 2’den önce yok.

---

## 4. Öğretici notlar (neden bu sıra)

1. **İndekslenmeyen site reklamı** para yakar: Google Ads canlı URL ister; Search Console boş host’u sevmez.
2. **İnce yinelenen `/giris`** kaliteyi düşürür: her path’in kendine özgü title/description’ı [`SEO.md`](SEO.md)’de; login’i siteyle aynı H1 yapmak kopya sinyalidir.
3. **Konu kümesi** (ClearPay + .NET + idempotent) dar ve boştur; “havale” doludur. Dar kümede README + bir yazı yeter.
4. **Backlink** şimdilik LinkedIn + GitHub + (isteğe) Medium. Sahte dizin / “100 backlink paketi” yok.
5. **Ölçüm** (GA4) launch’tan sonra: kim demo’yu açtı. Hesap yine kullanıcıda.

---

## 5. Yasak

1. Azure / Google / Search Console / Analytics / Ads / registrar hesabı açmak.
2. “Papara alternatifi”, “ucuz havale”, BDDK / lisans / gerçek FAST iddiası.
3. Coder Razor, Identity, ledger, `site.css` yazmak.
4. [`SEO.md`](SEO.md) / [`ADS.md`](ADS.md) / SPEC ekran listesini yeniden yazmak.
5. LED teknik destek reposuna dokunmak.
6. TASK-16 URL yokken “yayındayız” veya Ads launch.

---

## 6. Ajan el değiştirme

- **Deploy:** URL ve HTTPS [`CANLI.md`](CANLI.md). PR hesap açmaz.
- **SEO/Ads:** title, robots, sitemap, Ads metinleri. PR özetler, kopyalamaz.
- **Coder:** `_Layout` meta + canonical; `/giris` `/kayit` `@page`. PR `.cshtml` yazmaz.
- **Sales:** pitch cümleleri LinkedIn/README için [`SATIS.md`](SATIS.md).
- **Orchestrator:** TASK-16 abonelik yokken başlatılmaz. PR yeni TASK açmaz.
