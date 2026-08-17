# Eşzamanlı çalışma — yapı ve bu repo’nun hali

Tarih: **2026-08-17**. Dal: `cursor/yol-haritasi-career-first`. Kaynak: `TASKS.md`, `TARTISMA.md` T-016/T-017, `ORGANIZASYON.md`, `OTURUM-PLAN.md`. Uydurma canlı URL yok.

«Eşzamanlı» burada üç ayrı şey. Karışınca MSB3027, port 5153, HANDOFF silinmesi, ledger çift kesimi çıkar. Ayrı tutunca masalar aynı anda yazabilir; ürün kuyruğu yine **tek TASK**.

Paylaşım: bu dosya GitHub’da public (push sonrası blob). Notion kopyası varsayılan **private** — herkese açık yapmak senin **Share → Publish**.

---

## Üç katman

### 1. Git — tek repo, paylaşım = push

Repo: [HalilMertDeveli/clearpay](https://github.com/HalilMertDeveli/clearpay). `main` kanonik. İş dalı (bu belge): `cursor/yol-haritasi-career-first`. `main`’e force-push yok.

Aynı anda iki ajan **aynı dosyayı** yeniden yazmaz. OWN glob’u `ORGANIZASYON.md` tablosunda. Çakışınca git merhamet etmez: son commit kazanır, kardeşin satırı kaybolur. Deploy’un HANDOFF’u bir kez sessiz ezmesi bunun kanıtı — kural şimdi **append only**.

WIP senin dalında değilse `git stash`. Kardeş ajanın kirli ağacını «temizlik» diye commit etme. Paylaşılacak şey local disk değil: `git push -u origin <dal>`. Blob URL o push’tan sonra çalışır.

İki Kestrel aynı `:5153`’e bağlanamaz; bu git sorunu değil, aşağıdaki makine katmanı.

### 2. Masalar / ajanlar — tek ürün TASK, paralel OWN

Orchestrator `TASKS.md`’den **bir** Todo alır. «Sıradaki işi yap» ikinci TASK açmaz. Yapı işinde (şema, ekran-akış, port) birden fazla **Architect aynı TASK içinde** öneri üretir (T-016); Orchestrator TARTISMA’da **en robust tek kazananı** kilitler (T-017); Coder yalnız onu yazar. Kaybeden taslak kodlanmaz.

Diğer masalar (Sales, İK, Destek, Designer token, Deploy şablon) **kendi OWN docs**’unu paralel yazar. Razor / PageModel yalnız Coder. Flutter Dart (`mobile/clearpay/**/*.dart`) yine Coder. Payments `src/ClearPay.Domain/Ledger/**`. Tester `tests/**`. `src/` veya OWN değişmeden önce `TARTISMA.md` bloğu: Kim, Konu, Seçenekler, Karar, Neden, Sonra hangi dosya. `HANDOFF.md` tartışma defteri değil; landed / blok / sıradaki satırı **append**.

**Neden iki Payments aynı anda olmaz.** Havale aynı `Idempotency-Key` ile ikinci kez gelince HTTP **409**; debit, credit, Transfer, idempotency, audit, outbox **tek SQL transaction**. İki ajan aynı ledger dosyasını veya aynı tx yolunu yarıştırırsa invariant bozulur: ya last-write-wins (`UPDATE Balance` tuzağı — yasak), ya kısmi commit (biri zengin biri fakir), ya ikinci 200 (çift kesim). Para kuralı sırayla; UI/docs paralel.

Ürün ekran listesi SPEC’te **8**. 9. ekran tartışmasız açılmaz.

### 3. Makine — bu PC’de ne duruyor

| Parça | Ne işe yarar | Sen tıklarsın |
|-------|----------------|---------------|
| Visual Studio Community **2026** | `ClearPay.slnx`, F5, `http` profili | İkinci F5 yokken |
| .NET **8** SDK | `dotnet run` / `dotnet test` | PATH’te |
| Docker Desktop | SQL `:1433`, Redis, Rabbit; MySQL/Oracle ayrı compose | **VMP reboot sonrası** sen açarsın |
| Cursor (bu sohbet + paralel ajanlar) | masalar; Cloud Agents ücretli planda | GitHub bağları |
| GitHub `HalilMertDeveli/clearpay` | push = paylaşım; Actions `ci.yml` | senin hesap |
| Notion | insan kopyası (isteğe bağlı) | Share → Publish |

Lokal kanonik site: [http://localhost:5153/giris](http://localhost:5153/giris). `https` profili (`:7133`) kanonik değil. `launchSettings.json` `http` profili **5153**.

---

## Halil için yapı (bunlar olmadan «eşzamanlı» iddiası boş)

1. **GitHub yazma yetkisi** — `HalilMertDeveli/clearpay`. Ajan push eder; sen PR / `main` birleştirmesini kontrol edersin.
2. **Cursor** — bu sohbet. Paralel / Cloud Agents için ücretli plan gerekir; ücretsiz tek ajan sıraya girer.
3. **Tek Kestrel `:5153`** — VS’de ikinci F5 veya ikinci `dotnet run --launch-profile http` MSB3027 (dll kilit) veya «address already in use». Durdur, sonra bir tane aç.
4. **Docker, reboot’tan sonra** — Virtual Machine Platform `/norestart` ile açıldı; ajan reboot etmez. Extra DB (MSSQL bind `D:\ClearPay\data\mssql`, MySQL/Oracle sidecar) engine ayakta olmadan durmaz. Firmware VT zaten ON.
5. **Notion isteğe bağlı** — GitHub blob zaten public. Notion’da Publish tıklamazsan dışarıdaki kişi giriş ekranı görür.

Azure aboneliği eşzamanlı kod için şart değil. TASK-16 canlı URL için `az login` + `.\infra\deploy.ps1` senin tıkın; ajan `azurewebsites.net` uydurmaz.

---

## Bu repoda paralel ne oldu (dürüst)

Aynı anda **ürün TASK’ı iki tane ilerlemedi**. TASK-01…15 sırayla Done. Paralel olan şey masaların **ayrı OWN** yazmasıydı:

| Masa | Ne landed | Ne değil |
|------|-----------|----------|
| Deploy | `infra/main.bicep`, `infra/q2.bicep`, `infra/deploy.ps1`, Compose Redis/Rabbit şablonu, `.github/workflows/ci.yml` + `azure-deploy.yml` | Açık App Service URL; sen `az login` demedin |
| Coder / Designer | Alipay **düzeni** (T-038): cüzdan kromu, marka kopyası değil | Alipay ürünleri (Yu’e Bao, kredi, süper-app) — `YONETICI-CALISMA.md` **never** |
| Orchestrator | Alipay boşluk listesi (`YONETICI-CALISMA.md`, T-039): 57 özellik → 9 Q1 / 5 Q2 / 43 never | SPEC’e 9. ekran |
| Architect | Onion/Clean derleme; n-tier **isim eşlemesi** aynı dört csproj (T-019). İkinci BLL/DAL ağacı yok | Klasik UI→BLL(EF)→DAL |
| Sales / docs | İngilizce README, çok dil, `OTURUM-PLAN.md` tıklama sırası | «havale #1» / Papara rakibi copy |

TASK-04 örneği (T-016/T-017): üç Architect (şema, ekran-akış, port-DIP) aynı anda önerdi; Orchestrator tek kazananı kilitledi; Coder EF’i ondan sonra yazdı. İki Coder aynı `_Layout.cshtml` açmadı.

Flutter JWT (`mobile/clearpay`) Q2 istemci: aynı SQL, ikinci kasa yok. Coder OWN. TASK-16 Todo durur.

---

## Ürün durumu (`TASKS.md`, 2026-08-17)

**Done:** TASK-01…15 (repo, iskelet, giriş/kayıt, ledger, canlı özet, **havale 409**, yükle/çek REST, SOAP, hareket/dekont, admin, outbox/Hangfire, Redis+Rabbit bind, xUnit, README/Swagger, GitHub Actions). UI cilâları TASK numarası değil: mobil düzen (T-053), demo kart (T-055), kamu cüzdan örnekleri (T-057), Flutter çekmece (T-066).

**Doing:** boş.

**Todo (tek ürün işi):** TASK-16 — Azure App Service + Azure SQL, tarayıcıda HTTPS. Blok **Halil**. Ajan Portal açmaz, URL yazmaz.

HTTP **409** TASK-06 **Done**. Replay aynı `Idempotency-Key` → 409; başarı 201. «409 hâlâ skip» deme — `TASKS.md` Done diyor.

Lokal tık: site ayaktaysa [http://localhost:5153/giris](http://localhost:5153/giris). Demo `admin@clearpay.test` / `Deneme123`. Swagger: [http://localhost:5153/swagger](http://localhost:5153/swagger). Flutter: site açıkken `cd mobile\clearpay` → `flutter run` (emülatör `10.0.2.2:5153`).

---

## Bu makinede zaten yaşanan çatışmalar

Bunlar «ajan kötü» değil; katman 1–3 karışınca çıkan ses.

**MSB3027 / 5153 meşgul (T-031).** VS F5 ile `dotnet run` aynı çıktı dll’sini kilitler; iki süreç aynı `http` profilini ister. Çözüm: çalışan `ClearPay.Web`’i durdur, bir tane bırak. İkinci ajan «ben de F5» demez.

**Docker / VMP reboot (T-037).** Engine kapalıyken native SQL TCP de kapalı görünür. Compose YAML duruyor; container yok. Reboot senin; ajan Windows’u yeniden başlatmaz.

**Notion «neden private?».** MCP sayfa yazar; **Publish to web** API’si yok. Dışarı link = sen Share tıklarsın. GitHub markdown zaten logged-out açılır.

**`ERR_CONNECTION_REFUSED`.** Çoğu zaman eski tarayıcı sekmesi: Kestrel durmuş, SQL değil. `dotnet run --project src/ClearPay.Web --launch-profile http` yoksa 5153 dinlemez. YOL.md aynı notu taşır.

**HANDOFF overwrite.** Erken oturumda Deploy commit Architect bölümünü sildi. Kural: kardeş bölümünü silme. Tartışmayı HANDOFF’a yazma; karar `TARTISMA.md`’de kalır.

---

## Nasıl paylaşılır

1. Bu dal push: `git push -u origin cursor/yol-haritasi-career-first` (ajan yaptıysa tekrar etme).
2. GitHub (logged-out çalışır): [docs/ESZAMANLI.md bu dalda](https://github.com/HalilMertDeveli/clearpay/blob/cursor/yol-haritasi-career-first/docs/ESZAMANLI.md).
3. Notion kopyası (varsayılan private): [ClearPay — eşzamanlı çalışma](https://www.notion.so/3bf31a8b18e481c98a27e0d061c2adc4) — sayfada **Share → Publish → Publish to web**. «Anyone with the link». Arama motoru indeksi kapalı kalabilir.
4. `main`’e merge senin PR’ın; bu belge TASK-16’yı Done saymaz.

Adım adım tıklama (site aç, Docker, Azure talimat) duruyor: [`OTURUM-PLAN.md`](OTURUM-PLAN.md). Kariyer sırası: [`YOL.md`](YOL.md).
