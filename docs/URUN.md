# URUN — ClearPay ürün sözleşmesi

Kaynak ekran: [`docs/SPEC.md`](SPEC.md). İş sırası: [`docs/PLAN.md`](PLAN.md). Bu belge **kim neyi neden görür** ve **bitti sayılır mı**. Ekran eklenmez. Pitch: [`SATIS.md`](SATIS.md). Görsel: [`TASARIM.md`](TASARIM.md).

**ClearPay — demo dijital cüzdan (WePay benzeri).** Sahte banka uygulaması değil (şube, IBAN çekirdeği, “BankaX” yok). Footer: **Demo — yükleme için sahte gateway** (yalnızca BankGateway stub). Gerçek POS / FAST / kart yok.

---

## Ürün (tek cümle)

Kayıtlı kullanıcı TL bakiyesini görür, başka kullanıcıya **bu sitede** havale eder, BankGateway stub ile yükler/çeker, geçmişi ve dekontu açar; admin dondurur ve iz sürer. Hedef: kurumsal .NET mülakatında açılan WePay benzeri cüzdan demosu — lisanslı cüzdan değil, sahte banka uygulaması değil.

## Kim

| Rol | Ne yapar | Ne yapmaz |
|-----|----------|-----------|
| **Musteri** | Giriş, kayıt, özet, havale, yükle/çek, hareketler, dekont | Admin, başka cüzdan dondurma, gerçek banka, şube / BankaX UI |
| **Admin** | Kullanıcı dondur, başarısız kuyruk, audit ara | Müşteri yerine havale; POS tahsilatı |
| **Satici** | — | **Q2.** Ayrı ekran yok. Şimdi hikâye yok. |

Anonim: yalnızca giriş ve kayıt. Cookie sonrası sol menü: **Özet, Havale, Yükle/Çek, Hareketler**; **Admin** yalnız `Admin` rolünde.

## Dilim (kullanıcı)

UI: TR/EN/DE/FR (varsayılan Türkçe; dil seçici layout chrome, 9. ekran değil). Para kültüre göre `0,00 ₺` / `0.00 ₺`. Navy `#1B2A4A`. Piksel Figma yok.

Canlı path (hedef): `/` `/giris` `/kayit` `/havale` `/yukle-cek` `/hareketler` `/admin`. Dekont menüde yok — hareketten açılır (ekran 7; path TASK-09).

---

## Ekranlar (SPEC sabit — 8)

| # | Ekran | Path | Birincil iş | Butonlar |
|---|--------|------|-------------|----------|
| 1 | Giriş | `/giris` | Hesaba gir | Giriş |
| 2 | Kayıt | `/kayit` | Musteri hesabı aç | Hesap oluştur |
| 3 | Cüzdan özeti | `/` | Bakiye + bu ay + son 5 | Havale gönder, Yükle, Çek |
| 4 | Havale | `/havale` | P2P gönder | Gönder, İptal |
| 5 | Yükle / Çek | `/yukle-cek` | BankGateway stub ile bakiye al/ver | Yükle, Çek, İptal |
| 6 | Hareketler | `/hareketler` | Liste + filtre | Filtrele, Dekont |
| 7 | Dekont | hareketten | Tek işlem kanıtı | Geri |
| 8 | Admin | `/admin` | Dondur, kuyruk, audit | Kuyruğa al, Dondur, Ara |

Dokuzuncu satır yok. Satıcı paneli, gerçek POS, kampanya landing’i, şube / BankaX yok.

---

## Hikâyeler ve kabul

Kabul **ürün dilinde**. Teknik 409 / ledger: SPEC + Payments. Task bitişi: PLAN.

### US-01 — Giriş (ekran 1, TASK-03)

**Olarak** ziyaretçi, **istiyorum** e-posta ve şifre ile girmek, **ki** cüzdanıma geçeyim.

**Kabul**
- Alanlar: e-posta, şifre. Link: **Hesap oluştur** → kayıt.
- Doğru kimlik → özet (`/`). Yanlış → hata, oturum yok.
- Anonim `/` `/havale` `/yukle-cek` `/hareketler` → girişe yönlenir.
- Sunucu validasyonu. “Beni hatırla” / sosyal giriş yok.

