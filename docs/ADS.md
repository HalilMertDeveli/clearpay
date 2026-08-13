# ADS — ClearPay (demo kampanya taslağı)

**Kampanya yok** ta ki Azure’da açık URL olana kadar (`docs/CANLI.md`, TASK-16). Lokalhost’a, `5153`’e veya “yakında yayınlarız” diye tıklama / bütçe yok.

Ajan Google Ads hesabı **açmaz**, kart bağlamaz, harcama yapmaz. Hesabı **sen** açarsın (`docs/SENIN-ISLERIN.md`).

Ses: `docs/MARKA.md` (Designer). Kilit: **Demo — sahte banka gateway.** Papara / banka rakibi gibi durma. `docs/TASARIM.md` ve Razor’a dokunulmaz.

## Ne zaman

```
Canlı URL tarayıcıda açılır
  → Search Console + GA4 (docs/SEO.md, sen)
  → isteğe bağlı Search kampanyası (bu belge, sen)
```

Amaç: mülakatçı / işveren “ASP.NET Core cüzdan demo” arayınca portföy çıksın. Müşteri kazanmak, havale satmak, cüzdan kullanıcısı çekmek **yok**.

## Anahtar kelimeler (yalnızca portföy)

| Kullan | Kullanma |
|--------|----------|
| ASP.NET Core cüzdan demo | ucuz havale |
| .NET 8 wallet demo | Papara alternatif |
| ClearPay ledger demo | gerçek IBAN |
| idempotency 409 ASP.NET | kredi, kredi kartı |
| çift kayıt defteri demo | FAST, EFT, Bode, Tosla |
| sahte banka gateway | ödeme kuruluşu, BDDK lisans |

Eşleme: **tam ve ifade** (phrase). Geniş eşleme fintech’e kayar — kullanma.

## 3 reklam metni (başlıkta Demo)

Google Ads sınırları: başlık ~30, açıklama ~90. Her başlıkta **Demo**.

### 1 — Stack

- Başlık 1: `ClearPay Demo Cüzdan`
- Başlık 2: `ASP.NET Core 8`
- Başlık 3: `Sahte Banka Gateway`
- Açıklama 1: `Portföy cüzdanı. Ledger, 409, outbox. Lisanslı ödeme kuruluşu değil.`
- Açıklama 2: `Demo — sahte banka gateway. Gerçek havale / IBAN yok.`

### 2 — Mülakat

- Başlık 1: `Demo — ClearPay`
- Başlık 2: `.NET Ledger Öğren`
- Başlık 3: `Cüzdan Portföy`
- Açıklama 1: `Mülakat demosu: çift kayıt, idempotency, sahte gateway.`
- Açıklama 2: `Papara veya banka değil. Demo — sahte banka gateway.`

### 3 — Keşif

- Başlık 1: `ASP.NET Cüzdan Demo`
- Başlık 2: `ClearPay — Demo`
- Başlık 3: `SQL Server Ledger`
- Açıklama 1: `Açık kaynak portföy. Azure’da dene. Gerçek para yok.`
- Açıklama 2: `Demo — sahte banka gateway. Finansal hizmet satılmaz.`

Son URL = canlı kök + `/giris` (veya `/`). Görünen URL yolu: `ClearPay Demo`.

## Negatif anahtar kelimeler

Mutlaka ekle (kampanya veya negatif liste):

- kredi
- kredi kartı
- gerçek IBAN
- IBAN satışı
- Papara alternatif
- Papara
- Tosla
- Bode
- ucuz havale
- ücretsiz havale
- FAST
- EFT gönder
- ödeme kuruluşu
- BDDK
- lisanslı cüzdan
- yatırım
- kripto cüzdan (yanlış niyet)
- banka hesabı aç

## Ayarlar (sen tıklarsın)

- Ağ: **yalnızca Arama**. Display / Performance Max / Demand Gen yok (fintech benzeri yerlere düşer).
- Konum: Türkiye (veya mülakat hedefi). Dil: Türkçe + İngilizce ayrı reklam grubu olabilir.
- Bütçe: günlük tavan **sen** koyarsın; ajan önermez / yükseltmez. 0 ₺ = kampanya yok, sorun değil.
- Dönüşüm: GA4 “oturum / demo girişi” yeter. “Havale tamamlandı”yı para dönüşümü gibi işaretleme.

## Yasak iddialar

- “Güvenli para gönder”, “lisanslı”, “Papara gibi”, “gerçek IBAN”, “ucuz / anında havale”
- Banka logosu, kart şeması, BDDK / TCMB dil
- Schema veya uzantıda `BankOrCreditUnion`

Şüphede `docs/SEO.md` + footer cümlesine dön.
