# ClearPay — plan ve adım adım işlemler

Tarih: **2026-08-13**. Okuyan: Halil. Uydurma durum yok; kaynak `docs/TASKS.md`, `docs/CANLI.md`, `docs/DEPLOY.md`, `docs/ARCHITECTURE.md`, `infra/`, `.github/workflows/`, `docker-compose*.yml`, `launchSettings.json`.

KRONIK öğrenme defteri, YONETICI-RAPORU yönetici özeti; **tıklama sırası bu dosyadır** (GitHub’da public). Notion kopyasının MCP’te “Publish to web” aracı yok — varsayılan **private**. Halil: sayfada **Share → Publish → Publish to web** (anyone with the link).

- Bu dosya (logged-out çalışır, push sonrası): [docs/OTURUM-PLAN.md](https://github.com/HalilMertDeveli/clearpay/blob/main/docs/OTURUM-PLAN.md)
- Notion (giriş isteyebilir ta ki Publish): [ClearPay — oturum adımları](https://www.notion.so/3bb31a8b18e4816bb34ffa405b4dec5d)

Repo: `C:\Users\clt\Projects\clearpay` · GitHub: [HalilMertDeveli/clearpay](https://github.com/HalilMertDeveli/clearpay)

---

## 0. Notion’u dışarı aç (sen tıklarsın)

Ajan sayfayı yazar; **web’de herkese açık yapamaz** (API yok).

1. Notion’da bu oturum sayfasını aç.
2. Sağ üst **Share**.
3. **Publish** / **Publish to web**.
4. “Anyone with the link” açık olsun. Arama motoru indeksi isteğe kapalı kalabilir.
5. Çıkan `notion.site` (veya `notion.so`) URL README Docs satırındakiyle aynı olsun.

GitHub README zaten public; bu markdown da public.

---

## Ürün (kilit)

ClearPay, WePay benzeri **dijital cüzdan sitesi**. Demo. Papara / banka rakibi değil; e-para lisansı, FAST, kart tahsilatı, şube, IBAN çekirdeği yok. Sahte olan yalnızca yükle/çek `IBankGateway` stub’ı.

SPEC **8 ekran** sabittir: Giriş, Kayıt, Cüzdan özeti, Havale, Yükle/Çek, Hareketler, Dekont, Admin. 9. ekran yok. Satıcı paneli / POS Q2, kapsam dışı.

---

## Mimari (kilit)

Derleme kuralı **Onion / Clean Architecture**. Klasik n-tier (UI → BLL → DAL, BLL içinde EF) yok. Aynı dört proje n-tier bilenlere **isim eşlemesi**dir — ikinci BLL/DAL ağacı yok:

| Onion (csproj) | n-tier adı (aynı proje) |
|----------------|-------------------------|
| `ClearPay.Domain` | iş kuralları / varlık |
| `ClearPay.Application` | BLL (use case + port) |
| `ClearPay.Infrastructure` | DAL + dış sistem |
| `ClearPay.Web` | sunum + composition root |

Web Domain’e doğrudan ProjectReference vermez. PageModel’de ledger yok. `UPDATE Balance` yok. Bakiye = `LedgerPair.NetOf`.

---

## Dürüst durum (`TASKS.md`, 2026-08-13)

| | Task | Gerçek |
|--|------|--------|
| **Done** | TASK-01…05, TASK-15 | Repo, Identity, ledger şema, canlı özet (`SqlWalletReader`), GitHub Actions |
| **Doing** | — | Boş |
| **Todo** | TASK-06…14, TASK-16 | Havale/409, gateway, hareket/dekont, admin, outbox, Redis bağlama, test sertleştirme, Swagger, **açık Azure URL** |

İddia etme: HTTP **409**, `POST /api/transfers`, canlı `azurewebsites.net`, uygulamada Redis/Rabbit, Hangfire worker, JWT/Swagger. Cookie `LoginPath` = `/giris`. 409 testi TASK-06’ya kadar skip.

CI: [Actions `ci.yml`](https://github.com/HalilMertDeveli/clearpay/actions/workflows/ci.yml) — `main` üzerinde restore / build / test. Testler `/giris` assert eder. Kırmızıysa ürün Done sayma.

---

## Adım adım — zaten hazır vs sen tıklarsın

Aşağıda **zaten hazır** = ajanın yazdığı / derlenen. **Sen tıklarsın** = hesap, makine, reboot, sır.

### 1. Lokal siteyi aç

**Zaten hazır:** .NET 8, `ClearPay.slnx`, Identity SQLite (`App_Data`), `http` profili port **5153**. Docker SQL **giriş için şart değil**.

**Sen tıklarsın:**

1. Visual Studio Community 2026 → `C:\Users\clt\Projects\clearpay\ClearPay.slnx`.
2. Çalışan `ClearPay.Web` varsa durdur (MSB3027 kilit / 5153 meşgul olmasın).
3. **F5 / profil: `http`.** `https` profili yalnız `https://localhost:7133` — kanonik adres **değil**. Site: `http://localhost:5153`.
4. Terminal (repo kökü), VS kullanmıyorsan:

```bash
dotnet run --project src/ClearPay.Web --launch-profile http
```

5. Tarayıcı: [http://localhost:5153/giris](http://localhost:5153/giris) — kayıt → giriş → özet. SQL yoksa özet `0,00 ₺` (500 yok). Canlı net için adım 2.

### 2. Docker / VMP (ledger SQL + Oracle)

**Zaten hazır:** `docker-compose.yml` (SQL `:1433`, Redis, Rabbit). Uygulama Redis/Rabbit’e **TASK-12’de** bağlanır. Firmware **VT zaten ON**. Windows’ta Virtual Machine Platform / ilgili sanallaştırma özellikleri **açıldı** (`scripts/docker-vmp-fix.ps1`, `/norestart`).

**Sen tıklarsın (ajan reboot etmez):**

1. **Bilgisayarı yeniden başlat** (VMP/WSL pending reboot). Sonra Docker Desktop’ı **sen** aç.
2. Repo kökü:

```bash
docker compose up -d
docker compose ps
```

3. SQL data: `D:\ClearPay\data\mssql`. Lokal SA `.env.example` (`ClearPay_Dev1!`). `.env` git’e yok. Azure’da bu şifre yok.
4. Oracle XE (`docker-compose.databases.yml`, `:1521`) **reboot + Docker açık** olmadan kalkmaz. Native MySQL84 `:3306` dinliyorsa Compose MySQL’i çakıştırma; veriyi silme.

```bash
docker compose -f docker-compose.databases.yml up -d
```

MySQL `:3306` → `D:\ClearPay\data\mysql`. Oracle → `D:\ClearPay\data\oracle` (`gvenzl/oracle-xe:21-slim`).

### 3. GitHub ve CI

**Zaten hazır:** public repo [HalilMertDeveli/clearpay](https://github.com/HalilMertDeveli/clearpay), dal `main`. Workflow: `.github/workflows/ci.yml` (restore / build / test). Secret yok. Testler `/giris` bekler.

**Sen tıklarsın:** [CI badge / Actions](https://github.com/HalilMertDeveli/clearpay/actions/workflows/ci.yml). Azure deploy job (`azure-deploy.yml`) `AZURE_WEBAPP_NAME` boşsa **atlanır** — bu normal; TASK-16 değil.

### 4. Azure Q1 (şimdi URL yok)

**Zaten hazır:** `infra/main.bicep` (West Europe, App Service Linux F1, Azure SQL Basic), `infra/deploy.ps1`, `azure-deploy.yml`. Kaynak grubu adı: `rg-clearpay-weu`. Ajan hesap açmaz.

**Sen tıklarsın (TASK-16, abonelik sende):**

1. [Azure CLI](https://aka.ms/installazurecliwindows) kur → `az login` (Gmail: `halilmertdeveliii@gmail.com`).
2. Repo kökü: `.\infra\deploy.ps1 -SqlAdminPassword (Read-Host -AsSecureString)`. İsim doluysa `-WebAppName hm-clearpay`.
3. Portal → App Service → **Get publish profile**. GitHub → Settings → Secrets → `AZURE_WEBAPP_PUBLISH_PROFILE`. Variables → `AZURE_WEBAPP_NAME`.
4. Actions: **Azure deploy** veya `main` push. Tarayıcı: `https://<app>.azurewebsites.net/api/health` sonra `/giris`.
5. TASK-16 Done = tarayıcıda **açık URL**. Bugün yok. Connection string / JWT / publish profile **git’e koyma**.

### 5. Q2 kuyruk (uygulama bağlı değil)

**Zaten hazır:** Compose Redis `:6379` + Rabbit yönetim `http://localhost:15672`. `infra/q2.bicep` Azure Cache for Redis C0.

**Sen tıklarsın:**

- Canlı kuyruk: [CloudAMQP](https://www.cloudamqp.com/) hesabını **sen** aç; URL’yi Portal `ConnectionStrings__RabbitMq`. Repo’ya yazma.
- Q2 Redis: `.\infra\deploy.ps1 ... -IncludeQ2` (TASK-16 şartı değil).

### 6. Test — ne yeşil, ne değil

**Zaten hazır:** `dotnet test` (Release). Cookie `LoginPath` `/giris`. Ledger çift kayıt Domain’de.

**İddia etme:** HTTP 409 kanıtı. O uç TASK-06 (`POST /api/transfers` + `Idempotency-Key` → 201 / 409). Test dosyasında `[Fact(Skip=...)]`.

```bash
dotnet test
dotnet build ClearPay.slnx
```

### 7. Yalnız senin işin (ajan yapmaz)

- Azure aboneliği, `az login`, publish profile, App Settings değerleri
- Docker Desktop: **reboot**, sonra Docker’ı aç (VMP). Firmware VT ajan işi değil (zaten ON)
- CloudAMQP, DNS, Search Console, Ads (harcama yok)
- Secret’ı git’e koymamak (`.env`, JWT, SQL SA, Gmail App Password, publish profile)
- Bu Notion sayfasını **Share → Publish to web**

Kod sırası senin işin değil. Komut: «sıradaki işi yap» / «devam». Ürün sırası **TASK-06**.

---

## Ekranlar (SPEC, bugün)

| # | Ekran | Rota | Durum |
|---|--------|------|--------|
| 1 | Giriş | `/giris` | Çalışır |
| 2 | Kayıt | `/kayit` | Çalışır |
| 3 | Özet | `/` | TASK-05: ledger net; SQL yoksa 0,00 ₺ |
| 4 | Havale | `/havale` | Form kabuğu; API TASK-06 |
| 5 | Yükle / Çek | `/yukle-cek` | Form kabuğu; TASK-07/08 |
| 6 | Hareketler | `/hareketler` | Kabuk; TASK-09 |
| 7 | Dekont | — | Yok (TASK-09) |
| 8 | Admin | — | Yok (TASK-10) |

---

## Yasak

Ajan Azure / DNS / CloudAMQP hesabı açmaz. SPEC ekran listesi şişmez. LED teknik destek reposuna dokunulmaz. Bu dosyadaki “canlı URL” cümlesi TASK-16 Done olmadan yazılmaz.
