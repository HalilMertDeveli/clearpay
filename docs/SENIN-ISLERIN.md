# Senin işlerin

Kod ve TASK sırası ajanın işi. Sen hesap, makine ve sırları tutarsın.

- Baştan oku: [`KRONIK.md`](KRONIK.md)
- Nedenler: [`OGRENME.md`](OGRENME.md)
- Ödeme nasıl: [`ODEME-SENIN.md`](ODEME-SENIN.md)
- Google/Apple giriş (OAuth sen, kod Coder): [`GIRIS-SOSYAL.md`](GIRIS-SOSYAL.md)
- Azure (şimdi hesap açma): [`CANLI.md`](CANLI.md)

## Operasyon kimliği

Bütün hesaplar **`halilmertdeveliii@gmail.com`** (Gmail). Yeni Microsoft / Google / GitHub hesabı açma. Parola, App Password, connection string, JWT git’e koyma; ajan bunları sormaz.

| Servis | Bu Gmail’de | Ajan ne yapmaz |
|--------|-------------|----------------|
| **GitHub** | `HalilMertDeveli` — primary e-posta doğrulanmış; repo `HalilMertDeveli/clearpay` public `main` | Yeni hesap / force push / secret |
| **Gmail MCP** | Aynı kutu; etiket `ClearPay` | Papara maili, Ads harcaması |
| **Azure** | Portal hesabı bu Gmail iddiası; bu makinede `az` yok → abonelik görünmedi | Abonelik uydurma, Portal açma, URL uydurma |
| **Search Console / Ads** | Canlı URL sonrası sen; harcama yok | Hesap açma, kampanya, “ucuz havale” |

TASK-16: [`CANLI.md`](CANLI.md) (T-104). Canlı kök **https://clearpay-eecuaqc7c5ehbmb5.canadacentral-01.azurewebsites.net**. `az login` / `deploy.ps1` **yapma** (mevcut siteyi ezer). Kalan tık: Get publish profile → GitHub secret.

## Yol haritası — senin tıkların (plan)

Kalıcı metin: [`YOL.md`](YOL.md) (T-059). `src/` yok. TASK listesi şişmez. Bu makinede **`az` hâlâ yok** (2026-08-17 tekrar bakıldı).

1. **TASK-16 HTTPS** — site duruyor; zip yok (`/api/health` 404). Portal **`ClearPay_group` → `ClearPay` → Get publish profile**. GitHub Settings → Secrets → Actions → **`AZURE_WEBAPP_PUBLISH_PROFILE`** (XML; sohbete yapıştırma). Portal startup **`dotnet ClearPay.Web.dll`**, **HTTPS Only On**. SQL + `Jwt__SigningKey` App Settings. Sonra Actions **Azure deploy** (`main`). Tarayıcı: https://clearpay-eecuaqc7c5ehbmb5.canadacentral-01.azurewebsites.net/api/health sonra `/giris`. Production’da `admin@clearpay.test` yok — `/kayit`. Ayrıntı: [`CANLI.md`](CANLI.md).
2. **Mülakat kanıtı** — URL tarayıcıda **açık olduktan sonra:** GitHub Website = canlı kök. LinkedIn **taslak B** ([`PAZARLAMA.md`](PAZARLAMA.md); kopya [`IK.md`](IK.md)). 15/30 dk: kayıt → özet → havale iki kez → **409**. Ezber: 409 / transaction / outbox.
3. **Kapı** — Intertech, Softtech, Bileşim, TAV, İGA, uni BT, sanayi, Turkcell **.NET**. CV üç satır README. Java ilanı yok. Adres: [`GELIR.md`](GELIR.md) §5. Ajan İK paneli açmaz.
4. **Q2 ticari** — white-label veya kapalı devre **şimdi yok**. Avukat + yeni TARTISMA. 9. ekran / satıcı paneli yok. Kendi e-para lisansı kapalı (40 / 105 milyon TL).

## Her gün

1. **Visual Studio** — `D:\ClearPay\clearpay\ClearPay.slnx` (C: kopyası junction; asıl repo D:).
2. **Docker Desktop** açık tut. SQL için repo kökünde: `docker compose up -d`.
3. **Lokal kontrol** — http://localhost:5153 — sol menü, sonra giriş / kayıt.
4. **GitHub** hazır: `HalilMertDeveli/clearpay` (public, `main`). Yeni hesap açma.
5. **Secret / şifre git’e koyma** (connection string, JWT, Gmail App Password, Azure SQL).
6. **İleride Azure** — aboneliği **sen** açarsın; ajan talimat yazar, hesabı açmaz.
7. **DNS / domain** — senin panelin.
8. **Canlıda App Settings** (TASK-16) — sen yapıştırırsın: Azure SQL connection string, JWT. Ödeme kuruluşu KEY yok.
9. **Kod yazmak / hangi TASK** — senin işin değil. Komut: «sıradaki işi yap» / «devam».
10. **Search Console / GA4 / Ads** — canlı URL sonrası sen açarsın; harcama yok. “Ucuz havale” yazma.

---

## Canlı + Google (PR)

Playbook: [`PR.md`](PR.md). **Dürüst:** “havale” / “Papara”da #1 olmayız (ücretli tüketici araması + lisans).

