# DESTEK — ClearPay demo yardım

**Bu bir banka yardım masası değil.** ClearPay lisanslı ödeme kuruluşu, e-para veya gerçek havale değildir. Sahte banka gateway vardır; FAST, POS, 3D Secure, gerçek IBAN yoktur. Para kaybı, iade, BDDK şikayeti veya Papara/banka bakiyesi buradan çözülmez.

Footer ile aynı kilit: **Demo — sahte banka gateway.**

Kim için: demo’yu tıklayan (sen veya mülakatçı). Pitch: [`SATIS.md`](SATIS.md). Neden 409: [`OGRENME.md`](OGRENME.md). Lokal/Azure: [`DEPLOY.md`](DEPLOY.md) / [`CANLI.md`](CANLI.md).

---

## Nasıl kayıt olurum?

1. **Lokal siteyi aç.** Docker Desktop açık. Repo kökü: `docker compose up -d` (SQL). Sonra `dotnet run --project src/ClearPay.Web --launch-profile http` veya Visual Studio F5 (`ClearPay.slnx`). Adres: http://localhost:5153
2. Giriş sayfasında **Hesap oluştur** (şimdilik `/Account/Register`; canlı path planı `/kayit`).
3. **Ad**, **e-posta**, **şifre**, **şifre tekrar**. Şifre: en az 8 karakter, bir küçük harf, bir rakam.
4. **Hesap oluştur** → rol `Musteri`, cookie, cüzdan özeti **0,00 ₺**.

Doğrulama e-postası yok; “şifremi unuttum” yok. E-posta yalnızca format kontrolü — gerçek inbox şart değil. Korumalı sayfalar (özet, havale, yükle/çek, hareketler) girişsiz **Giriş**’e gider.

Azure URL yokken kayıt yine lokaldir. Aşağıdaki “Azure yokken lokal” maddesine bak.

---

## Demo para nedir? Gerçek para mı?

Hayır. Ekrandaki ₺ **ledger satırıdır**; senin banka/Papara bakiyen buraya gelmez, buradan da çıkmaz.

| Ne görürsün | Ne olduğu |
|-------------|-----------|
| Kayıt sonrası **0,00 ₺** | Boş cüzdan. Hediyelik bakiye yok. |
| **Yükle / Çek** | Sahte `BankGateway` (TASK-07). Gerçek IBAN’dan para yatmaz / çekilmez. |
| **Havale** | Başka demo kullanıcıya (TASK-06). Gönder şu an form kabuğu; API yok. |
| İade | Ledger’da ters kayıt (kod kuralı). Destek “elle bakiye düzeltmez”. |

Yapma: gerçek kart, FAST, Papara başvurusu, Ads’te “ucuz havale”. Nasıl dene: [`ODEME-SENIN.md`](ODEME-SENIN.md).

---

## 409 nedir? Hata mı?

**409 Conflict** = aynı havale niyeti **zaten işlendi**. İkinci kesinti yok.

- İstek: `POST /api/transfers` + başlık `Idempotency-Key`
- Aynı key tekrar → **409**; bakiye ikinci kez düşmez
- Çift tıklama veya tarayıcı retry’si “iki kez gönderdim” olmasın diye

409 bir arıza kodu değil; “bu niyet tamam.” 500 veya “yetersiz bakiye” (4xx, bakiye değişmez) başka şeylerdir.

**Dürüst durum:** HTTP 409 henüz yok — havale API **TASK-06**. Gönder düğmesi bakiye 0 iken kapalı kalabilir. Mülakatta cümle hazır; kanıt kodda TASK-06 testidir.

---

## Timeout nedir? Param kayboldu mu?

Sahte gateway **bilerek** gecikebilir (Yükle/Çek, TASK-07; config veya ekran). Bu, gerçek banka kesintisi değildir.

Kural: timeout’ta ledger **kesinleşmez**; outbox / iş kuyruğu kaydı kalır; worker (Hangfire, TASK-11) tekrar dener. HTTP koptu diye “mesaj kayboldu” olmaz — önce DB (ledger + outbox aynı transaction), sonra yayın.

Tarayıcıdaki “istek zaman aşımı” ≠ destek iadesi. Canlı banka SLA’sı yok.

**Dürüst durum:** timeout kutusu ve worker henüz TASK-07 / TASK-11. Şimdi denemek: site ayağa kalksın; para motoru sıradaki task’larda.

---

## Azure yokken lokal nasıl açarım?

Canlı yayın **TASK-16**; Azure aboneliği **açılmadı**. Demo için localhost yeter.

```bash
docker compose up -d
dotnet run --project src/ClearPay.Web --launch-profile http
```

- Site: http://localhost:5153
- SQL: `localhost,1433` (yalnızca Docker; SA şifresi `.env.example` — Azure’da kullanma)
- Web Compose servisi değil; Redis/Rabbit **TASK-12**
- Ads / Search Console / özel domain **şimdi yok**

Azure’u **sen** açarsın; ajan hesap açmaz. Plan: [`CANLI.md`](CANLI.md) — West Europe, App Service Linux + Azure SQL, aday URL `https://clearpay.azurewebsites.net`. Connection string / JWT git’e konmaz.

F1 soğuk başlama (10–30 sn) **canlıda** geçerli; lokalde söz konusu değil.

---

## Cevaplamadığımız şeyler

- Gerçek para iadesi, IBAN kurtarma, kart itirazı
- Papara / banka / iyzico müşteri hizmetleri
- BDDK, lisans, “güvenli ödeme kuruluşu” teyidi
- Destek bileti, SLA, 7/24 hat
- LED teknik destek sitesi (başka ürün; bu repo değil)

Şifre sıfırlama veya e-posta doğrulama istersen: ürün özelliği yok; yeni demo hesap aç.

---

## Şimdi / sonra

| Konu | Şimdi | Sonra |
|------|--------|--------|
| Kayıt / giriş | Lokal cookie Identity; özet 0,00 ₺ | Canlı `/giris` `/kayit` (Coder) |
| Demo para | 0 ₺; yükle/çek API yok | TASK-07 sahte gateway |
| 409 | Cümle + Domain kuralı | TASK-06 HTTP + test |
| Timeout / outbox | Kural yazılı | TASK-07 kutu, TASK-11 worker |
| Azure | Yok | TASK-16, senin abonelik |
