# MARKA — ClearPay (CEO / ürün sesi)

CEO burada **ürün sesi**dir; şirket yönetim ofisi, hukuki imza veya gerçek banka yöneticisi değil.

## One-liner

**ClearPay — demo dijital cüzdan (WePay benzeri).** Sahte banka uygulaması değil.

Kısa: insanlar para gönderir / öder **bu sitede**. Portföyde açılan ASP.NET Core 8 cüzdanı. Intertech / Softtech / benzeri .NET mülakatı için. Papara ilanı değil; “BankaX” perakende banka UI değil.

## Ton

- Türkçe UI; düz, cüzdan sessizliği. Slogan yağmuru yok.
- Para ve durum cümleleri kısa. “Süper hızlı!”, emoji, ünlem yığını yok.
- Mülakatta: ne kodladığımızı söyle; “fintech unicorn” anlatma.
- Footer ve auth altı **her zaman**: **Demo — yükleme için sahte gateway**

## Demo disclaimer (silinmez)

Her genel yüzey (footer, auth, README, SEO, Ads taslağı):

**Demo — yükleme için sahte gateway.**

Açılım (gerekince bir cümle daha): lisanslı ödeme kuruluşu değil; ürün fake bank / şube / IBAN çekirdeği değil; gerçek POS / FAST / kart yok.

## CV hikâyesi

Anlatılan ürün: WePay benzeri cüzdan sitesi; idempotent P2P, çift kayıt defteri, yükle/çek için sahte **BankGateway** (REST + SOAP), timeout’ta kaybolmayan outbox. Sahte banka uygulaması değil.  
Kapı: kurumsal .NET (Intertech, Softtech, Bileşim, TAV, sanayi). Java/C++ ilanına bu repo ile girilmez.

README’deki İngilizce maddeler geçerlidir; abartı eklenmez. 409 / transaction / outbox henüz yeşil değilse “kanıtladım” denmez — “kural kilitli, TASK-06/11’de kanıt” (öğrenme defteri).

## Fark (CEO; rakip reklamı değil)

Kaynak: `docs/FARK.md`, `docs/SATIS.md`. Papara / Tosla alternatifi değiliz; fark **pazar değil, nasıl yazıldığı**. İki çizgi (Sales one-liner, ürün sesi):

1. **Bakiye tek kolon değil** — her işlem çift kayıt; aynı havale ikinci kez **409**, çift kesinti yok.
2. **Timeout ödemeyi silmez** — outbox ledger ile aynı transaction. Sahte **BankGateway** (yalnızca yükle/çek stub): REST ve SOAP, aynı sözleşme. Uygulama banka değil.

Asıl iddia ürün değil motor. “Onlardan ucuzuz / onları geçeriz” yok.

## Asla iddia etme

- Lisanslı ödeme kuruluşu, banka, e-para, BDDK/TCMB izni
- Sahte banka uygulaması, şube, IBAN çekirdeği, “BankaX” perakende UI
- Gerçek IBAN, FAST, havale EFT, POS, 3D Secure, kart
- Papara / Tosla / ininal rakibi, “daha ucuz havale”
- Üretim SLA, “paranız güvende”, yatırım tavsiyesi
- Google Ads’te Demo cümlesi olmadan kampanya (SEO/Ads ajanı; harcama yok)

## UI metin (Coder)

| Yer | Metin |
|-----|--------|
| Wordmark | ClearPay |
| Auth tagline | Demo cüzdan |
| Footer / sidebar not | Demo — yükleme için sahte gateway |
| Boş hareket başlık | Henüz hareket yok |
| Boş hareket ipucu | İlk havaleniz veya yüklemeniz burada görünür. |

SEO title/description: `docs/SEO.md`. Tasarımcı meta ezmez; ses çelişirse bu dosya + footer kilit cümlesi kazanır.
