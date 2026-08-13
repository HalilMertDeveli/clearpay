# MARKA — ClearPay (CEO / ürün sesi)

CEO burada **ürün sesi**dir; şirket yönetim ofisi, hukuki imza veya gerçek banka yöneticisi değil.

## One-liner

**ClearPay — demo cüzdan, sahte banka.**

Kısa: portföyde açılan ASP.NET Core 8 cüzdanı. Intertech / Softtech / benzeri .NET mülakatı için. Papara ilanı değil.

## Ton

- Türkçe UI; düz, banka sessizliği. Slogan yağmuru yok.
- Para ve durum cümleleri kısa. “Süper hızlı!”, emoji, ünlem yığını yok.
- Mülakatta: ne kodladığımızı söyle; “fintech unicorn” anlatma.
- Footer ve auth altı **her zaman**: **Demo — sahte banka gateway**

## Demo disclaimer (silinmez)

Her genel yüzey (footer, auth, README, SEO, Ads taslağı):

**Demo — sahte banka gateway.**

Açılım (gerekince bir cümle daha): lisanslı ödeme kuruluşu değil; gerçek POS / FAST / kart yok.

## CV hikâyesi

Anlatılan ürün: idempotent P2P, çift kayıt defteri, sahte banka (REST + SOAP), timeout’ta kaybolmayan outbox.  
Kapı: kurumsal .NET (Intertech, Softtech, Bileşim, TAV, sanayi). Java/C++ ilanına bu repo ile girilmez.

README’deki İngilizce maddeler geçerlidir; abartı eklenmez. 409 / transaction / outbox henüz yeşil değilse “kanıtladım” denmez — “kural kilitli, TASK-06/11’de kanıt” (öğrenme defteri).

## Fark (CEO; rakip reklamı değil)

Kaynak: `docs/FARK.md`, `docs/SATIS.md`. Papara / Tosla alternatifi değiliz; fark **pazar değil, nasıl yazıldığı**. İki çizgi (Sales one-liner, ürün sesi):

1. **Bakiye tek kolon değil** — her işlem çift kayıt; aynı havale ikinci kez **409**, çift kesinti yok.
2. **Timeout ödemeyi silmez** — outbox ledger ile aynı transaction. Sahte banka: REST ve SOAP, aynı sözleşme.

Asıl iddia ürün değil motor. “Onlardan ucuzuz / onları geçeriz” yok.

## Asla iddia etme

- Lisanslı ödeme kuruluşu, banka, e-para, BDDK/TCMB izni
- Gerçek IBAN, FAST, havale EFT, POS, 3D Secure, kart
- Papara / Tosla / ininal rakibi, “daha ucuz havale”
- Üretim SLA, “paranız güvende”, yatırım tavsiyesi
- Google Ads’te Demo cümlesi olmadan kampanya (SEO/Ads ajanı; harcama yok)

## UI metin (Coder)

| Yer | Metin |
|-----|--------|
| Wordmark | ClearPay |
| Auth tagline | Demo cüzdan |
| Footer / sidebar not | Demo — sahte banka gateway |
| Boş hareket başlık | Henüz hareket yok |
| Boş hareket ipucu | İlk havaleniz veya yüklemeniz burada görünür. |

SEO title/description: `docs/SEO.md`. Tasarımcı meta ezmez; ses çelişirse bu dosya + footer kilit cümlesi kazanır.