- **Canlı URL** — https://clearpay-eecuaqc7c5ehbmb5.canadacentral-01.azurewebsites.net ([`CANLI.md`](CANLI.md) T-104). Zip + `/giris` hâlâ Halil secret/deploy. Özel domain sonra.
- **Google** — Search Console doğrulaması **sen**; sitemap/robots/meta ajan ([`SEO.md`](SEO.md)). Title: `ClearPay — ASP.NET Core cüzdan demo`. `/giris` ince kopya indeks olmasın.
- **Üst sıra (gerçekçi)** — `ClearPay ASP.NET`, `idempotent wallet .NET`, repo, senin adın + ClearPay. README İngilizce, bir LinkedIn/Medium, GitHub topics.
- **Launch sırası** — 1 yeşil build 2 Azure URL 3 Search Console 4 LinkedIn 5 isteğe Ads ([`ADS.md`](ADS.md); başlıkta Demo, “ucuz havale” yok).

---

## Ödeme yöntemi — senin işin

ClearPay’de ödeme **Papara başvurusu değil**. Sahte banka + çift kayıt. Adımlar: [`ODEME-SENIN.md`](ODEME-SENIN.md).

### Sen yapmazsın

Gerçek banka / Papara / iyzico / FAST başvurusu. POS, 3D Secure, lisans. Gerçek para yatırma. Ads’te “ucuz havale / Papara alternatif”.

### Sen yaparsın (nasıl)

1. Demo olduğunu bil — sahte `BankGateway`. Ekrana “gerçek havale” diye bakma.
2. Docker Desktop → `docker compose up -d`. Site: VS F5 veya `dotnet run --project src/ClearPay.Web --launch-profile http` → http://localhost:5153
3. TASK-03+: kayıt → giriş → özet 0 ₺. TASK-06+: havale; aynı gönderi iki kez → **409**, bakiye çift düşmez.
4. TASK-07: yükle/çek’te sahte **timeout**; kuyruk kaydı kalsın.
5. Canlıda: Azure’u sen açarsın (`CANLI.md`). App Settings’e SQL + JWT. **Ödeme KEY yok.**
6. Mülakat: 15/30 dk script + 409/tx/outbox + hangi firma (`IK.md`). Kısa üçlü: `OGRENME.md` / `FARK.md` / `KRONIK.md` §13.
7. Google Ads’te “ucuz havale / Papara alternatif” **yazma**.
8. **Banka / iş yeri sunumu** — pitch, rota: [`GELIR.md`](GELIR.md). Nereye gider: [`YOL.md`](YOL.md). Lisans başvurusu yok.

---

## Google / Apple ile giriş — senin işin

Ayrıntı: [`GIRIS-SOSYAL.md`](GIRIS-SOSYAL.md). **Kod yazmazsın.** Buton Coder’da.

### Sen yapmazsın

Razor / Identity kodu. Secret’ı `appsettings.json` veya git. Yeni Google hesabı. Apple Developer’ı ajanın açması.

### Sen yaparsın (nasıl)

1. **Google Cloud** (mevcut Gmail) → OAuth **Web** client. Origin: `http://localhost:5153`. Redirect: `http://localhost:5153/signin-google`.
2. Client ID + secret’ı **yalnızca user-secrets**’a yapıştır:

```powershell
cd C:\Users\clt\Projects\clearpay\src\ClearPay.Web
dotnet user-secrets init
dotnet user-secrets set "Authentication:Google:ClientId" "..."
dotnet user-secrets set "Authentication:Google:ClientSecret" "..."
```

3. `git status` bu değerleri **göstermez**. Gösterirse dur; commit etme.
4. **Apple** isteğe bağlı (ücretli Developer). Yoksa atla. Varsa `GIRIS-SOSYAL.md` §2.
5. Canlıda aynı anahtarlar Azure App Settings (`Authentication__Google__...`); canlı redirect URI ekle. TASK-16’dan önce şart değil.

## Flutter Firebase (T-065) — senin işin

Ajan Firebase projesi açmaz. Kasa SQL’de kalır; Firestore bakiye yok.

1. [Firebase console](https://console.firebase.google.com/) — bu Gmail. Proje: ClearPay demo (veya mevcut Google Cloud).
2. Command Prompt:

```bat
npm install -g firebase-tools
firebase login
cd /d D:\ClearPay\clearpay\mobile\clearpay
tool\configure-firebase.cmd
```

3. Yeni Google hesabı yok. Secret’ı sohbete yapıştırma. `flutterfire` yazdığı `firebase_options.dart` / `google-services.json` istemci anahtarıdır (JWT signing key değil).
4. **Authentication → Sign-in method → E-posta/şifre** aç (SMS/Blaze yok). Android Studio’da ekstra json üretme: dosya `mobile/clearpay/android/app/google-services.json` (paket `com.clearpay.clearpay`). iOS `GoogleService-Info.plist` Runner’da. Web `/giris` değişmez.
5. **Firestore** (T-091): Console → Firestore Database (zaten açtın). Rules **default deny** ise uygulamada ping log’da skip olur; JWT girişi durmaz. İstersen yalnız `app_meta/ping` yazımına izin ver — **bakiye/havale koleksiyonunu public write açma**. Kasa SQL’de kalır.

## Mobil ↔ web canlı bakiye (T-071) — senin işin

Kod ajanın. Sen API’yi ayağa kaldırır, JWT ve 409’u Swagger’da kanıtlarsın. Adım adım: [`API-ESZAMAN.md`](API-ESZAMAN.md).

1. Site http://localhost:5153 ayakta (`dotnet run` veya VS F5).
2. http://localhost:5153/swagger → `POST /api/token` → Authorize Bearer.
3. Para POST’larında her seferinde yeni `Idempotency-Key` (ikinci aynı key → **409**).
4. İki istemci: tarayıcı özet açık + Flutter aynı hesap havale. Web kendiliğinden yenilenmeli.
5. İkinci veritabanı / Firestore bakiye yok. Azure SignalR Service şimdi yok. TASK-16 URL sende.
