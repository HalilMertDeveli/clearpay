# SPEC — ClearPay

## Ürün
Dijital cüzdan **sitesi** (WePay benzeri). İnsanlar para gönderir / öder **bu sitede**. Sahte perakende banka uygulaması değil: şube, IBAN çekirdeği, “BankaX” UI yok.

Canvas kilit: Papara / banka mobil **havale hissi** (navy; sol menü **Özet, Havale, Yükle/Çek, Hareketler, Admin**). Kayıt/giriş, cüzdan özeti, havale, yükle/çek, hareketler/dekont, admin.

**Sahte olan yalnızca BankGateway** (REST+SOAP stub): yükle/çek timeout/retry entegrasyon stand-in’i. Kullanıcının gördüğü uygulama banka değildir. Gerçek POS / FAST / BOA yoktur.

## Dil ve stack (kilit)
- Dil: **C# 12**
- Web: **ASP.NET Core** (Razor Pages + Web API, aynı uygulama)
- Runtime: **.NET 8** (ilanlarda “.NET Core”)
- DB: **SQL Server** (lokal Docker, canlı Azure SQL)
- Kullanılmaz: Java, .NET Framework 4.8, klasik ASP.NET, gerçek ödeme kuruluşu

## Hedef
Kurumsal .NET mülakatında anlatılır, internette açılan demo. Kapı: Intertech, Bileşim, Softtech, TAV, İGA, uni BT, sanayi, Turkcell .NET. Açılmaz: Trendyol/THY/Garanti Java, ASELSAN C++.

## Roller
- **Musteri** — cüzdan, havale, yükle/çek, hareketler, dekont
- **Satici** — Q2 (ayrı ekran yok; şimdilik kapsam dışı)
- **Admin** — dondur, başarısız kuyruk, audit ara

## Ekranlar (sabit liste)
| # | Ekran | Ne görünür | Butonlar |
|---|--------|------------|----------|
| 1 | Giriş | E-posta, şifre. Link: hesap oluştur | Giriş |
| 2 | Kayıt | Ad, e-posta, şifre, şifre tekrar | Hesap oluştur |
| 3 | Cüzdan özeti | Bakiye, bu ay giden/gelen, son 5 hareket | Havale gönder, Yükle, Çek |
| 4 | Havale | Alıcı, tutar, açıklama, kalan bakiye | Gönder, İptal |
| 5 | Yükle / Çek | BankGateway stub, tutar, IBAN benzeri. Durum: başarı / timeout | Yükle, Çek, İptal |
| 6 | Hareketler | Tarih, işlem no, tür, karşı taraf, tutar, durum. Filtre + sayfa | Filtrele, Dekont |
| 7 | Dekont | Tek işlem: taraflar, tutar, correlation id, zaman | Geri |
| 8 | Admin | Kullanıcı dondur. Başarısız kuyruk. Audit arama | Kuyruğa al, Dondur, Ara |

Sol menü her sitede aynı: **Özet, Havale, Yükle/Çek, Hareketler, Admin** (Admin yalnızca role göre).

Arayüz tahmini mockup’lara yakın Razor; kesin Figma değildir. Diller: **Türkçe (varsayılan), English, Deutsch, Français** — cookie `c=tr|en|de|fr`. Dil seçici **layout chrome** (sol menü / üst çubuk); 9. ekran değil. Ads/Papara metni çevrilmez. Görsel: navy `#1B2A4A`, beyaz zemin, gölge/gradient yok.

## Para kuralları (bozulmaz)
1. **Çift kayıt defteri:** her harekette + ve − satırı (`LedgerEntry`)
2. **Tek kesinti:** aynı `Idempotency-Key` ikinci kez **409 Conflict**; ikinci kesinti yok
3. **Bakiye invarianti:** cüzdan bakiyesi = ledger net; negatif bakiye yok
4. **Dondurulmuş cüzdan** gönderemez / çekemez
5. **Iade** ledger ile ters kayıt; bakiye “elle” düzeltilmez
6. **Audit:** kim, ne, ne zaman, correlation id
7. **Outbox:** ledger ile **aynı SQL transaction**; timeout’ta kaybolmaz

## Veri
`User`, `Wallet`, `LedgerEntry`, `Transfer`, `IdempotencyRecord`, `AuditLog`, `OutboxMessage`

## API
- Site: cookie (Identity)
- JSON API: JWT + OpenAPI/Swagger
- Havale: `POST /api/transfers` + `Idempotency-Key` → başarı 201, tekrar 409

## Kapsam dışı (şimdi değil)
- Sahte banka uygulaması (şube, IBAN çekirdeği, “BankaX” perakende UI)
- Gerçek banka / POS / 3D Secure
- Satıcı ödemesi ekranı (Q2 adayı)
- Kafka, Kubernetes, Java ikizi
- LED teknik destek sitesine özellik eklemek

## Başarı kriteri
1. Lokal: Docker Compose ile site + SQL açılır
2. Giriş → boş/dolu cüzdan → havale → hareket → dekont çalışır
3. Çift tıklama tek kayıt; 409 anlatılır
4. BankGateway timeout → kuyruk tekrar dener
5. Testler yeşil; İngilizce README + Swagger
6. Azure’da açık URL