### US-02 — Kayıt (ekran 2, TASK-03)

**Olarak** ziyaretçi, **istiyorum** ad, e-posta, şifre ve tekrar ile hesap açmak, **ki** boş cüzdanla başlayayım.

**Kabul**
- Rol `Musteri` + cookie. Kayıt sonrası özet.
- Yeni cüzdan bakiyesi **0,00 ₺**; bu ay giden/gelen 0; son hareketler boş.
- Şifreler uyuşmaz / e-posta alınmış → hesap oluşmaz.
- Admin rolü self-serve yok.

### US-03 — Boş / dolu özet (ekran 3, TASK-03 sonra TASK-05)

**Olarak** Musteri, **istiyorum** bakiyeyi, bu ay giden/geleni ve son 5 hareketi görmek, **ki** sonraki işe (havale / yükle / çek) gideyim.

**Kabul**
- TASK-03: sabit sıfırlar + boş liste (Identity kanıtı).
- TASK-05: bakiye ledger net (veya invariant’lı kolon); donduk/aktif rozeti.
- CTA: **Havale gönder**, **Yükle**, **Çek**. Bakiye 0 iken Gönder disabled kalabilir.
- Boş durum: başlık + ne yapılacağı (`TASARIM.md`). Landing sayfası değil.

### US-04 — Havale (ekran 4, TASK-06)

**Olarak** Musteri, **istiyorum** kayıtlı alıcıya tutar ve açıklama göndermek, **ki** bakiyem ve alıcının bakiyesi aynı anda doğru kalsın.

**Kabul**
- Alanlar: alıcı (e-posta), tutar, açıklama; **kalan bakiye** görünür. **Gönder** / **İptal**.
- Başarı → özet veya hareketler; bakiyeler değişmiş.
- Aynı gönderiyi ikinci kez (aynı `Idempotency-Key`) → **409**; ikinci kesinti yok.
- Yetersiz bakiye / dondurulmuş / kendini / bulunamayan alıcı → 4xx, bakiye değişmez.
- IBAN / FAST / banka adı alanı **yok**.

### US-05 — Yükle / çek (ekran 5, TASK-07/08)

**Olarak** Musteri, **istiyorum** sahte **BankGateway** (REST/SOAP stub) üzerinden cüzdana yüklemek veya çekmek, **ki** başarı ve timeout’u göreyim.

**Kabul**
- İki kolon: Yükle | Çek. Tutar + IBAN **benzeri** (gerçek IBAN doğrulaması / EFT yok).
- Durum: başarı veya timeout. Timeout’ta ledger **kesinleşmez**; kuyruk/outbox kaydı kalır.
- REST sonra SOAP; kullanıcı aynı sonucu görür (`IBankGateway`).
- Dondurulmuş cüzdan çekemez. Gerçek POS, kart, 3DS, OTP **yok**. Şube / BankaX ekranı **yok**.

### US-06 — Hareketler (ekran 6, TASK-09)

**Olarak** Musteri, **istiyorum** tarih, işlem no, tür, karşı taraf, tutar, durum listesini filtreleyip sayfalamak, **ki** bir satırdan dekont açabileyim.

**Kabul**
- **Filtrele**, **Dekont**. Kendi hareketleri; başkasının defteri yok.
- Boş dönem: boş durum bloğu, sahte satır yok.

### US-07 — Dekont (ekran 7, TASK-09)

**Olarak** Musteri, **istiyorum** tek işlemin taraflarını, tutarını, **correlation id**’sini ve zamanını görmek, **ki** mülakatta iz sürebileyim.

**Kabul**
- **Geri** → hareketler. Yeni menü maddesi yok.
- Correlation id düz metin (kopyalanabilir). PDF aynı fişin belgesi (T-079, `/dekont/{id}?handler=Pdf`); e-posta yok.

### US-08 — Admin (ekran 8, TASK-10)

**Olarak** Admin, **istiyorum** kullanıcı dondurmak, başarısız kuyruğu görmek / kuyruğa almak ve audit aramak, **ki** para kaybı ve iz sorulunca cevap vereyim.

**Kabul**
- Yalnız `Admin`. Musteri `/admin` → 403 veya özet.
- **Dondur**: o cüzdan gönderemez / çekemez (SPEC kural 4).
- **Kuyruğa al**, **Ara** (kullanıcı, correlation id, tarih).
- Satıcı onayı, POS raporu, kampanya paneli yok.

---

## Kullanıcıya görünen para kuralları

Kod: SPEC § Para + Payments. Ürün cümlesi:

1. Her harekette iki satır (+/−). “Bakiye güncellendi” tek kolon değil.
2. Çift tıklama / aynı istek tekrarı ikinci kez kesmez (**409**).
3. Bakiye negatif olmaz; yetersizse işlem olmaz.
4. Dondurulmuş hesap gönderemez / çekemez.
5. İade = ters kayıt; ekranda “düzelt” butonu yok.
6. Timeout ödemeyi yutmaz (outbox; TASK-11).

---

## Kapsam dışı (şimdi değil)

Bunlar hikâye, ekran, kabul **değildir**. Coder’a TASK açılmaz.

| Dışarıda | Neden |
|----------|--------|
| **Sahte banka uygulaması** (şube, IBAN çekirdeği, BankaX) | Ürün WePay benzeri cüzdan sitesi. Sahte olan yalnızca `BankGateway`. |
| **Gerçek POS / sanal POS / kart / 3D Secure** | Acquiring, ACS, PCI. ClearPay checkout PSP değil. Sahte `BankGateway` var. |
| Gerçek banka, FAST, BOA, EFT, gerçek IBAN | UX hissi (alıcı, tutar, dekont) SPEC’ten; mesaj protokolü yok. |
| Satıcı paneli / pazaryeri tahsilatı | Rol `Satici` Q2; ekran listesinde yok. |
| Papara / iyzico / PayTR SDK, lisans, BDDK iddiası | Demo. Sales: `FARK.md`. |
| Kafka UI, Kubernetes konsolu, FIDS | SPEC yasak. |
| LED teknik destek sitesine cüzdan | Ayrı repo. |
| Google Ads harcaması, “ucuz havale” | SEO/Ads; canlı URL sonrası, Demo başlıkta. |

Q2 (satıcı, canlı Redis/Rabbit) ürün kararı **sonra**; Todo’da yok.

---

## TASK kapısı (ürün)

| Faz | TASK | Ürün “bitti” |
|-----|------|----------------|
| Görünen site | 03 | Giriş, kayıt, özet 0,00 ₺ |
| Para | 04–06 | Defter + canlı özet + havale 409 |
| Gateway + geçmiş | 07–09 | Yükle/çek (BankGateway stub), SOAP aynı sözleşme, hareket, dekont |
| Ops | 10–12 | Admin, outbox işler, lokal Redis/Rabbit |
| Kanıt | 13–16 | Test, README, CI, Azure URL |

Şimdi kritik yol: **TASK-03**. Havale/POS/Azure ürün olarak açılmaz. Coder TASK-03 = cüzdan login + boş özet (banka teması yok).

---

## El değiştirme

| Kime | Ne |
|------|-----|
| **Architect** | 9. ekran yok. Path `/giris` `/kayit` CANLI ile; dekont menüye eklenmez. |
| **Coder** | Hikâye alanları + buton adları. PageModel’de ledger yok. Identity TASK-03. Razor’u banka temasına çevirme. |
| **Payments** | US-04/05 para; 409 ve timeout kabulü kodda. Domain ledger durur. |
| **Tester** | Bu kabul + PLAN. TASK-03: kayıt → 0,00 ₺. TASK-06: aynı key 409. |
| **Designer** | Kompozisyon `TASARIM.md`; yeni sayfa yok. One-liner cüzdan/WePay. |
| **Sales / SEO / PR** | Demo kelimesi. WePay/cüzdan; “sahte banka uygulaması” yok. POS / Papara alternatif yok. |

Çatışma: SPEC ekran listesi kazanır. Bu dosya hikâye uydurarak listeyi genişletmez.
