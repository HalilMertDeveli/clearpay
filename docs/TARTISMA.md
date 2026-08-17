# Tartışma — önce konuş, sonra yaz

Ekipler fikirleri **burada** değiştirir. `docs/HANDOFF.md` yalnızca **append** durum defteridir (landed / blok / sıradaki); tartışma üzerine yazılmaz, silinmez, HANDOFF’u overwrite etmez.

**Kural:** `src/` veya bir masanın kaynak dosyası (OWN) değişmeden önce bu dosyaya bir blok eklenir. Bloğu olmayan `src/` / OWN rewrite yok.

---

## Şablon (gelecek maddeler)

Tarih + kısa başlık. Alanlar sabit; madde silinmez, üzerine yazılmaz — yeni blok ile düzelt.

```
## T-NNN — YYYY-MM-DD — kısa konu

- **Kim:** birim / ajan (kim tartıştı)
- **Konu:** ne kararlaştırılacak
- **Seçenekler:**
  1. …
  2. …
- **Karar:** seçilen seçenek (tek cümle)
- **Neden:** neden diğerleri değil
- **Sonra hangi dosya:** kim, hangi OWN / glob (iş bundan sonra)
```

---

## T-001 — 2026-08-13 — LED vs ayrı ClearPay repo

- **Kim:** Yönetim (Orchestrator), Öğrenme
- **Konu:** Cüzdan / ödeme LED teknik destek sitesine mi yazılır, yoksa ayrı repo mu?
- **Seçenekler:**
  1. LED reposuna cüzdan ekranı ve ledger eklemek (aynı makine, tek GitHub).
  2. Ayrı public repo: `HalilMertDeveli/clearpay`, dal `main`.
- **Karar:** **2.** ClearPay kendi reposu. LED’e ödeme / cüzdan / özellik yok.
- **Neden:** LED başka ürün, başka SPEC; ödeme LED kapsamı dışı. CV’de tek ürün anlatılsın. “Aynı makinede” aynı ürün demek değildir.
- **Sonra hangi dosya:** `docs/PLAN.md`, `docs/OGRENME.md`, `docs/KRONIK.md` §1, `.cursor/rules/orchestrator.mdc` (LED’e dokunma). Kod: bu repo `src/`.

---

## T-002 — 2026-08-13 — 409 vs ikinci 200 (idempotency)

- **Kim:** Payments, Architect, Öğrenme
- **Konu:** Aynı `Idempotency-Key` ikinci kez gelince HTTP ne olsun?
- **Seçenekler:**
  1. İkinci **200** (veya 201) + aynı body — “REST’te mevcut kaynağı tekrar ver”.
  2. **409 Conflict** — niyet zaten işlendi; ikinci kesinti yok.
- **Karar:** **2.** Başarı **201**, tekrar **409**. SPEC kilit.
- **Neden:** İkinci 200/201 istemciye “yeni işlem oldu” der; çift tıklama / retry cüzdanı iki kez keser. 409 = aynı niyet, ikinci debit yok. Mülakat cümlesi net. (`payments.mdc` 200+body’yi andı; tercih yine 409.)
- **Sonra hangi dosya:** `docs/SPEC.md` § Para / API; `.cursor/rules/payments.mdc`; HTTP kanıtı TASK-06 (`POST /api/transfers`). Tester 409 testi o task’ta.

---

## T-003 — 2026-08-13 — UPDATE Balance vs çift kayıt

- **Kim:** Payments, Finans, Architect
- **Konu:** Bakiye nasıl durur — kolon mu, defter mi?
- **Seçenekler:**
  1. Öğrenci CRUD: `wallet.Balance -= amount; SaveChanges();` audit’siz UPDATE.
  2. Çift kayıt: her harekette + ve − `LedgerEntry`, aynı `PairId`; bakiye = `LedgerPair.NetOf`. `Wallet`’ta bakiye kolonu yok.
- **Karar:** **2.** `UPDATE Balance` helper yok. İade = ters çift; eski satır silinmez.
- **Neden:** Kolon iz bırakmaz, yarışta last-write-wins, mutabakat anahtarı yok, kısmi yazmada biri zengin biri fakir kalır, freeze kuralı kolona yapışmaz. Tek SQL transaction: debit, credit, Transfer, Idempotency, Audit, Outbox.
- **Sonra hangi dosya:** `src/ClearPay.Domain/Ledger/**` (Payments); `docs/FINANS.md`; `docs/SPEC.md` madde 1/3/5. Coder EF: unique `IdempotencyRecord.Key`, indeks `LedgerEntry(WalletId, CreatedAt)`.

---

## T-004 — 2026-08-13 — Papara rakibi değil / mutabakat wedge

- **Kim:** Satış, Pazarlama, Ürün
- **Konu:** Pazarda neyiz — tüketici cüzdanı mı, başka bir tercih sebebi mi?
- **Seçenekler:**
  1. Papara / Tosla GTM: “onlardan ucuz / alternatif havale”.
  2. Kapalı devre kampüs/yemek cüzdanı (gerçek iş; Q1 ekranı yok).
  3. Developer-first ledger API’yi FAST/kart yerine ödeme yöntemi saymak.
  4. **Mutabakat-öncelikli defter:** her kuruşun +/− satırı ve correlation id sizin defterinizde.
- **Karar:** **4.** Tüketici Papara yerini **almıyoruz**. Lisans / FAST / POS iddiası yok. Demo — sahte banka gateway.
- **Neden:** Papara ile özellik yarışı kaybedilir ve yalandır. Onlar “bakiye güncellendi” der; biz satır satır muteber kılarız. 1 lisans + Ads yasağı; 2 Q1 ekran şişirir; 3 FAST’ın yerine geçmez.
- **Sonra hangi dosya:** `docs/FARK.md`, `docs/SATIS.md`, `.cursor/rules/sales.mdc`. Designer fold: `docs/MARKA.md`. Ads/PR aynı kilit.

---

## T-005 — 2026-08-13 — Azure West Europe + azurewebsites.net vs özel domain

- **Kim:** Deploy, SEO, PR
- **Konu:** Q1 canlı nerede, hangi URL, özel domain şimdi mi?
- **Seçenekler:**
  1. Canada Central (LED sitesi oradaydı — “tutarlılık”).
  2. **West Europe**, App Service Linux + Azure SQL; Q1 URL `https://clearpay.azurewebsites.net` (yedek `clearpay-wallet` / `hm-clearpay`).
  3. Özel domain (`clearpay.app` vb.) **şimdi** satın al, DNS ajan bassın.
- **Karar:** **2 şimdi; 3 sonra.** Bölge West Europe (TR gecikmesi). Özel domain kullanıcı satın alır; ajan registrar/DNS açmaz. TASK-16 abonelik yokken başlamaz.
- **Neden:** LED ile aynı RG şart değil. Ücretsiz/ucuz kanıt URL’si önce; HTTPS App Service adı yeterli. Domain + managed cert ayrı fatura ve hesap. Sıra: TASK-15 Actions → kullanıcı Azure → TASK-16.
- **Sonra hangi dosya:** `docs/CANLI.md`, `docs/DEPLOY.md`. SEO host placeholder aynı URL. Ajan hesap açmaz.

---

## T-006 — 2026-08-13 — Ads / “havale #1” yok

- **Kim:** PR, SEO/Ads, Pazarlama
- **Konu:** Google’da ne için üst sıra vaadi, Ads ne zaman?
- **Seçenekler:**
  1. “Havale” / “Papara alternatif” / “ucuz havale” #1; kampanya localhost’a.
  2. Kazanılabilir kelime: `ClearPay ASP.NET`, `idempotent wallet .NET`, repo, isim+ClearPay. Ads **yalnızca** canlı Azure URL sonrası; başlıkta **Demo**. Harcama yok (hesap kullanıcı).
- **Karar:** **2.** Tüketici “havale” yarışı yok. TASK-16 URL yokken kampanya yok.
- **Neden:** “Havale” banka mobil + Papara + FAST; lisans ve bütçe ister. Demo o pazarı kazanamaz. Hiring manager “ASP.NET cüzdan idempotent” arar. `ucuz havale` yalan ve Ads yasağı.
- **Sonra hangi dosya:** `docs/PR.md`, `docs/ADS.md`, `docs/SEO.md`, `docs/PAZARLAMA.md`. Coder layout meta (`SEO.md`); SC/GA4/Ads hesabı kullanıcı.

---

## T-007 — 2026-08-13 — SOLID portlar, havale API’den önce

- **Kim:** Architect, Payments, Coder
- **Konu:** Havale PageModel / API’de mi hesaplanır, yoksa Application portu mu önce?
- **Seçenekler:**
  1. TASK-06’da Web’de ledger + `new RestBankGateway()`; port sonra.
  2. Portlar **şimdi:** `IWalletReader`, `ITransferExecutor`, `IIdempotencyStore`, `IClock`, `IBankGateway`. Stub `NotImplementedException`. Havale HTTP TASK-06.
- **Karar:** **2.** DIP kapısı para özelliğinden önce. PageModel’de ledger/havale yok.
- **Neden:** Para kuralı UI’de olursa ekran bozulunca defter bozulur; REST/SOAP aynı sözleşmeyi `switch` ile Web’de kopyalar. OCP: gateway strateji. Coder `AddClearPay()` enjekte eder; Architect Program.cs yazmaz.
- **Sonra hangi dosya:** `src/ClearPay.Application/Ports|Wallets|Transfers|Banking`, `src/ClearPay.Infrastructure/{DependencyInjection,Time,Persistence,Banking}`, `docs/ARCHITECTURE.md`. Domain/Razor dokunulmadı. `POST /api/transfers` yok (TASK-06).

---

## T-008 — 2026-08-13 — TASK-03 tek ürün kapısı

- **Kim:** Orchestrator, Ürün, Coder
- **Konu:** Görünen site, ledger, Azure, Ads aynı anda mı?
- **Seçenekler:**
  1. Paralel: Identity + havale API + Azure publish + Ads.
  2. **Tek kapı TASK-03:** giriş, kayıt, boş özet `0,00 ₺`. PageModel’de ledger yok. TASK-04…16 sırada. TASK-16 / Ads harcama yok.
- **Karar:** **2.** Coder TASK-03 = US-01/02/03. Payments Domain durur (havale API yok). Diğer masalar OWN docs yazar, Razor yazmaz.
- **Neden:** Ev → kilit → para motoru. 409/outbox ekran yokken “kanıtladım” olmaz. Azure abonelik yok. SPEC 8 ekran kilitli; 9. ekran yok. Tek seferde tek TASK.
- **Sonra hangi dosya:** `docs/TASKS.md` (öncelik TASK-03), `docs/URUN.md`, `docs/PLAN.md` Faz 1. Coder: `src/ClearPay.Web/**`. Tester: login/register/özet smoke.

---

## T-009 — 2026-08-13 — Identity deposu (TASK-03)

- **Kim:** Yazılım (Coder, Architect), Deploy
- **Konu:** Cookie Identity hangi veritabanında?
- **Seçenekler:**
  1. **SQLite** (`App_Data/identity.db`): login/kayıt Compose SQL olmadan ayağa kalkar; ledger ile karışmaz.
  2. SQL Server şimdi: TASK-03 Compose’a kilitlenir; Identity ile ledger aynı DB erken birleşir.
- **Karar:** **1.** TASK-03 Identity SQLite. Ledger SQL Server TASK-04+ (EF, login yeşil olunca). Canlıda Identity da Azure SQL (`docs/CANLI.md`); SQLite prod değil.
- **Neden:** Kilit (cookie) para motorundan ayrı kanıtlanır. Docker PATH yoksa bile kayıt/giriş çalışır. `App_Data/*.db` commit yok.
- **Sonra hangi dosya:** Coder `src/ClearPay.Web` (`AddClearPayIdentity`, `App_Data`). Payments Domain durur. Deploy Compose SQL-only kalır.

---

## T-010 — 2026-08-13 — Çok dilli README + MIT

- **Kim:** Orchestrator (docs)
- **Konu:** GitHub varsayılan README hangi dil; lisans var mı?
- **Seçenekler:**
  1. Tek `README.md` içinde EN + TR + FR bölümleri.
  2. **`README.md` İngilizce (GitHub varsayılan)** + `README.tr.md` + `README.fr.md`; üstte dil linkleri. Yoksa MIT `LICENSE`.
- **Karar:** **2.** Eşit içerik; sahte ekran görüntüsü yok; Papara rakibi iddiası yok. Build rozeti placeholder (Actions = TASK-15).
- **Neden:** Standard-readme: birden fazla dilde `README.md` İngilizce kalır. Tek dosyada üç dil katlanır. MIT eksikti.
- **Sonra hangi dosya:** `README.md`, `README.tr.md`, `README.fr.md`, `LICENSE`. Razor yok. TASK-14 Swagger ayrı durur.

---

## T-011 — 2026-08-13 — Cüzdan/ödeme sitesi, sahte banka uygulaması değil

- **Kim:** Yönetim, Ürün, Coder, Architect
- **Konu:** ClearPay nedir — WePay benzeri cüzdan/pay mi, yoksa sahte perakende banka mı?
- **Seçenekler:**
  1. Sahte banka uygulaması: şube, hesap açma, IBAN/FAST banka UX, “banka gibi” menü.
  2. **Cüzdan / pay sitesi** (orijinal canvas, WePay hissi): Özet, Havale, Yükle/Çek, Hareketler. Navy. Papara / mobil havale **hissi**. `IBankGateway` yalnızca yükle/çek entegrasyon stub’u (REST sonra SOAP).
- **Karar:** **2.** Lisanslı banka / e-para / PSP değiliz. Sahte BankGateway = yükle/çek mock; ürün “fake bank app” değil.
- **Neden:** Canvas ve SPEC cüzdan. Banka uygulaması 9. ekran ve lisans iddiası açar. Footer “Demo — sahte banka gateway” entegrasyonu anlatır, şube bankasını değil.
- **Sonra hangi dosya:** Product `docs/SPEC.md` / `docs/URUN.md`. Coder TASK-03 UI: navy, Özet/Havale/Yükle-Çek — retail bank layout yok. Architect port `IBankGateway` yükle/çek stub. Sales/SEO “banka uygulaması” demez.

---

## T-012 — 2026-08-13 — Azure Q1 + Redis/Rabbit şablon (hesap yok)

- **Kim:** Deploy, Orchestrator (kullanıcı: Azure her şey + diğer altyapı/veritabanı sistemleri)
- **Konu:** Yalnızca App Service+SQL mü, yoksa SPEC’teki cache/kuyruk da şablonlansın mı?
- **Seçenekler:**
  1. Yalnızca Q1: App Service Linux + Azure SQL. Redis/Rabbit’i TASK-12 ürün koduna bırak.
  2. **TASK-15 CI + Bicep Q1 + lokal Compose SQL/Redis/Rabbit + Q2 Redis Bicep + CloudAMQP talimatı.** Ödeme kodu Redis/Rabbit’e bağlanmaz. AWS/GCP/Kafka/K8s yok.
- **Karar:** **2.** Ajan abonelik/DNS açmaz, secret git’e koymaz. TASK-16 Done yalnızca açık URL. Identity: Development SQLite (T-009), Production Azure SQL. Cookie `LoginPath` `/giris`.
- **Neden:** CANLI/PLAN Q1 Hangfire+SQL; Q2 Redis + CloudAMQP. Compose TASK-12 yığını lokal. İkinci bulut SPEC dışı.
- **Sonra hangi dosya:** `.github/workflows/ci.yml`, `azure-deploy.yml`, `infra/main.bicep`, `infra/q2.bicep`, `infra/deploy.ps1`, `docker-compose.yml`, `docs/CANLI.md`, `docs/DEPLOY.md`, `AddClearPayIdentity` Production `UseSqlServer`.

---

## T-012 — 2026-08-13 — Azure için ajan ne yapar (hesap yok)

- **Kim:** Deploy, Orchestrator (kullanıcı: «azure için gerekli her şey»)
- **Konu:** Abonelik yokken TASK-16 URL mi, yoksa pipeline + şablon + Production Identity mi?
- **Seçenekler:**
  1. Portalda abonelik/RG/App Service ajan açsın (hesap yoksa imkânsız / yasak).
  2. **TASK-15 CI + Bicep + deploy workflow + Production’da Identity Azure SQL.** URL kullanıcı `az login` / Portal sonrası. SQLite prod değil (T-009).
- **Karar:** **2.** Ajan hesap/DNS açmaz, secret git’e koymaz. `dotnet test` kırmızıysa Actions Done yok.
- **Neden:** T-005 West Europe + azurewebsites.net durur. Kullanıcı isteği hazırlığı ister, hesabı vermez. Q1 Redis yok.
- **Sonra hangi dosya:** `.github/workflows/ci.yml`, `.github/workflows/azure-deploy.yml`, `infra/main.bicep`, `infra/deploy.ps1`, `docs/CANLI.md`, `docs/DEPLOY.md`, `AddClearPayIdentity` Production `UseSqlServer`. Cookie `LoginPath` `/giris`.

---

## T-012 — 2026-08-13 — UI kopyası: cüzdan, fake bank app değil

- **Kim:** Ürün, Coder, Designer (ses)
- **Konu:** Görünen metin WePay benzeri cüzdan mı, yoksa “sahte banka uygulaması” mı?
- **Seçenekler:**
  1. Footer/kopya “Demo — sahte banka gateway” / “Banka hesabı” / IBAN çekirdek — ürünü fake bank gibi okutur.
  2. **Cüzdan dili:** menü Özet–Havale–Yükle/Çek–Hareketler (SPEC). Kicker/CTA: Cüzdan, Havale, Yükle/Çek. Asla BankaX, şube, IBAN core. Footer: **Demo — yükleme için sahte gateway** (top-up stub; “bu bir sahte banka” değil).
- **Karar:** **2.** `IBankGateway` kod adı kalır; kullanıcıya banka uygulaması denmez.
- **Neden:** Canvas ve T-011 cüzdan. “Sahte banka” cümlesi entegrasyonu değil ürün kimliğini çalıyor. IBAN/şube 9. ekran ve lisans iddiası açar.
- **Sonra hangi dosya:** Coder `src/ClearPay.Web/Pages/**`, `_Layout` / `_AuthLayout`. Ses: `docs/MARKA.md`, `docs/TASARIM.md` footer. Product: `docs/SPEC.md` ekran 5, `docs/URUN.md` yüzey. Tester: kayıt → `0,00 ₺`.

---

## T-013 — 2026-08-13 — Para kazanma: lisans mı, yazılım mı, kariyer mi

- **Kim:** Satış, Ürün, Orchestrator (araştırma; kullanıcı banka/iş yeri sunumu istedi)
- **Konu:** ClearPay bugün nasıl para getirir — kendi e-para lisansı, B2B yazılım, kapalı devre, yoksa kariyer kapısı mı?
- **Seçenekler:**
  1. Papara/Tosla gibi lisanslı tüketici cüzdanı: TCMB ödeme kuruluşu (40 milyon TL özkaynak, 30 Haz 2026) veya e-para (105 milyon TL). SPEC ekranı ve Ads yasağı bozulur.
  2. **Üç kademe, lisans yok:** (a) kariyer — Intertech/Softtech/Bileşim/TAV/İGA .NET maaşı; (b) lisanslı ÖK’ye white-label defter; (c) 6493 sınırlı ağ kapalı devre (kampüs/yemekhane/OSB) — 50 milyon TL/12 ay üstü TCMB bildirimi. Satıcı paneli Q2; SPEC 8 ekran kilit.
- **Karar:** **2.** Kendi lisansımız yok ve şimdi açılmıyor. Banka/iş yeri sunumu: “FAST/Papara yerini almıyoruz; her kuruş +/− ve correlation id sizin defterinizde.” İlk nakit: mülakat. İlk ticari konuşma: kapalı devre (üniversite BT / fabrika yemekhane), avukat 6493 teyidi sonrası.
- **Neden:** T-004 wedge durur. 40/105 milyon TL özkaynak + MASAK bu repo’nun Q1’i değil. Firisbe/PayKit/Kampüs Kart zaten white-label satıyor; farkımız mutabakat motoru, tam PSP değil. TASK-06/11 bitmeden “satın alın” yalan.
- **Sonra hangi dosya:** `docs/GELIR.md` (OWN). Canvas sunum. `docs/SENIN-ISLERIN.md` işaret. `src/` ve SPEC ekran listesi yok. Sales pitch `SATIS.md` mülakat durur; banka cümlesi GELIR’de.

---

## T-014 — 2026-08-13 — Canlı UI hareketi (kurumsal, parti değil)

- **Kim:** Coder, Designer, Ürün (kullanıcı: canlı animasyon + arayüze ekstra çalışma)
- **Konu:** Site durağan mı kalsın, yoksa WePay hissi için ölçülü CSS hareketi mi?
- **Seçenekler:**
  1. Hareketsiz iskelet; veya Bootstrap / gölge / gradient / emoji “carnival”.
  2. **Evet hareket, kurumsal:** sidebar kayma, buton basışı, kart giriş, bakiye fade, nav active, form focus. `prefers-reduced-motion` kapatır. Sahte şube / BankaX yok.
- **Karar:** **2.** `wwwroot/css/motion.css` layout’tan `brand.css` sonrası. Identity + boş `0,00 ₺` özet durur. Havale API yok.
- **Neden:** Kullanıcı canlı sistem istedi; SPEC navy/Inter ve T-011 cüzdan kilit. Parti animasyonu mülakat demosunu ucuzlatır; reduced-motion erişim.
- **Sonra hangi dosya:** Coder `src/ClearPay.Web/wwwroot/css/motion.css`, `site.css` / `brand.css` cilası, `_Layout` / `_AuthLayout` link, Pages Razor (ledger yok). Tester: build + `:5153`.

---

## T-014 — 2026-08-13 — Q1 Azure + lokal Redis/Rabbit + Q2 şablon (hesap yok)

- **Kim:** Deploy, Orchestrator (kullanıcı: Azure her şey + diğer altyapı/veritabanı sistemleri)
- **Konu:** Yalnızca App Service+SQL mü, yoksa SPEC’teki diğer veri/kuyruk sistemleri de şablonlansın mı?
- **Seçenekler:**
  1. Yalnızca Q1: App Service Linux + Azure SQL. Redis/Rabbit’i TASK-12 ürün koduna bırak.
  2. **Q1 şablon + lokal Compose SQL/Redis/Rabbit + Q2 Bicep Redis + CloudAMQP talimatı.** Ödeme kodu Redis/Rabbit’e bağlanmaz (TASK-12). AWS/GCP/Kafka/K8s yok.
- **Karar:** **2.** Ajan abonelik açmaz, secret git’e koymaz. TASK-16 Done yalnızca açık URL. Identity: Development SQLite (T-009), Production Azure SQL.
- **Neden:** CANLI/PLAN Q1 Hangfire+SQL; Q2 Redis + CloudAMQP. Compose TASK-12 yığını lokal kanıt için. İkinci bulut ve broker SPEC dışı.
- **Sonra hangi dosya:** `.github/workflows/ci.yml`, `azure-deploy.yml`, `infra/main.bicep`, `infra/q2.bicep`, `infra/deploy.ps1`, `docker-compose.yml`, `docs/CANLI.md`, `docs/DEPLOY.md`, `AddClearPayIdentity` Production `UseSqlServer`. Cookie `LoginPath` `/giris`.

---

## T-015 — 2026-08-13 — Sahte banka APP vs sahte GATEWAY

- **Kim:** Ürün, Orchestrator, Designer, Sales (kullanıcı: canvas kilit — WePay gibi; sahte banka uygulaması yapmıyoruz)
- **Konu:** Sahte olan nedir — kullanıcının gördüğü uygulama mı, yoksa yalnızca yükle/çek entegrasyonu mu?
- **Seçenekler:**
  1. **Sahte banka APP:** şube, IBAN çekirdeği, “BankaX” perakende banka UI, core-banking ekranları. Footer “biz sahte bankayız.”
  2. **Sahte GATEWAY only:** ürün = WePay benzeri dijital cüzdan **sitesi** (kayıt/giriş, özet, havale, yükle/çek, hareketler/dekont, admin; sol menü SPEC). Sahte olan yalnızca `BankGateway` (REST+SOAP stub) — yükle/çek timeout/retry stand-in. Kullanıcının gördüğü uygulama banka değil. Gerçek POS/FAST/BOA yok (durur).
- **Karar:** **2. GATEWAY only.** Canvas: Papara / banka mobil **havale hissi**, sahte core-banking app yok. One-liner “demo cüzdan, sahte banka” düşer.
- **Neden:** Kullanıcı: “WePay gibi bir uygulama olacak, sahte bir banka uygulaması yapmıyoruz.” İnsanlar PARA’yı BİZİM sitede gönderir. T-011 cüzdan durur; T-004 wedge ledger UX’tir, “biz fake bank’ız” değil. Domain ledger ve SPEC 8 ekran değişmez. Coder TASK-03 = cüzdan login + boş özet.
- **Sonra hangi dosya:** `docs/SPEC.md` ürün, `docs/URUN.md`, `docs/MARKA.md`, `docs/FARK.md`, `docs/SATIS.md`, `docs/ARCHITECTURE.md` (bir satır), `README.md` / `README.tr.md` / `README.fr.md`. HANDOFF Designer/Sales. Razor banka teması yok; Domain ledger yok.

---

## T-018 — 2026-08-13 — TASARIM tariflerini Razor+CSS’e yaz

- **Kim:** Coder (Designer e73e2df tarifler + motion)
- **Konu:** Tarifler dokümanda mı kalsın, yoksa layout/CSS şimdi mi uygulansın?
- **Seçenekler:**
  1. Yalnızca docs; UI iskelet.
  2. **Uygula:** giriş kartı 420px, özet hero, havale stack-form, hareket tablosu, `--motion: 180ms ease`, reduced-motion. Cüzdan; şube yok. Identity durur.
- **Karar:** **2.** `brand.css` + `motion.css` (opacity fade ≤250ms, lift/scale yok). PageModel’de ledger yok.
- **Neden:** Kullanıcı tariflerin uygulanmasını ve `:5153` canlı siteyi istedi. T-011/T-014/T-015 cüzdan + kurumsal hareket.
- **Sonra hangi dosya:** `src/ClearPay.Web/wwwroot/css/{brand,motion,site}.css`, `_Layout` / `_AuthLayout`, Pages. Tester: build + run.

---

## T-019 — 2026-08-13 — Onion/Clean derleme kuralı + n-tier isim eşlemesi

- **Kim:** Architect, Payments, Coder (kullanıcı: en iyi mimari seçilsin ve kullanılsın — onion veya n-tier; mümkünse birden fazla)
- **Konu:** Kod hangi mimariyi derleme kuralı olarak tutar; n-tier ayrıca nasıl “kullanılır” — ikinci uygulama mı, yoksa aynı projelerin adı mı?
- **Seçenekler:**
  1. Klasik n-tier: UI → BLL → DAL, EF/SQL BLL’de, çift yönlü referans, ayrı `BLL`/`DAL` ağacı. Onion bırakılır.
  2. Yalnızca Onion/Clean; n-tier adı yok sayılır (kullanıcı “birden fazla” demesine rağmen).
  3. İki rakip uygulama / çift Domain (SPEC tek host’u bozar).
  4. **Tek host, iki ad:** derleme kuralı Onion/Clean (Domain merkez, bağımlılık içeri; Application port; Infrastructure adapter; Web composition root). Aynı dört proje n-tier diline eşlenir (Web = sunum, Application = iş, Infrastructure = veri). Hexagonal port/adapter aynı soğanın yüzeyi — üçüncü proje ağacı değil.
- **Karar:** **4.** Ledger + idempotency + 409 için klasik n-tier’a geçilmez. “Birden fazla mimari” = aynı csproj grafiği, iki (üç) kelime; ikinci BLL/DAL yok.
- **Neden:** Klasik n-tier’da BLL EF görür, PageModel ledger hesabı ve `UPDATE Balance` yolu açılır; 409/çift kayıt UI’ye kilitlenir. Onion’da Domain EF/HTTP bilmez; DIP zaten T-007. Kullanıcı her iki adı istedi; iki app SPEC’i kırar. Hexagonal zaten `IWalletReader` / `IBankGateway`.
- **Sonra hangi dosya:** `docs/ARCHITECTURE.md`; `Program.cs` (`SqlOptions` Web’den çıksın); PageModels `IWalletReader` (TASK-03 boş özet adapter, ledger SQL yok); `tests/ClearPay.Tests` katman testi; HANDOFF append. README / `docker-compose.databases.yml` / Azure dokunulmaz. TASK-04…16 Done değil.

---

## T-016 — 2026-08-13 — Paralel Architect (aynı TASK)

- **Kim:** Orchestrator, Architect, Coder (kullanıcı: birden fazla mimar aynı anda kullanılsın; Coder’a söyle)
- **Konu:** Tek Architect mı, yoksa aynı TASK’ta birden fazla Architect paralel mi?
- **Seçenekler:**
  1. **Tek Architect:** yapı işinde bir ajan sırayla şema + ekran-akış + port yazar; Coder bekler.
  2. **Paralel Architect, aynı TASK:** ayrı OWN dilimleri (SQL/şema, ekran-akış SPEC uyumu, port/DIP/gateway) aynı anda öneri üretir. TASKS.md hâlâ **tek TASK**. Coder Razor/şemayı kararlardan **sonra** yazar.
- **Karar:** **2.** Aynı TASK içinde birden fazla Architect paralel çalışır. Architect’ler TARTISMA’da hizalanır; Coder’a **tek** HANDOFF el değiştirme notu düşer. İki Architect aynı dosyayı sessizce ezmez; HANDOFF overwrite yok.
- **Neden:** Şema, ekran-akış ve DIP ayrı OWN; tek ajan sıraya sokunca TASK-04 gecikir. Çok TASK paralel yasak durur. Coder SPEC ekran listesini şişirmez; PageModel’de ledger yok; portlar Application’da. Kazanan seçimi T-017.
- **Sonra hangi dosya:** T-017; `.cursor/rules/orchestrator.mdc`, `architect.mdc`, `coder.mdc`; `docs/AGENTS.md` Architect satırı; `docs/HANDOFF.md` append (Coder). `src/` yok. TASKS.md yok.

---

## T-017 — 2026-08-13 — Paralel öneriden en robust tek seçim

- **Kim:** Orchestrator, Architect, Coder, Payments (kullanıcı: en robust hangisi ise o seçilsin)
- **Konu:** Paralel Architect taslaklarından hangisi koda gider?
- **Seçenekler:**
  1. Coder her taslağı birleştirir / hepsini yazar.
  2. İlk biten taslak otomatik kazanır.
  3. **Paralel üret → Orchestrator TARTISMA’da en robust tek kazananı yazar → Coder yalnızca onu uygular.**
- **Karar:** **3.** Paralel Architect → **en robust tek seçim** → Coder yalnızca onu yazar. Kaybeden öneri kodlanmaz. Orchestrator seçimi TARTISMA’da (yeni T-NNN) yazar; HANDOFF’ta kazanan OWN glob.
- **Neden (robust tanımı):** (1) SPEC 8 ekran + para kuralları: çift kayıt, 409, negatif bakiye yok, freeze, iade=ters kayıt, outbox aynı SQL tx. (2) `UPDATE Balance` yok; ledger audit’siz düzeltilmez. (3) DIP: para kuralı PageModel’de değil; port + Domain. (4) Tek host, Clean Arch, sahte gateway; lisans/FAST/Papara/9. ekran yok. (5) Yarış, kısmi commit, timeout kaybı, HANDOFF overwrite, aynı dosyayı iki ajanın ezmesi yok. (6) Tartışmasız `src/` yok. Eşitlikte: Payments/ledger + idempotency/outbox’ı koruyan > “kolay UI”; şema netliği > erken özellik.
- **Sonra hangi dosya:** `.cursor/rules/orchestrator.mdc`, `architect.mdc`, `coder.mdc`; `docs/AGENTS.md`; `docs/HANDOFF.md` append. Coder TARTISMA kazanan OWN glob. `src/` yok. TASKS sırası TASK-04 (değişmez).

---

## T-020 - 2026-08-13 - Lokal SQL Server + MySQL + Oracle (ayri compose)

- **Kim:** Deploy / Orchestrator (kullanici: uc motor lokal + test; cok-DB uygulama sonra)
- **Konu:** Oracle, SQL Server, MySQL bu PC'de ayaga kalksin; docker-compose.yml Redis/Rabbit ajanina dokunulmasin.
- **Secenekler:**
  1. Ucunu mevcut docker-compose.yml'e ekle (Redis/Rabbit/SQL ajanini ezer).
  2. **Ayri docker-compose.databases.yml:** MySQL :3306 + Oracle XE :1521. SQL Server mevcut compose sql (:1433) ve/veya Windows native instance. Cok-DB uygulama **sonra**; SPEC 8 ekran sabit.
  3. Native Oracle XE Windows kurucusu (agir, OTN hesap).
- **Karar:** **2.** SQL ikinci instance yok. Oracle resmi/kolay imaj: gvenzl/oracle-xe:21-slim. WSL2/Hyper-V ozellikleri acildi; Docker daemon reboot sonrasi.
- **Neden:** Kullanici uc motor + lokal test istedi; urun uygulamasi sonra. Compose carpismasin. Azure hesap / TASK-16 / LED yok. Sifre git'e unique secret olarak yazilmaz; lokal demo .env.example ile ayni.
- **Sonra hangi dosya:** docker-compose.databases.yml, .env.example, scripts/db-smoke.ps1, docs/DEPLOY.md (lokal motor tablosu), docs/HANDOFF.md append. docker-compose.yml / CANLI / Razor dokunulmaz.

---

## T-021 — 2026-08-13 — TASK-04 ledger EF: SQL Server, Identity SQLite kalır

- **Kim:** Yazılım (Coder, Architect, Payments), Yönetim
- **Konu:** TASK-03 bitti. Ledger tabloları nereye, Identity nereye? Havale API şimdi mi?
- **Seçenekler:**
  1. Identity + ledger tek SQL Server şimdi (T-009’u bozar; cookie’yi Compose’a kilitler).
  2. **Ledger EF SQL Server** (`ClearPay` DB): `Wallet`, `LedgerEntry`, `Transfer`, `IdempotencyRecord`, `AuditLog`, `OutboxMessage`. Unique `Wallet.UserId`, unique `IdempotencyRecord.Key`, indeks `LedgerEntry(WalletId, CreatedAt)`. Identity **SQLite** (T-009). **Havale API yok** (TASK-06).
  3. Domain POCO rewrite + yeni bakiye kolonu.
- **Karar:** **2.** Payments Domain POCOs durur (`UPDATE Balance` yok). Coder Infrastructure DbContext + migration. PageModel’de ledger yok. `POST /api/transfers` yok.
- **Neden:** T-009 kilit ayrı, para motoru ayrı. TASK-04 kabul: şema + indeks; çift kayıt Domain’de zaten var. 409 HTTP TASK-06.
- **Sonra hangi dosya:** Coder `src/ClearPay.Infrastructure` (EF, migration), Web `AddDbContext` SQL connection. Domain/Ledger **dokunulmaz**. Tester sonra migration smoke.

---

## T-022 — 2026-08-13 — TASK-04 ekran-akış (SPEC 8, Architect b)

- **Kim:** Architect (ekran-akış OWN), Coder
- **Konu:** Ledger şeması hangi ekranı / kaydı değiştirir?
- **Seçenekler:**
  1. Kayıtta SQL Server’a wallet insert (Compose yoksa kayıt kırılır; T-009 bozulur). Dekont/havale API şimdi.
  2. **Ekran listesi sabit:** 8 SPEC ekranı. Giriş/kayıt/boş özet durur. Havale/Yükle-Çek/Hareketler placeholder. PageModel ledger net yok. Wallet satırı TASK-05 (SQL okuyunca). `POST /api/transfers` yok. 9. ekran / satıcı / POS yok.
  3. Özet şimdi ledger net (TASK-05’i öne çeker).
- **Karar:** **2.** TASK-04 görünür ürün akışını değiştirmez; şema hazır, UI boş özet.
- **Neden:** Robust = SPEC 8 + T-009 cookie kanıtı. Kayıt Identity SQLite’da kalır. Canlı bakiye TASK-05; 409 HTTP TASK-06.
- **Sonra hangi dosya:** Coder Razor **yazmaz** (TASK-04). Payments Domain kuralı. Infrastructure EF.

---

## T-023 — 2026-08-13 — TASK-04 port-DIP (Architect c)

- **Kim:** Architect (port/DIP OWN), Coder, Payments
- **Konu:** DbContext kime enjekte edilir; hangi port şimdi gerçeklenir?
- **Seçenekler:**
  1. `ClearPayDbContext` PageModel’e; özet SQL net şimdi (DIP kırılır, TASK-05).
  2. **`AddClearPay` içinde `ClearPayDbContext` (SQL Server).** `IWalletReader` → `EmptyWalletReader` kalır. `ITransferExecutor` / `IIdempotencyStore` stub. Program.cs’te `SqlOptions` / `UseSqlServer` yok. Havale API yok. Gateway stub. Migrate: SQL yoksa Identity çalışır (test factory migrate kapalı).
  3. `IWalletReader` SQL adapter + `ITransferExecutor` gerçek (TASK-05+06).
- **Karar:** **2.** DIP durur: Web Application port; Infrastructure adapter; Domain EF bilmez.
- **Neden:** T-007 portlar para özelliğinden önce. T-019 Onion. Eşitlikte ledger şema netliği > kolay UI. Outbox tablosu aynı tx tasarımı; worker TASK-11.
- **Sonra hangi dosya:** Coder `ClearPay.Infrastructure/Persistence` + DI. Web Pages dokunulmaz. Application port imzası dokunulmaz.

---

## T-024 — 2026-08-13 — TASK-04 kazanan (T-017, üç Architect)

- **Kim:** Orchestrator (T-016/T-017), Architect a/b/c, Coder, Payments, Tester
- **Konu:** Paralel önerilerden hangisi koda gider?
- **Seçenekler:**
  1. Coder üç taslağı birleştirir / hepsini yazar (T-017 yasak).
  2. Identity+ledger tek DB + kayıtta wallet + PageModel DbContext (kolay UI; T-009/DIP kırılır).
  3. **Kazanan = T-021.2 + T-022.2 + T-023.2:** SQL Server ledger EF (Balance kolonu yok, 1 user=1 wallet unique, indeks PLAN, outbox satırı şimdi / worker sonra); Identity SQLite ayrı; 8 ekran; EmptyWalletReader; stub executor; Domain LedgerPair/CreateRefund/WouldGoNegative durur (rewrite yok; freeze helper / OutboxStatus eklemesi serbest); havale HTTP yok.
- **Karar:** **3.** Coder yalnızca bunu yazar. Kaybeden (tek DB, kayıtta wallet, PageModel EF, SQL özet, transfer API) kodlanmaz.
- **Neden (robust):** SPEC 8; çift kayıt Domain’de; `UPDATE Balance` yok; 409 şema unique Key (HTTP TASK-06); freeze/iade=ters Domain’de; outbox aynı tx insert tasarımı; DIP; tek host + sahte gateway; T-009; HANDOFF overwrite yok. Eşitlikte ledger+idempotency/outbox > kolay UI.
- **Sonra hangi dosya:** Coder `src/ClearPay.Infrastructure/Persistence/**` (DbContext, Fluent, migration), `ServiceCollectionExtensions`, Web Design paketi. Payments Domain ekleme (OutboxStatus, `Wallet.EnsureCanDebit`). Tester `dotnet build`/`test` + model testi. Razor/Havale API yok. `docs/TASKS.md` TASK-04 Done. `docs/HANDOFF.md` append. `docs/ARCHITECTURE.md` TASK-04 satırı.

---

## T-029 - 2026-08-13 - Lokal MSSQL/MySQL/Oracle veri D: bind mount

- **Kim:** Deploy (kullanici: D: bos alana kur; MSSQL + MySQL + Oracle; app ledger SQL Server kalsin)
- **Konu:** Compose SQL data C: named volume'da siser (~20 GB bos). Uc motor D: uzerinde mi, yoksa Docker data-root mu tasinir?
- **Secenekler:**
  1. Docker Desktop data-root'u D: yapmak (sistem geneli; diger projeleri bozar).
  2. **Proje bind mount:** `D:\ClearPay\data\mssql|mysql|oracle`. MSSQL `docker-compose.yml` servis sql. MySQL+Oracle `docker-compose.databases.yml` (T-020 ayri dosya durur). App connection string yalnizca MSSQL. C: named volume sessiz silinmez.
  3. Ucunu tek docker-compose.yml'e birlestir (T-020 reddi; Redis/Rabbit OWN carpismasi).
  4. Web/Identity'yi MySQL veya Oracle'a tasi (SPEC kilit SQL Server; TASK-03 SQLite Identity; TASK-05 yok).
- **Karar:** **2.** Uc motor lokal Compose; veri D: bind. ClearPay Web ledger **yalnizca MSSQL** (:1433). MySQL :3306 ve Oracle :1521 yan servis (ogrenme / lokal test; Q2 Azure DB degil). Identity SQLite durur. EF/SQL Server migration TASK-04 ajaninda kalir.
- **Neden:** C: ~20 GB, D: ~940 GB bos. AutoCAD/ss/sss/Test dolu veya amacsiz; yeni `D:\ClearPay\data` bos kok. data-root tasimak Desktop'u bozar. T-020 ayri compose korunur. Secret `.env` (gitignore); `.env.example` placeholder. Azure/DNS/LED/TASK-05 yok.
- **Sonra hangi dosya:** `docker-compose.yml` (sql bind), `docker-compose.databases.yml` (mysql/oracle bind), `docs/DEPLOY.md`, `.env` (git yok), `docs/HANDOFF.md` append. `src/` ve TASK-04 migration ezilmez.

---

## T-025 — 2026-08-13 — Operasyon kimliği tek Gmail

- **Kim:** Orchestrator, Öğrenme, Deploy
- **Konu:** Azure / GitHub / Search Console / Ads / Gmail MCP hangi hesapta yürür?
- **Seçenekler:**
  1. Ayrı hesaplar (ajan yeni Microsoft/Google hesabı uydurur).
  2. **Tek operasyon:** `halilmertdeveliii@gmail.com` (yazım Gmail; kullanıcı: bütün hesaplar buradan). Ajan hesap açmaz; parola/KEY sormaz; secret git’e koymaz.
- **Karar:** **2.** Operasyon kimliği bu Gmail. Azure/SC/Ads hesabı **açılmaz** (zaten var iddiası). TASK-16 **şimdi değil**. Ads harcama yok. Papara maili yok. LED repo yok.
- **Neden:** Kullanıcı tek kimlik verdi. Yeni hesap uydurmak T-005/T-012 ile çelişir. Abonelik `az` yokken görünmez; uydurulmaz. TASK-04 + D: bind ajanları ezilmez.
- **Sonra hangi dosya:** `docs/SENIN-ISLERIN.md`, `docs/CANLI.md`, `docs/TASKS.md` not, `docs/HANDOFF.md` append. `src/` / compose / ledger yok.

---

## T-026 — 2026-08-13 — CI 12 fail: Location `/giris` vs `/Account/Login`

- **Kim:** Tester / error-fixer (CI run 31701150300), Coder Identity
- **Konu:** Cookie `LoginPath` `/giris`. Testler `Location` içinde `/Account/Login` arıyor. 12 kırmızı. LoginPath’i mi geri alalım, testleri mi hizalayalım?
- **Seçenekler:**
  1. `LoginPath`’i default `/Account/Login` yapmak (TR `/giris` kırılır; T-012 ürün).
  2. **Test `Location` assert = yapılandırılmış path `/giris`.** `/Account/Login` sayfa hâlâ 200 (Razor). TASK-04 EF / Domain / compose **dokunulmaz**.
  3. Her iki string’i OR’la kabul (gevşek; yanlış path kaçırır).
- **Karar:** **2.** Assert `Contain("/giris")`. Uygulama doğru; test eski default’u bekliyordu.
- **Neden:** [Cookie LoginPath](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/cookie) Location’a o path’i yazar. SO: [AllowAutoRedirect=false + Location](https://stackoverflow.com/questions/60019975/how-to-disable-auto-redirect-when-integration-testing-in-asp-net-core-3), [LoginPath PathString](https://stackoverflow.com/questions/39206489/asp-net-core-cookieauthenticationoptions-loginpath-on-different-domain). Reddit’te aynı hata yok. CI Node 20 uyarısı: `checkout@v5` + `setup-dotnet@v5` ([setup-dotnet v5](https://github.com/actions/setup-dotnet/releases/tag/v5.0.0)). Lokal MSB3027: Debug `ClearPay.Web` kilitli → Release test, process öldürme yok (Docker ajanı).
- **Sonra hangi dosya:** Tester `AuthPagesTests` / `AuthOrUiTests` / `PlaceholderPagesTests` (yalnızca Location satırı). Deploy `ci.yml` action pin. Skill `.cursor/skills/clearpay-error-fixer/SKILL.md`. Infrastructure Persistence / Domain / compose **yok**.

---

## T-027 — 2026-08-13 — UI dilleri TR/EN/DE/FR (layout chrome)

- **Kim:** Orchestrator, Architect (ekran-akış), Coder
- **Konu:** Kullanıcı uygulamada İngilizce, Türkçe, Almanca, Fransızca istiyor. SPEC «Çok dilli UI» kapsam dışı ve UI Türkçe kilitliydi.
- **Seçenekler:**
  1. Reddet: SPEC kapsam dışı; UI Türkçe kalır (9. ekran / ürün şişmesi korkusu).
  2. **Kabul:** TR/EN/DE/FR cookie + `.resx` Razor. Dil seçici **layout chrome** (sol menü + üst); 9. ekran yok. Varsayılan Türkçe.
- **Karar:** **2.** README zaten EN/TR/FR (T-010); UI aynı diller + DE. Ads/Papara metni çevrilmez. Demo disclaimer her dilde (lokalize). Para kuralları değişmez (₺, çift kayıt, 409).
- **Neden:** Kullanıcı şimdi istiyor; 9. ekran açılmaz (seçici chrome). Cookie `c=tr|en|de|fr` (`RequestLocalization` + `.AspNetCore.Culture`). Eşitlikte SPEC 8 ekran durur. TASK-04 Domain/EF ezilmez; TASK-05+ başlamaz.
- **Sonra hangi dosya:** Coder `src/ClearPay.Web/**` (localization, `_Layout` / `_AuthLayout`, görünür metinler). `docs/SPEC.md` dar (kapsam dışı satır kalkar; varsayılan TR). Tester `dotnet build` + seçici smoke. `docs/HANDOFF.md` append. `docs/TASKS.md` yeni TASK yok. Payments Domain / Infrastructure Persistence **yok**.

---

## T-028 — 2026-08-13 — TASK-05 özet: ledger net vs EmptyWalletReader

- **Kim:** Architect (port), Payments, Coder
- **Konu:** Cüzdan özeti bakiyeyi nereden okur?
- **Seçenekler:**
  1. `EmptyWalletReader` kalır (hep 0,00 ₺) — TASK-03 kanıtı, canlı özet yok.
  2. `Wallet.Balance` kolonu + UPDATE — SPEC yasak.
  3. **`SqlWalletReader`:** bakiye = `LedgerPair.NetOf`; ay giden/gelen aggregate; son 5 `LedgerEntry`; freeze rozeti. PageModel yalnızca DTO. SQL yoksa (Docker kapalı) sıfır özet, 500 yok. Cüzdan yoksa 1 user = 1 wallet insert (ledger satırı yok).
- **Karar:** **3.** En robust: invariant Domain’de; HTTP/Razor ledger math yok; `UPDATE Balance` yok.
- **Neden:** PLAN TASK-05; T-003/T-007. 1 SQL down kullanıcı deneyimi TASK-04 HANDOFF ile aynı. Havale API yok (TASK-06).
- **Sonra hangi dosya:** `SqlWalletReader`, `AddClearPay` kayıt, `WalletReaderPortTests` + `SqlWalletReaderTests`. `EmptyWalletReader` kaydı kalkar. Razor Index zaten port bağlı.

---

## T-029 — 2026-08-13 — TASK-05 Coder applies T-028 (`SqlWalletReader` live özet; no havale POST)

- **Kim:** Coder / Payments / Tester
- **Konu:** T-028 kararını koda geçirmek.
- **Seçenekler:** 1. EmptyWalletReader bırakmak. 2. **SqlWalletReader + canlı özet.**
- **Karar:** **2.** Havale POST yok.
- **Neden:** T-028.
- **Sonra hangi dosya:** `SqlWalletReader`, testler, TASKS TASK-05 Done.

---

## T-030 — 2026-08-13 — Görsel çok dilli README (TASK-05 gerçeği)

- **Kim:** Orchestrator, Coder (README OWN T-010)
- **Konu:** Kullanıcı, işlemler bittikten sonra daha görsel, yapıyı daha iyi açıklayan, birden fazla dil seçeneği olan README istiyor. Eski README hâlâ EmptyWalletReader / “UI Türkçe” diyor.
- **Seçenekler:**
  1. Tek `README.md` içinde EN+TR+FR+DE (T-010 reddi; GitHub varsayılan katlanır).
  2. **T-010 durur:** `README.md` İngilizce varsayılan; `README.tr.md` + `README.de.md` + `README.fr.md`; üstte dil çubuğu. Mermaid + SVG (`docs/assets/`) + güncel ekran tablosu (TASK-05 canlı özet). TASK-14 Swagger ayrı.
  3. Ekran görüntüsü için tarayıcı otomasyonu / Edge (kullanıcı LinkedIn’de iptal; PC kilidi yok).
- **Karar:** **2.** DE eklenir (UI T-027 ile aynı dört dil). 409 HTTP / Azure URL / havale “çalışır” iddiası yok. Birinci şahıs ses T-010 korunur; diyagram eklenir.
- **Neden:** Standard-readme: varsayılan İngilizce. Kullanıcı görsel + dil seçeneği istedi; T-010 dosya ayrımı bozulmaz. SVG GitHub’da render olur; sahte ekran görüntüsü uydurulmaz.
- **Sonra hangi dosya:** `README.md`, `README.tr.md`, `README.de.md`, `README.fr.md`, `docs/assets/clearpay-layers.svg`. `src/` yok. TASK-14 Done değil. `docs/HANDOFF.md` append.

---

## T-031 — 2026-08-13 — VS F5 vs :5153 (MSB3027 / address in use)

- **Kim:** Tester / error-fixer, Coder (Web launch)
- **Konu:** Kullanıcı VS Error List + site. `dotnet build` Debug: `MSB3027` locked by `ClearPay.Web`. `https` profili `5153` + `7133` bağlıyor; F5 ikinci Kestrel’i kırar.
- **Seçenekler:**
  1. İkinci process’i öldürüp bırakmak (site düşer).
  2. **`http` = yalnızca `:5153`.** `https` = yalnızca `:7133` (5153 paylaşılmaz). Temiz durdur → Debug build → `dotnet run --launch-profile http`.
  3. Portu 5154’e taşımak (kullanıcı / health / HANDOFF kırılır).
- **Karar:** **2.** Site kanonu `http://localhost:5153`. Onion/PageModel değişmez.
- **Neden:** SO [address already in use](https://stackoverflow.com/questions/55143246/unable-to-start-kestrel-system-io-ioexception-failed-to-bind-to-address-http), [MSB3027 lock](https://stackoverflow.com/questions/47977927/error-the-process-cannot-access-the-file-because-it-is-being-used-by-another-pro). VS F5 `https` seçiliyse 5153’ü çalmaz. SetCultureModel artık diskte (CS0234 race bitti).
- **Sonra hangi dosya:** `src/ClearPay.Web/Properties/launchSettings.json`. Persistence / Domain / compose yok.

---

## T-032 — 2026-08-13 — UI canlı görsel yenileme (WePay/Papara hissi, sahte banka değil)

- **Kim:** Designer + Coder (kullanıcı: UI zayıf, daha canlı; dijital cüzdan sitesi)
- **Konu:** Mevcut flat navy/beyaz iskelet (T-014/T-018: gölge yok, gradient yok, kart lift yok) mülakat demosu için soluk. Canlı fintech cüzdan mı, yoksa bar durur mu?
- **Seçenekler:**
  1. TASARIM barı durur: gölge/gradient/lift yok; yalnızca kopya cilası (kullanıcı “zayıf” dedi; yetmez).
  2. **Kurumsal canlı:** navy `#1B2A4A` + teal `#0F766E` / `#14B8A6` + ılık vurgu `#C2782A`. Kart elevation, hero/özet’te hafif gradient + cam (glass), 12px radius, daha keskin tip. Motion 150–250ms (fade-up + 2px hover; bounce/confetti yok). Boş cüzdan: `empty-mark` + mevcut CTA’lar. 8 ekran; menü aynı; footer demo one-liner. Google/Apple butonları varsa silinmez, stillenir.
  3. Carnival / sahte banka: emoji, şube, IBAN-as-product, 9. ekran, Papara mor yağmuru — yasak (T-011/T-015).
- **Karar:** **2.** SPEC ekran listesi durur; görsel cümle “gölge/gradient yok” bu maddeyle gevşer (yalnızca chrome). Identity, ledger, `SqlWalletReader` dokunulmaz.
- **Neden:** Kullanıcı canlı WePay/Papara **cüzdan** istedi, fake retail bank değil. Eşitlikte ledger > UI; burada UI isteği açık. Reduced-motion kapatır. T-014 kurumsal hareket durur, lift yasağı 2px’e açılır.
- **Sonra hangi dosya:** `docs/TASARIM.md`, `.cursor/rules/designer.mdc`, `docs/SPEC.md` (görsel bir satır). Coder/Designer: `wwwroot/css/{site,brand,motion}.css`, `_Layout` / `_AuthLayout`, wallet Razor (Index, Havale, YukleCek, Hareketler). Login/Register markup’a sosyal buton eklenmez; CSS kancaları hazır. `docs/HANDOFF.md` append. Domain / Persistence yok.

---

## T-033 — 2026-08-13 — Oturum planı tek OWN doküman + Notion

- **Kim:** Orchestrator (kullanıcı: planlar kesin doküman; Notion’da adım adım; README’den tıklansın; dışarı açık)
- **Konu:** KRONIK öğrenme, YONETICI-RAPORU yönetici özeti. Tıklama sırası nereye yazılır? Notion MCP publish yok.
- **Seçenekler:**
  1. KRONIK/YONETICI’ye status bölümü (OWN karışır).
  2. **Yeni** `docs/OTURUM-PLAN.md` + yeni Notion sayfası (adım adım). README Docs’ta bir satır. GitHub kopyası public; Notion’da Share → Publish to web kullanıcı tıklar (ajan hesabı/publish API yok).
  3. Yalnız Notion, repo’da yok (GitHub ziyaretçisi göremez).
- **Karar:** **2.** SPEC ekran uydurma yok. Azure URL iddiası yok. `src/` yok.
- **Neden:** Kullanıcı “doküman oluşsun + Notion + README tıklama + dışarı açık” dedi. MCP’te publish aracı yok; public kanıt = bu markdown. Notion aynı metin, paylaşım Halil’de.
- **Sonra hangi dosya:** `docs/OTURUM-PLAN.md` (OWN), `README.md` / `README.tr.md` / `README.fr.md` / `README.de.md` birer Docs satırı, `docs/HANDOFF.md` append. LED yok.

---

## T-035 — 2026-08-13 — Google/Apple Identity (src; docs already landed)

- **Kim:** Coder (Identity OWN)
- **Konu:** `docs/GIRIS-SOSYAL.md` + `SENIN-ISLERIN.md` origin/main’de; src’de OAuth yok.
- **Seçenekler:** 1. Docs only. 2. **AddGoogle/AddApple + giriş/kayıt butonları; secret git’te yok.**
- **Karar:** **2.** E-posta/şifre durur. Secret yoksa buton durur, challenge “yapılandırılmadı”. 9. ekran yok. `GIRIS-SOSYAL.md` rewrite yok.
- **Neden:** Kullanıcı Coder src istedi; OAuth hesabı kullanıcıda.
- **Sonra hangi dosya:** Infrastructure Identity + Web Account + appsettings placeholders + UserSecretsId. Ledger/TASK-06 yok.

---

## T-034 — 2026-08-13 — UI kesin canlı animasyon (kullanıcı: değiştirilsin)

- **Kim:** Coder / Designer (kullanıcı: arayüz kesin değişsin; çok daha iyi; canlı animasyonlu; Cursor eklentisi serbest)
- **Konu:** T-032 token’ları var ama `motion.css` yalnızca 200ms opacity — kullanıcı hâlâ “zayıf / değiştirilsin” diyor. Carnival mı, yoksa fintech canlı mı?
- **Seçenekler:**
  1. T-032’de kal (fade only) — kullanıcı reddetti.
  2. **Canlı cüzdan:** ambient döngü (orb, mesh, pulse, shimmer 6–14s), bakiye count-up ~700ms, kart stagger + 2px lift, canlı rozet + saat. 8 ekran. Navy+teal. `prefers-reduced-motion` hepsini kapatır. Emoji/confetti/şube yok.
  3. 9. ekran / Papara mor yağmuru / Bootstrap — yasak.
- **Karar:** **2.** PageModel ledger math yok. `SqlWalletReader` / Domain dokunulmaz. TASK-06 başlamaz.
- **Neden:** Kullanıcı “kesin başar” dedi; T-032 görsel bar durur, hareket yetersizdi. Ambient süre 250ms bandının dışında (döngü); tıklama hâlâ 150–250ms.
- **Sonra hangi dosya:** `wwwroot/css/{site,brand,motion}.css`, `wwwroot/js/site.js`, `_Layout` / `_AuthLayout`, `Index.cshtml` (+ PageModel tutar attribute), `.resx` LiveBadge. `docs/TASARIM.md` motion satırı. `docs/HANDOFF.md` append.

---

## T-036 — 2026-08-13 — Yeni Notion oturum sayfası (önceki ajan fail)

- **Kim:** Orchestrator (kullanıcı: önceki Notion ajanı connection failed; yeni public sayfa; README tıklama; lokal md)
- **Konu:** T-033 sayfası (`3bb31a8b…`) duruyor; MCP Publish yok; önceki ajan fail. Yeni olgu: VMP özellikleri açıldı, firmware VT zaten ON, **reboot kullanıcıda**; VS F5 = `http` profili (`:5153`); `https` yalnız `:7133`.
- **Seçenekler:**
  1. Eski Notion URL’yi README’de bırakmak (kullanıcı **yeni** sayfa istedi).
  2. **Yeni** Notion sayfası + `docs/OTURUM-PLAN.md` tıklama sırasını VMP/F5 gerçeğiyle yenilemek. README Docs **bir satır** (EN/TR/FR). MCP publish yok → Halil Share → Publish to web.
  3. Yalnız GitHub md, Notion yok (kullanıcı Notion istedi).
- **Karar:** **2.** `src/` yok. Azure `azurewebsites.net` iddiası yok. 409 = TASK-06 skip. Eski sayfa silinmez.
- **Neden:** Kullanıcı “yeni sayfa + herkes linkle + README + lokal kopya + commit/push” dedi. Public kanıt hâlâ GitHub md; Notion logged-out için Halil Publish tıklar.
- **Sonra hangi dosya:** `docs/OTURUM-PLAN.md`, `README.md` / `README.tr.md` / `README.fr.md` Docs satırı, `docs/HANDOFF.md` append. LED yok.

---

## T-037 — 2026-08-13 — Docker engine duruyor; native SQL TCP kapalı

- **Kim:** Deploy (kullanıcı: Docker kısmı hala devam etmiyor, çöz)
- **Konu:** Desktop 4.86 kurulu, process var, `docker.exe` PATH’te yok, `docker info` engine’e takılıyor. WSL2: distro yok; VMP DISM **Enabled** ama **CBS.RebootPending**. Native `MSSQLSERVER` çalışıyor; TCP **Enabled=0** (1433 kapalı); named pipe kapalı; shared memory + Windows auth **çalışıyor**. Compose SQL `:1433` reboot’suz kalkmaz.
- **Seçenekler:**
  1. Ajan reboot etsin (kullanıcı oturumu kesilir).
  2. **Development:** `Server=localhost` + Integrated Security (shared memory; 1433’ü Docker’a bırak). User PATH’e Docker CLI. `scripts/docker-up.ps1` reboot sonrası compose. Compose dosyaları T-029 bind ile durur.
  3. Native SQL’de TCP 1433 aç (Docker SQL ile port çatışır).
- **Karar:** **2.** Reboot ajan yapmaz. MySQL native `:3306` durur; Compose MySQL çakıştırılmaz. Oracle reboot+Docker ister.
- **Neden:** DISM 3010 = reboot şart. Shared memory ledger’ı şimdi açar; 1433 boş kalsın ki reboot sonrası `clearpay-sql` bağlansın.
- **Sonra hangi dosya:** `appsettings.Development.json`, `scripts/docker-up.ps1`, `docs/DEPLOY.md` bir paragraf, `docs/HANDOFF.md` append. `docker-compose.yml` ezilmez.

---

## T-038 — 2026-08-13 — Alipay cüzdan *düzeni* (marka kopyası değil)

- **Kim:** Designer, Coder (kullanıcı: «alipay'in sitesine git ve arayüzü ona benzer yap»)
- **Konu:** ClearPay UI, Alipay tüketici cüzdan evine benzer *yapı* alsın; Alipay olmadığımızı iddia etmeyelim; logo / QR markası / ticari ikon asset yok.
- **Kaynak (yapı, piksel screenshot repo’ya yok):**
  - https://www.alipay.com/ — web: «hesabım var, hızlı giriş»; ortalı form, banka portalı değil
  - https://global.alipay.com/platform/site/ihome — global merchant (ev cüzdanı değil; mavi+beyaz güven paleti)
  - https://miniprogram.alipay.com/docs/miniprogram/design/service-center — ev üstü **dokuz kare ızgara**; Scan/Pay/Collect çekirdek
  - https://chinability.vercel.app/guides/alipay-ui.html — Home: Pay / Scan / Collect / Card; My: Balance + Bills
  - https://oh-my-design.kr/design-systems/alipay — Ant/Alipay token: Daybreak `#1677FF`, beyaz kart, 6–8px radius, `#F5F5F5` zemin (palet **kopyalanmaz**)
  - https://missjaya0817.github.io/project2.html — Alipay dört çekirdek: scan, pay, collect, pocket; marka mavi `#1091E8`
- **Seçenekler:**
  1. Paleti Alipay `#1677FF` yapmak + ızgara (MARKA navy kilitini kırar; «Alipay klonu» okunur).
  2. **Yapı kopyala, palet MARKA:** navy `#1B2A4A` + teal; ev = büyük TL bakiye + 4 işlem (Gönder / Yükle / Çek / Hareketler) mavi bantta; altta örtüşen beyaz kart (ay özeti + son hareketler). Auth: üst navy şerit + ortalı opak beyaz kart (e-posta/şifre). Sidebar SPEC sırası durur.
  3. Piksel-perfect Alipay screenshot / logo SVG (telif + marka; yasak).
- **Karar:** **2.** Ürün adı **ClearPay**. Footer **Demo — yükleme için sahte gateway**. 8 ekran. POS/satıcı/QR markası yok. PageModel’de ledger yok. Docker reboot hikâyesi durur.
- **Neden:** Kullanıcı Alipay *hissi* istedi; MARKA navy kilitli; 1 ve 3 marka/telif. Eşitlikte ledger > UI — bu madde yalnız kompozisyon.
- **Sonra hangi dosya:** Designer `docs/TASARIM.md`, `docs/MARKA.md` (Alipay değiliz), `wwwroot/css/brand.css`. Coder Razor: `_Layout`, `_AuthLayout`, `Index.cshtml`, Login/Register iskelet; `site.css` iskelet; `motion.css` ızgara. `HANDOFF.md` append. Domain / SqlWalletReader yok.

---

## T-039 — 2026-08-13 — Alipay özellik envanteri SPEC ekranı açmaz

- **Kim:** Yönetici / Orchestrator-doc (kullanıcı: Alipay’deki bütün özelliklere bak, yönetici çalışma listesi yapsın)
- **Konu:** Alipay tüketici + web ürünleri tarandı. ClearPay SPEC 8 ekran genişler mi? Layout benzerliği (T-038) ürün kopyası mı?
- **Seçenekler:**
  1. Alipay’de olan her şeyi (Yu’e Bao, Huabei, mini program, QR POS, Antom kasa…) Q1 ekran/TASK yapmak.
  2. **Envanter + boşluk listesi; SPEC durur.** Q1 = mevcut 8 ekran / TASKS. Q2 = satıcı + onaylı eklenti. never = lisans / POS / gerçek banka / 9. ekran / Papara-Alipay rakip GTM.
  3. “Alipay rakibiyiz” pazarlama metni (`FARK.md` / Ads).
- **Karar:** **2.** Layout benzerliği ≠ Alipay ürünlerini kopyalamak. T-038 Coder Razor OWN; bu madde `src/` yok. 409 skip = TASK-06; Azure URL yok.
- **Neden:** 1 lisans yalanı ve 9. ekran. 3 T-004’ü bozar. Demo cüzdan boşluk listesi, süper-app yarışı değil.
- **Sonra hangi dosya:** `docs/YONETICI-CALISMA.md` (yeni), `docs/YONETICI-RAPORU.md` bir satır, `docs/HANDOFF.md` append. README dokunulmadı (uzman ton). `brand.css` / `_Layout` yok.

---

## T-040 — 2026-08-14 — UI sıkılaştırma + CSS motion (kütüphane yok)

- **Kim:** Designer + Coder (kullanıcı: arayüzü beğenmedi; arayüz geliştirilsin **ve** animasyon eklensin; gölge/gradient yok)
- **Konu:** Mevcut 8 ekran kurumsal okunaklı olsun. T-032/T-034 chrome (elevation, hero gradient, ambient orb/shimmer/pulse) kullanıcı kilidini bozuyor. Bootstrap mı, yoksa mevcut CSS + kısa motion mı?
- **Seçenekler:**
  1. Bootstrap CDN / UI kit — hayır (SPEC Bootstrap yok; CDN ağır).
  2. **Mevcut CSS’i hiyerarşi / ritim / tipografi ile sıkılaştır** + `motion.css` 150–250ms opacity/translate; buton/menü/dil color-border transition; özet kahramanı / stat / tablo hover **gölgesiz**. `prefers-reduced-motion: reduce` kapatır. Sonsuz flashy loop / emoji / rainbow yok. 8 ekran. Dil TR/EN/DE/FR durur.
  3. T-034 ambient + gradient/gölge durur — bu turdaki kilit: yok.
- **Karar:** **2.** PageModel / ledger / `SqlWalletReader` / EF yok. TASK-06 başlamaz. npm / GSAP / Framer yok. T-038 **yapı** (navy bant + 4 işlem + örtüşen tabaka) durur; chrome düz navy + 1px çizgi.
- **Neden:** Kullanıcı “mevcut CSS sıkılaştır” + “animasyonu CSS/layout’tan ekle, kütüphane yok, gradient olmasın” dedi. 1 Bootstrap. 3 yeni kilit ile çelişir.
- **Sonra hangi dosya:** Coder `wwwroot/css/{site,brand,motion}.css`, gerekirse `wwwroot/js/site.js`, `_Layout` / `_AuthLayout`, Index/Havale/YukleCek/Hareketler sınıf. Designer `docs/TASARIM.md` görsel bar. `docs/HANDOFF.md` append. `resx` ezilmez. Domain yok.

---

## T-041 — 2026-08-14 — Redis özet cache (TASK-12 kısmi; kasa SQL)

- **Kim:** Architect + Coder (kullanıcı: add redis; para SQL’de kalsın; MySQL giriş / Azure yedek / canlı Azure / Rabbit **şimdi değil**)
- **Konu:** Compose Redis (`localhost:6379`) var; uygulama bağlı değil. Özet bakiyesi cache mi, Redis kasa mı, yoksa TASK-06 bitene kadar dokunmama mı?
- **Seçenekler:**
  1. TASK-12’yi atlama; önce TASK-06 havale/409 — ürün sırası durur; kullanıcı bu turda Redis istedi.
  2. **`IWalletSummaryCache` + `CachedWalletReader` dekoratör.** Kaynak ledger SQL (`SqlWalletReader` / `LedgerPair.NetOf`). Redis yok/düşer → SQL. `WalletId == Guid.Empty` (SQL yok) cache’lenmez. Identity SQLite durur. Havale invalidate portu hazır; `POST /api/transfers` yok. Rabbit / MySQL Identity / Azure hesap **yok**.
  3. Bakiyeyi Redis’te tut / MySQL giriş / Azure’u yedek kasa yap — hayır (SPEC kasa = SQL Server lokal, Azure SQL canlı; cache ≠ kasa).
  4. TASK-12 tam (Redis + Rabbit) — Rabbit bu turda değil; kullanıcı sonra hatırlatılacak.
- **Karar:** **2.** TASK-12 Todo kalır (Rabbit). TASK-06 sıradaki ürün işi durur. `UPDATE Balance` yok. PageModel Redis/ledger yok. Azure Redis hesabı açılmaz.
- **Neden:** Kullanıcı Redis bağını daralttı. 3 güvenlik/sağlık ve SPEC’e aykırı. 4 kapsam şişirir. 1 kullanıcı isteğini ezer.
- **Sonra hangi dosya:** Coder `src/ClearPay.Application/Ports/IWalletSummaryCache.cs`; Infrastructure `Caching/*`, `ServiceCollectionExtensions`, StackExchange.Redis; `Program.cs` health `redis`; Tester `CachedWalletReaderTests` + WebFactory Redis boş. `docs/HANDOFF.md` append. `docs/TASKS.md` not (Done değil). `docs/ARCHITECTURE.md` cache satırı. Compose servisi ezilmez. Domain / Razor yok.

---

## T-042 — 2026-08-14 — TASK-06 Havale (üç Architect; en robust)

- **Kim:** Architect a (şema), b (ekran-akış), c (port/DIP); Orchestrator kilit; Payments+Coder uygular
- **Konu:** Ekran 4 + `POST /api/transfers` + `Idempotency-Key` 201/409. Şema? Razor vs API? Executor / 409 / cache invalidate?
- **Seçenekler:**
  1. Yeni tablo/migration + treasury cüzdan şimdi + cookie ile API — hayır (şema TASK-04 tam; treasury TASK-07; ARCHITECTURE cookie≠JWT).
  2. Yalnız Razor POST, HTTP 409 yok — SPEC API ve mülakat kanıtı düşer.
  3. **Mevcut şema; `TransferResultKind`; çift giriş tek motor; unique Key 409 otoritesi; test ledger SQLite double; alıcı e-posta; Redis invalidate commit sonrası iki userId.**
- **Karar:** **3.** Kazanan (a+b+c birleşik):
  - Migration yok. `MoneyTransaction.RequiredInserts` tek SQL tx. Treasury yok (TASK-07).
  - `TransferOutcome.Kind`: Created / Replay / KeyPayloadMismatch / InsufficientFunds / FrozenSender / SelfTransfer / RecipientNotFound / InvalidAmount / MissingKey. Created→201; Replay|mismatch→409; diğer 4xx; 4xx’te insert yok.
  - Razor `[Authorize]` form POST → `ITransferExecutor` (cookie + anti-forgery). GET hidden `IdempotencyKey` = sunucu Guid. `POST /api/transfers` **JWT** + header `Idempotency-Key`. Sayfa fetch/JWT taşımaz. Swagger TASK-14.
  - `SqlTransferExecutor` + ayrı `IIdempotencyStore` (ISP). `IUserDirectory` e-posta→userId (PageModel Identity/ledger yok). `FindAsync` hızlı yol; **otorite** unique `UX_IdempotencyRecord_Key` (`SaveChanges` 2627/2601 veya SQLite 19) → 409. Serializable tx; negatif yok. Commit sonrası `InvalidateAsync(actor)` ve `InvalidateAsync(recipient)`. 4xx/rollback invalidate yok.
  - CI 409: `ClearPay:UseSqliteLedger=true` + `EnsureCreated` (kasa üretimde SQL Server; MySQL/Oracle yok). Skip kalkar.
- **Neden:** T-002 409, T-003 çift kayıt, T-007 DIP, T-041 cache. Unique index TOCTOU’suz. EF InMemory unique tutmaz. Cookie-API ARCHITECTURE’ı bozar. 9. ekran yok.
- **Sonra hangi dosya:** Payments `SqlTransferExecutor`, `SqlIdempotencyStore`, `IUserDirectory`/`IdentityUserDirectory`, `TransferResultKind`. Coder `Havale.cshtml(.cs)`, `TransfersController`, `TokenController`, JWT, `SharedResource*.resx`. Tester `TransferExecutorTests` + `TransferApiTests` (409). `NotImplemented*` silinir. Domain POCO/migration yok. `docs/HANDOFF.md` append. `docs/TASKS.md` TASK-06 Done.

---

## T-043 — 2026-08-14 — TASK-07 Yükle/çek REST gateway

- **Kim:** Orchestrator + Payments + Coder (Architect a/b/c TASK-06 şeması durur)
- **Konu:** Ekran 5 + sahte REST BankGateway. Timeout’ta ledger yok, outbox kalır. Treasury?
- **Seçenekler:**
  1. Müşteri cüzdanına tek satır credit / `UPDATE Balance` — yasak.
  2. **Clearing cüzdan `UserId=clearpay-treasury`:** yükle = treasury − / müşteri +; çek = müşteri − / treasury +. Treasury `WouldGoNegative` muaf (dış para); müşteri negatif yok. Migration yok.
  3. Treasury’yi TASK-06 Transfer tablosuna yaz — hayır (P2P değil).
- **Karar:** **2.** `IFundingExecutor` + `SqlFundingExecutor`. `RestBankGateway`: hesap ipucunda `TIMEOUT` veya `BankGateway:SimulateTimeout` → `TimedOut`. Başarı: LedgerPair (`TopUp`/`Withdraw`) + Idempotency + Audit + Outbox **aynı tx**; Transfer satırı yok. Timeout: **ledger yok**; `OutboxMessage` Pending + Idempotency (aynı key replay timeout). Freeze: çekemez, yükleme (credit) olabilir. Cache invalidate başarıda. SOAP TASK-08. PageModel gateway/ledger yok. Sahte gateway; FAST/Papara yok.
- **Neden:** Çift kayıt kilitli. Timeout SPEC madde 4. Unique key ikinci yüklemeyi çiftlemez. 9. ekran yok.
- **Sonra hangi dosya:** Payments `RestBankGateway`, `SqlFundingExecutor`, `IFundingExecutor`. Coder `YukleCek.cshtml(.cs)`, resx. Tester `FundingExecutorTests`. `docs/HANDOFF.md` append. `docs/TASKS.md` TASK-07 Done.

---

## T-044 — 2026-08-14 — TASK-08 SOAP aynı IBankGateway

- **Kim:** Orchestrator + Coder (OCP; Payments sözleşmesi durur)
- **Konu:** SOAP strategy REST ile aynı sonuç modeli. Yeni ekran? Ayrı executor?
- **Seçenekler:**
  1. SOAP için ikinci executor / ikinci ekran — 9. ekran, DIP şişer.
  2. **`SoapBankGateway` aynı `IBankGateway`.** Timeout/`FAIL`/başarı REST ile aynı. `BankGateway:Strategy=SOAP|REST` (varsayılan REST). `SqlFundingExecutor` Web `switch` yazmaz.
- **Karar:** **2.** Reference öneki `SOAP-`. PageModel/ledger yok. Sahte stub; gerçek SOAP stack/WCF yok.
- **Neden:** PLAN OCP/LSP. T-043 timeout kuralı durur.
- **Sonra hangi dosya:** Coder `SoapBankGateway`, `AddClearPay` strategy bind. Tester `BankGatewayStrategyTests`. `docs/HANDOFF.md` append. TASK-08 Done.

---

## T-045 — 2026-08-14 — TASK-09 Hareketler + dekont

- **Kim:** Orchestrator + Coder (Payments şema durur)
- **Konu:** Ekran 6–7 filtre/sayfa + dekont correlation id. Dapper vs EF? Yeni tablo?
- **Seçenekler:**
  1. Yeni hareket tablosu / 9. ekran — hayır.
  2. **`IActivityReader` EF:** kullanıcının `LedgerEntry` + çiftin karşı `Wallet.UserId`. Filtre tarih/tür. Sayfa 20. Dekont `/dekont/{correlationId}` — taraflar, tutar, correlation id, zaman. Yalnız kendi cüzdanının id’si. Treasury etiketi `clearpay-treasury`. PageModel ledger net yok.
- **Karar:** **2.** Dapper şart değil (PLAN “veya SP”); EF mevcut bağlam. Status Completed (timeout ledger’da yok). `IUserDirectory.FindEmailByUserIdAsync` isteğe bağlı etiket.
- **Neden:** SPEC ekran 6–7. Correlation id mülakat. Migration yok.
- **Sonra hangi dosya:** Coder `IActivityReader`, `SqlActivityReader`, `Hareketler.cshtml(.cs)`, `Dekont.cshtml(.cs)`. Tester `ActivityReaderTests`. HANDOFF append. TASK-09 Done.

---

## T-046 — 2026-08-14 — TASK-10 Admin freeze/kuyruk/audit

- **Kim:** Orchestrator + Coder
- **Konu:** Ekran 8 Admin. Freeze nasıl? Seed admin?
- **Seçenekler:**
  1. 9. ekran / PageModel’de ledger — hayır.
  2. **`IAdminPanel` port.** Freeze = `Wallet.IsFrozen` + AuditLog (bakiye UPDATE yok). Başarısız kuyruk = `OutboxMessage.Failed`; “kuyruğa al” → Pending. Audit ara: actor / correlation id / tarih. Rol `Admin`. Dev seed `admin@clearpay.test` / `Deneme123` (Production seed yok). Menü role gizli.
- **Karar:** **2.**
- **Neden:** SPEC ekran 8. Freeze kuralı Domain’de durur.
- **Sonra hangi dosya:** Coder `IAdminPanel`, `SqlAdminPanel`, `Admin.cshtml(.cs)`, `_Layout` Admin link, `IdentitySeeder`. Tester `AdminPanelTests`. HANDOFF. TASK-10 Done.

---

## T-047 — 2026-08-14 — TASK-11 Outbox + Hangfire

- **Kim:** Payments + Coder
- **Konu:** Pending outbox worker. SQL Hangfire vs memory? Dashboard 9. ekran mı?
- **Seçenekler:**
  1. HTTP sonrası kuyruğa elde bas — timeout kaybettirir.
  2. **Hangfire in-process.** `IOutboxProcessor` Pending → publisher → Sent/Failed. Ledger zaten commit. Dashboard yok (8 ekran). Test/SQLite: `Hangfire:Enabled=false` + MemoryStorage; canlı SQL: Hangfire.SqlServer aynı ClearPay bağlantısı. Publisher TASK-11 log no-op; Rabbit TASK-12.
- **Karar:** **2.**
- **Neden:** SPEC outbox. Mesaj DB’de bekler.
- **Sonra hangi dosya:** Payments `SqlOutboxProcessor`. Coder Hangfire DI. Tester `OutboxProcessorTests`. HANDOFF. TASK-11 Done.

---

## T-048 — 2026-08-14 — TASK-12 RabbitMQ bind (Redis landed)

- **Kim:** Coder (T-041 Redis durur)
- **Konu:** Compose Rabbit var; uygulama bağlı değil. Kasa SQL mi?
- **Seçenekler:**
  1. Rabbit’i kasa yapmak / Azure CloudAMQP hesabı açmak — hayır.
  2. **`IOutboxPublisher` → Rabbit queue `clearpay.outbox` when `ConnectionStrings:RabbitMq` var.** Yok/düşer → log publisher (Hangfire yedek). Redis özet cache T-041 durur. Canlı hesap açılmaz.
- **Karar:** **2.** Health `rabbit`: up/down/off. PageModel yok. Ledger SQL.
- **Neden:** PLAN lokal Compose bind; canlı Q2.
- **Sonra hangi dosya:** Coder `RabbitOutboxPublisher`, DI, health. Tester factory Rabbit boş = off. HANDOFF. TASK-12 Done.

---

## T-049 — 2026-08-14 — TASK-13 test sertleştirme

- **Kim:** Tester
- **Konu:** 409 kanıtı var; PLAN ledger invariant / freeze API / yetersiz bakiye HTTP eksik mi?
- **Seçenekler:**
  1. Yeni ekran veya ledger rewrite — hayır.
  2. **Mevcut executor’a HTTP kanıt ekle.** 409 replay + aynı key farklı payload 409 (ikinci kesinti yok). Freeze → 403. Idempotency-Key yok → 400. Ledger: çift kayıt toplamı 0; `Wallet`’ta `Balance` kolonu yok. PageModel/ledger kuralı durur.
- **Karar:** **2.** Ürün davranışı değişmez; boşluk varsa yalnızca test.
- **Neden:** PLAN TASK-13. 8 ekran. `UPDATE Balance` yok.
- **Sonra hangi dosya:** Tester `TransferApiTests`, `LedgerInvariantTests`. TASKS/HANDOFF. `src/` yok unless a real hole.

---

## T-050 — 2026-08-14 — TASK-14 README + Swagger + CV

- **Kim:** Coder
- **Konu:** SPEC CV üçlüsü + OpenAPI. Ads? 9. ekran?
- **Seçenekler:**
  1. Ads harcama / Papara GTM / 9. ekran Swagger-only UI — hayır.
  2. **Swashbuckle `/swagger`.** `POST /api/transfers` 201/409 + `Idempotency-Key` header. JWT bearer. README EN (ve TR/DE/FR eş) güncel ekran tablosu + PLAN’daki 3 mülakat cümlesi. Canlı URL uydurma yok.
- **Karar:** **2.** Razor ekran listesi değişmez. Secret yok.
- **Neden:** SPEC başarı 5; PLAN TASK-14.
- **Sonra hangi dosya:** Coder Web Swagger + `README*.md`. Tester swagger.json 409. HANDOFF. TASK-14 Done.

---

## T-051 — 2026-08-14 — TASK-16 Azure talimat (URL yok)

- **Kim:** Deploy
- **Konu:** Canlı URL ajan mı açar? Hangfire App Setting adı?
- **Seçenekler:**
  1. Ajan `az login` / abonelik / DNS / canlı URL uydurur — hayır.
  2. **Infra + tıklama listesi.** Bicep/App Setting `Hangfire__Enabled=true` (eski `Hangfire__WorkerEnabled` kodda yok). Production Identity = Azure SQL (`ConnectionStrings:ClearPay`). Redis/Rabbit Q2 sen yapıştırırsın. TASK-16 Done = tarayıcıda URL; şimdi blok Halil.
- **Karar:** **2.** Secret git’e yok. 8 ekran.
- **Neden:** PLAN TASK-16; SPEC Azure açık URL kullanıcı işi.
- **Sonra hangi dosya:** Deploy `infra/main.bicep`, `docs/CANLI.md`, `docs/DEPLOY.md`, Production Hangfire. HANDOFF tıklama. TASK-16 Todo kalır (URL).

---

## T-052 — 2026-08-14 — Havale asılı kalmasın (Hangfire + Redis + SQL)

- **Kim:** Payments + Coder (Orchestrator bugfix; 8 ekran değişmez)
- **Konu:** `/havale` çalışmıyor veya uzun sürüyor. İlk `dotnet run` Hangfire `JobStorage` ile düştü; site `Hangfire__Enabled=false` ile ayağa kalktı. Redis/Rabbit Docker kapalıyken transfer asılı kalabilir.
- **Seçenekler:**
  1. Havale’yi Hangfire/Redis/Rabbit’e bağlamak — hayır; kasa SQL, outbox aynı tx Pending kalır.
  2. **Degrade + kısa timeout.** Hangfire: `IRecurringJobManager` (statik `RecurringJob` yok — JobStorage.Current boot’ta yok). Redis down → cache atla, SQL devam (T-041). SQL unreachable → kullanıcıya net hata, asılı kalma yok (`Connect Timeout`/`CommandTimeout` birkaç saniye). Ledger `UPDATE Balance` yok. PageModel ledger yok.
- **Karar:** **2.** Transfer Redis/Rabbit/Hangfire düşse de SQL’e karşı biter veya hızlı fail eder.
- **Neden:** Hangfire 1.8 statik `RecurringJob` `JobStorage.Current` ister; DI `IRecurringJobManager` boot’u kırmaz. SE.Redis `AbortOnConnectFail=false` iken GET/SET/DEL AsyncTimeout kadar bekler (havale POST ~20s). SPEC 409/çift kayıt durur.
- **Sonra hangi dosya:** Payments `SqlTransferExecutor`, Redis cache/factory, Hangfire DI. Coder `Havale.cshtml.cs` + resx + API 503. Tester transfer/409. HANDOFF append. Razor markup yok.

---

## T-053 — 2026-08-14 — Mobil bankacılık düzeni (T-040 derinlik maddesi güncellenir)

- **Kim:** Designer + Coder (kullanıcı: “Papara gibi sanal bankacılık uygulaması, gerçekçi arayüz”; seçim: mobil-önce düzen + derinlik serbest)
- **Konu:** Bugünkü kabuk masaüstü paneli (sol menü + geniş 920px kolon). Mobil bankacılık hissi için alt sekme çubuğu ve kart yığını gerekir. T-040 “gölge/gradient yok” dediği için gerçekçi kart yükseltmesi bloke.
- **Seçenekler:**
  1. Masaüstü panosu kalsın, yalnız tipografi rötuşu — kullanıcı “gerçekçi arayüz” dedi; hiss değişmez.
  2. Masaüstünde de telefon çerçevesi (mockup içinde uygulama) — gerçek ürün değil, demo maketi gibi durur; 4 dil + admin geniş tablosu sığmaz.
  3. **Mobil-önce kabuk:** `max-width: 800px` altında alt sekme çubuğu (`.tabbar`, mevcut rotalar), üstte ince topbar, kart yığını; masaüstünde sol menü **durur** ve içerik ortalanmış dar kolon (`560px`) olur, Hareketler/Admin geniş kalır. Derinlik: yumuşak gölge ölçeği (`--elev-1..3`) ve **yalnız bakiye kartında** navy gradient (`--navy` → `--navy-mid`). Motion 150–250ms sınırı durur.
  4. Papara markası birebir (mor kimlik, logo) — hayır: marka hakkı + MARKA.md “Papara rakibi/alternatifi değiliz”.
- **Karar:** **3.** T-040’ın “gölge / gradient yok” maddesi bu blokla güncellenir; T-040’ın geri kalanı (Bootstrap yok, npm/GSAP yok, emoji yok, 150–250ms, `prefers-reduced-motion`) aynen durur. T-038 yapı (navy bant + 4 işlem + örtüşen tabaka) korunur. SPEC 8 ekran, rotalar, footer **Demo — yükleme için sahte gateway**, wordmark **ClearPay** değişmez. Ledger / PageModel / resx anahtar şeması dokunulmaz — alt çubuk mevcut `NavOverview / NavTransfer / NavTopUpWithdraw / NavActivity / NavAdmin` anahtarlarını yeniden kullanır.
- **Neden:** Kullanıcı bu turda “gerçekçi” istedi ve derinlik gevşetmesini açıkça seçti; 1 hissi değiştirmez, 2 ürünü maket gibi gösterir, 4 hem yasak hem yanlış iddia. Gölge kart hiyerarşisi için; gradient tek yerde kalırsa T-032/T-034’teki karnaval geri gelmez. Şube / BankaX / IBAN çekirdeği yine yok.
- **Sonra hangi dosya:** Designer `wwwroot/css/brand.css` (elevation / gradient / radius / tabbar token), `docs/TASARIM.md`, `docs/MARKA.md`. Coder `Pages/Shared/_Layout.cshtml` (+`_AuthLayout`), `wwwroot/css/site.css`, `wwwroot/css/motion.css`, `Pages/{Index,Havale,YukleCek,Hareketler,Dekont,Admin}.cshtml`, `Pages/Account/{Login,Register}.cshtml`. `docs/TASKS.md` satır, `docs/HANDOFF.md` append. Domain / Application / Infrastructure / migration yok.

---

## T-054 — 2026-08-14 — Giriş hero + vendored anime.js (Yapı Kredi hissi, marka kopyası değil)

- **Kim:** Designer + Coder (kullanıcı: anime.js; giriş sayfası yapikredi.com.tr gibi olsun; git push)
- **Konu:** Auth bugün tek kolon kart. Kullanıcı banka ana sayfası hissi (sol hero + sağ giriş) ve anime.js istedi. T-040/T-053 “npm / GSAP / Framer yok” ve motion 150–250ms. CDN mi, npm mi, vendor dosya mı?
- **Seçenekler:**
  1. npm + GSAP/Framer — hayır; T-040 paket yöneticisi ve karnaval yasak.
  2. CDN’den anime.js — hayır; gizlilik/CI/offline; üçüncü parti script runtime’da.
  3. **Vendor `wwwroot/js/vendor/anime.min.js` (anime.js 3.2.2, MIT) + `auth-hero.js` yalnız `_AuthLayout`.** Split sahne: sol navy hero (geometrik orb, ClearPay navy/teal), sağ mevcut kart. Yapı Kredi **düzeni** (hero + sağ panel); logo, mavi kimlik, Worldcard, fotoğraf **yok**. `prefers-reduced-motion: reduce` → JS no-op, içerik görünür kalır. Giriş animasyonu tek sefer: 180–240ms stagger (toplam ~600ms); sonsuz loop / pulse / bounce yok. 8 ekran; Register aynı layout.
  4. Yalnız CSS — kullanıcı anime.js istedi.
- **Karar:** **3.** T-040 “npm/GSAP yok” maddesi **yalnız auth layout + vendored anime.js** için bu blokla gevşer; uygulama kabuğu / cüzdan ekranları CSS motion durur. Papara/YK marka kopyası yok.
- **Neden:** Kullanıcı açıkça anime.js + YK hissi istedi. 1 kuralı ezer ve ağır. 2 runtime CDN. 4 isteği karşılamaz. Vendor dosya offline/CI-güvenli; MIT.
- **Sonra hangi dosya:** Coder `_AuthLayout.cshtml`, `wwwroot/js/vendor/anime.min.js`, `wwwroot/js/auth-hero.js`, `wwwroot/css/site.css` (auth-stage), `wwwroot/css/brand.css` (hero token), `wwwroot/css/motion.css` (auth-motion override). `docs/TASARIM.md` bir paragraf. `docs/HANDOFF.md` append. Domain / ledger / resx yok.

---

## T-055 — 2026-08-14 — Demo kayıtlı kart (Yükle/Çek paneli; PAN yok)

- **Kim:** Architect + Coder (kullanıcı: hesap bağla, kart ekle, karta para yüklensin)
- **Konu:** Gerçek Visa/Mastercard / banka OAuth / 3DS bu demo’da yok (SPEC + MARKA). Yükle/Çek zaten `Account` metni + sahte `IBankGateway`. Kart yüzü ve kayıtlı enstrüman listesi yok. 9. ekran mı, yoksa ekran 5 paneli mi?
- **Seçenekler:**
  1. Gerçek kart / iyzico / Papara API — hayır; lisans, 3DS, sır; ajan hesap açmaz.
  2. Yeni `/kartlar` ekranı — hayır; SPEC 8 sabit, 9. ekran yok.
  3. **Yükle/Çek paneli.** `LinkedInstrument` (UserId, Last4, Label) SQL tablosu. PAN/CVV yok; 16 hane istenmez. Seçim `AccountHint` = `****1234`. `IFundingExecutor` / gateway **değişmez**. Kart bakiyesi ayrı kasa değil; para cüzdan ledger.
- **Karar:** **3.** PageModel `ClearPayDbContext` yok; port `ILinkedInstrumentStore`. Timeout ipucu `TIMEOUT` durur. Footer **Demo — yükleme için sahte gateway**.
- **Neden:** 1 yasa dışı / kapsam dışı. 2 ekran listesini şişirir. 3 SPEC ekran 5’e sığar; kasa tek kalır.
- **Sonra hangi dosya:** Domain `LinkedInstrument`. Application `ILinkedInstrumentStore` + DTO. Infrastructure EF + `SqlLinkedInstrumentStore` + migration. Coder `YukleCek.cshtml(.cs)`, `SharedResource*.resx`, `brand.css` kart yüzü. Tester store test. HANDOFF append.

---

## T-056 — 2026-08-17 — Halil GitHub UI/operasyon taraması (cüzdan içi cilâ)

- **Kim:** Orchestrator + Architect + Coder (kullanıcı: kendi GitHub hesaplarındaki UI/operasyonu araştır, bu projeye yakışanı ekle)
- **Konu:** `gh api user` = **HalilMertDeveli** (org yok). `clearpay` zaten bu repo. Diğer public repolar tarandı; SPEC 8 ekran + WePay/Papara-tarzı **cüzdan sitesi** durur. Hangisi kopyalanır, hangisi reddedilir?
- **İncelenen (ilgili):** `BankAppAsp` (havale formu, hesap kartı, boş hesap metni — **UPDATE Balance**), `IdentityCourse` (giriş/kayıt switch, TempData flash, AccessDenied, cookie HttpOnly), `TaskManagementSystem` (Beni hatırla, özet kart, arama, SameSite Strict iddiası, profil/bildirim), `personal-Finance-Tracker` (Flutter giriş + “already have account”, pasta grafik, Firebase pembe tema), `Darky-Landing-Page` (wow.js / animate.css / koyu landing), `StoryGame` / `ASP-NET-E-Trade` / `Udemy.TodoAppNTier` (N-tier ders; Bootstrap). **LED yok:** `led-teknik-destek`, `ASP.NET-APP-FOR-LED` klon/kopya yok.
- **Seçenekler:**
  1. BankApp `Balance -= Amount` / `UPDATE` kasa; TaskManagement profil + bildirim 9. ekran; Flutter pasta/Firebase; Darky landing + wow.js karanlık mod; LED ürünü; satıcı/POS/Kafka; Cookie **SameSite=Strict** (Google/Apple OAuth kırılır) — hayır.
  2. **Cüzdan içi cilâ (8 ekran):** (a) IdentityCourse flash + AccessDenied **hata kromu** (`/erisim-yok`, Error.cshtml gibi; SPEC ekran listesi değil) + TaskManagement **Beni hatırla** (ekran 1). (b) BankApp gönderiminden tersine: çift tıklama **busy** (409 durur; buton `disabled` handler’ı düşürmesin) + bakiye 0/dondurulmuşta Havale Gönder kapalı (TASARIM zaten söyledi). (c) Dekont: correlation id **kopyala** + **yazdır** (`@media print`); başarılı havale/yükle/çek → dekont (fiş), Index flash kaybolmaz TempData dekontta.
- **Karar:** **2.** Dil seçici zaten TR/EN/DE/FR; yeni dil yok. Google/Apple durur. Ledger / PageModel DbContext / `UPDATE Balance` yok. npm/GSAP/wow.js yok.
- **Neden:** 1 kapsam dışı veya kasa/OAuth kırar. 2 Halil’in kendi ders/ürün kalıplarını cüzdan ekranlarına taşır; mülakat anlatımı (409 + fiş + Identity) güçlenir.
- **Sonra hangi dosya:** Coder `Dekont`/`Havale`/`YukleCek`/`Login`/`AccessDenied`, `site.js`, `site.css`/`brand.css`/`motion.css`, `SharedResource*.resx`. Application `LoginRequest.RememberMe`. Infrastructure `AccessDeniedPath`. Designer `docs/TASARIM.md`. Tester UI smoke. HANDOFF append. TASK-16 Todo kalır.

---

## T-057 — 2026-08-17 — Lokal Identity SQL Server (SQLite Development kalkar)

- **Kim:** Orchestrator + Architect + Coder + Deploy (kullanıcı: “yalnız SQLite var, lokal SQL Server kur, ilişkisel kullan”)
- **Konu:** T-009 Identity SQLite, ledger ayrı SQL. Olgu: Windows SQL Server (`MSSQLSERVER`) ayakta; `ClearPay` DB’de ledger tabloları var (`Wallet`…`LinkedInstrument`). Identity hâlâ `App_Data/identity.db`. Docker SQL (`D:\ClearPay\data\mssql`) kapalı/boş; uygulama Development’ta zaten `lpc:localhost` Windows SQL. SPEC kilit: SQL Server. Kullanıcı Identity’yi de aynı ilişkisel SQL’e almak istiyor. TASK-16 Azure değil.
- **Seçenekler:**
  1. T-009 durur (Identity SQLite) — kullanıcı reddi; SPEC “DB: SQL Server” ile çelişir.
  2. Identity ayrı `ClearPayIdentity` DB + EnsureCreated — Production tek Azure SQL bağlantısından sapar.
  3. **Aynı `ClearPay` SQL Server DB.** Development Identity = `UseSqlServer(ConnectionStrings:ClearPay)` (Production ile aynı). EF Identity migration; history `__EFMigrationsHistoryIdentity` (ledger history karışmaz). Test factory `ClearPay:UseSqliteLedger=true` → Identity+ledger SQLite (CI). Docker Compose / MySQL / Oracle / `D:\` bind **dokunulmaz**. Eski SQLite kullanıcıları taşınmaz; `IdentitySeeder` admin tohumu SQL’de. 8 ekran; `UPDATE Balance` yok; PageModel ledger yok.
- **Karar:** **3.** T-009 Development SQLite maddesi bu blokla kapanır. Canlı hâlâ Azure SQL (T-051). TASK-16 Todo kalır.
- **Neden:** 1 kullanıcı isteği + SPEC stack. 2 iki DB, canlı tek connection’a ters. 3 ilişkisel tek kasa+kilit; testler Compose/Windows SQL istemez.
- **Sonra hangi dosya:** Coder `AddClearPayIdentity`, `IdentitySeeder`, `AppIdentityDbContext` + factory + Identity migrations. `appsettings.Development.json` (Identity key dokümantasyon). Tester `dotnet test`. Deploy `docs/DEPLOY.md` / `docs/CANLI.md` / `docs/ARCHITECTURE.md` satır (Compose dosyası ezilmez). HANDOFF append. TASKS.md TASK-16 durur.

---

## T-057 — 2026-08-17 — Kamu cüzdan/ledger örnekleri: 8 ekranda olması gerekenler

- **Kim:** Orchestrator + Architect + Coder + Payments + Designer + Tester (kullanıcı: MY GitHub değil; dijital cüzdan / WePay / Papara-benzeri / Razor para / ledger UX kamu örnekleri; 2–5 ekle, commit/push)
- **Konu:** TASK-06/09/10 Done; TASK-16 Azure blok Halil. Ciddi demo cüzdanda 8 ekranı kırmadan ne eksik? Ledger mi, yoksa güven UX’i mi?
- **İncelenen (kamu, kopya değil):** `wepayui/wepayui` (ödeme incele + kayıt); `paparateam/papara-android` + `merchantApiClient-node` (`listLedgers` startDate+endDate, işlem referansı); `Emmanuel-Ejeagha/naira-ledger-engine` (P2P fiş, freeze, reversal, SignalR); `amirhossein-tohidi/fintech-wallet-service` (idempotency, outbox, reserve/confirm, Kafka); `muisoft/fintech-wallet-dotnet` (deposit/withdraw/transfer/history); `NuelUzoma/Digital_Wallet_System` (Redis idempotency); `birukdjn/ArifCore` (atomik P2P + 409-benzeri çift ödeme koruması); `Dedmoo/FintechLedgerApi` (reversal, statement); `ameer017/paylite` (geçmiş filtre, P2P); `Williansouzh/digital-wallets-backend-challenge`; `Tareq-Bilal/E-Wallet-Server-Side` (deposit/withdraw/refund); `Emmanuel-Ejeagha/digital-wallet-api`; `DouglasHutchful1/TranzaPay` (admin); `ak123456789/EIP-Sample` (PayPal-tarzı idempotency+outbox).
- **Seçenekler:**
  1. Kafka / reserve-confirm / Paystack-Flutterwave gerçek PSP / KYC+QR+SignalR+CSV yeni ekran / satıcı POS / IBAN çekirdeği / 2FA sayfası / PWA / `UPDATE Balance` — hayır.
  2. **8 ekran içi (kazanan):** (a) Havale **tutar onay** adımı (WePayUI review; aynı `/havale`). (b) Aynı idempotency key tekrarında **Replay → mevcut dekont** + anahtar kırıntısı (“aynı işlem iki kez kesilmez”; 409 API durur). (c) Hareketler **bitiş tarihi** (Papara listLedgers; tür filtresi durur) + özet/hareketler/dekontta correlation; yükle-çek dekontunda ****son4. (d) Admin **çöz** (SPEC dondur; naira freeze; tek yön dondurma demo’da kör). (e) Boş kuyruk/hareket `empty-block`+CTA; `aria-busy` iskelet **sonsuz shimmer yok** (T-040); skip-link durur; Google/Apple / Beni hatırla / kopyala-yazdır durur.
- **Karar:** **2.** TASK-16 Todo kalır. 9. ekran yok. `UPDATE Balance` yok. PageModel ledger yok. Outbox/409 SQL aynı.
- **Neden:** Ledger çekirdeği (çift kayıt, 409, freeze, outbox) zaten var; kamu cüzdanlarda eksik olan güven adımı (onay), tarih aralığı, unfreeze ve fişte referans/son4. 1 SPEC’i kırar veya sahte banka/PSP olur.
- **Sonra hangi dosya:** Coder `Havale`/`Hareketler`/`Dekont`/`Index`/`Admin` Razor+PageModel, `site.js`, `site.css`. Application `IActivityReader` + `IAdminPanel`. Infrastructure `SqlActivityReader`/`SqlAdminPanel`. Designer `docs/TASARIM.md` + `brand.css`. Tester xUnit. HANDOFF append.

---

## T-059 — 2026-08-17 — Yol haritası belgesi (Q1 kariyer, Q2 park)

- **Kim:** Orchestrator, Satış, İK, Deploy
- **Konu:** “Ne işe yarar, nereye götüreceğim” planı nereye yazılır; TASK-16 / Q2 ne zaman?
- **Seçenekler:**
  1. SPEC’i Papara/lisans ürününe çevir; TASK listesini Q2 satıcı paneliyle şişir.
  2. **Kalıcı `docs/YOL.md`:** Q1 = mülakat demosu (409/tx/outbox + TASK-16 URL). Kariyer kapısı ilk nakit. Kendi e-para lisansı kapalı. Q2 kapalı devre/white-label **park** (avukat + 9. ekran onayı yokken `src/` yok). TASK-16 URL ajan uydurmaz (`az login` Halil).
- **Karar:** **2.** Plan Cursor onayı; OWN `docs/YOL.md`. `src/` yok. TASKS Todo şişmez (TASK-16 durur).
- **Neden:** T-004/T-013 durur. Kod Q1 (TASK-01…15) bitti; eksik kanıt HTTPS. Ticari satış avukatsız yalan.
- **Sonra hangi dosya:** `docs/YOL.md`; `docs/GELIR.md` işaret; `docs/IK.md` (409/outbox kanıt TASK-06/11 Done); `docs/SENIN-ISLERIN.md` / `docs/CANLI.md` TASK-16 tık; `docs/HANDOFF.md` append. README Docs satırı.

---

## T-058 — 2026-08-17 — Lokal Identity SQL Server (numara: T-057 kamu cüzdana ait)

- **Kim:** Orchestrator (düzeltme). Önceki Identity bloğu T-057 başlığıyla yazıldı; aynı gün kamu cüzdan maddesi de T-057 aldı.
- **Konu:** Identity SQLite → Windows SQL Server `ClearPay` kararı hangi numarada durur?
- **Seçenekler:**
  1. Identity bloğunu silip kamu T-057 tek kalsın — HANDOFF overwrite / silme yasak.
  2. **Identity kararı T-058.** Kamu cüzdan T-057 durur. Üstteki Identity T-057 başlığı tarihsel; uygulama T-058.
- **Karar:** **2.** Kod/docs T-058. T-009 Development SQLite kapanır. TASK-16 Todo.
- **Neden:** Çift T-057 çatışması; düzeltme yeni blok.
- **Sonra hangi dosya:** HANDOFF append T-058. `AddClearPayIdentity` mesajı T-058. DEPLOY/ARCHITECTURE T-058.

---

## T-060 — 2026-08-17 — Gelecek senaryoları (dört masa; YOL’a katalog)

- **Kim:** Orchestrator + Product + Architect + Payments + Sales/İK (kullanıcı: ajanlar tartışsın, proje geleceğini hesaplasın, olası senaryo listesi)
- **Konu:** TASK-01…15 bitti; TASK-16 URL Halil’de. T-059 `docs/YOL.md` Q1 kariyer / Q2 park kilitli. Ayrı `GELECEK.md` mi, yoksa YOL’a senaryo kataloğu mu? Hangi gelecek **izlenir**, hangisi **park**, hangisi **kapalı**?
- **Seçenekler:**
  1. Papara / kendi e-para lisansı (Yol A) / 9. ekran / Kafka / gerçek Visa-FAST — hayır (T-004, T-013, SPEC).
  2. URL’den önce satıcı paneli + özellik fabrikası — hayır; Sales/İK vetosu, Architect SPEC kilit.
  3. **YOL kataloğu.** T-059 durur. Dört masa senaryoları `docs/YOL.md` içine yazılır (ikinci OWN yok). Kazanan 12 ay: tek host + TASK-16 HTTPS + Yol B (mülakat maaşı). 8 ekran cilâsı (T-056/057) içeride. Q2 C/D + satıcı = park (avukat + kullanıcı onayı). Ledger invarianti Payments; mikroservis/Kafka dual-write reddi.
- **Karar:** **3.** `src/` yok. SPEC 8 değişmez. TASK-16 Todo. `GELECEK.md` açılmaz (T-059 ile çatışır).
- **Neden:** Product/Sales/Architect/Payments aynı kapıya çıktı: nakit = kariyer; kanıt = tarayıcıda 409; kasa = SQL çift kayıt. 1 yasa/iddiayı bozar. 2 wedge’i sulandırır. 3 T-059’u ezer değil, hesaplanmış liste ekler.
- **Sonra hangi dosya:** Orchestrator `docs/YOL.md` senaryo bölümü; `docs/HANDOFF.md` append. SPEC/PLAN/TASKS ekran eklenmez.

---

## T-061 — 2026-08-17 — Flutter JWT istemci (Q2; 8 ekran durur)

- **Kim:** Orchestrator, Architect, Coder (kullanıcı onaylı plan: ASP.NET site + Flutter mobil, aynı ledger)
- **Konu:** Site C#, mobil Flutter olabilir mi? Eşzaman ikinci defter mi? SPEC “site” + eski Flutter reddi (Firebase/pasta) bu istemciyi yasaklar mı?
- **Seçenekler:**
  1. Flutter yok — yalnız Razor (kullanıcı Q2 istedi).
  2. **JWT istemci:** `GET /api/wallet|movements|receipts` + mevcut `POST /api/transfers` + `POST /api/topup|withdraw`; CORS debug+canlı kök. `mobile/clearpay` Dart. Domain’e Dart yok. 8 ekran. Pull-to-refresh = Q2.1 eşzaman. SignalR yok. Hive/`UPDATE Balance` yok.
  3. Flutter’da offline bakiye / ikinci SQL / 9. ekran / PWA — hayır.
- **Karar:** **2.** Q1 TASK-16 Todo durur. `ClearPay.slnx` Flutter içermez. LED yok.
- **Neden:** İki istemci, tek kasa (T-019 portlar). Eski Flutter reddi Firebase ikiziydi. Cookie≠JWT (ARCHITECTURE).
- **Sonra hangi dosya:** Coder `src/ClearPay.Web/Controllers/**`, `OpenApi`, `Program.cs` CORS. Tester JWT GET/POST. `mobile/clearpay/**`. SPEC/ARCHITECTURE/YOL/README birer cümle. `docs/HANDOFF.md` append. Domain/Persistence rewrite yok.

---

## T-062 — 2026-08-17 — Flutter’da site işlemleri + ajanlar mobil OWN

- **Kim:** Orchestrator, Coder (kullanıcı: uygulama kurulsun; işlemler Flutter içinde; ajanlar mobil içinde çalışsın)
- **Konu:** T-061 JWT istemci var ama kayıt sitede; Windows’ta `flutter run` yok; Coder kuralı yalnız Razor. Ajan kökü yalnız `mobile/` olursa TARTISMA kaybolur.
- **Seçenekler:**
  1. WebView ile site — hayır (T-061 cookie≠para API).
  2. **JWT tam müşteri akışı:** `POST /api/register` (aynı `RegisterRequest`, cookie SignIn yok, JWT döner). `GET/POST /api/cards`. Admin JWT `IAdminPanel` (rol Admin; 9. ekran değil). Flutter: kayıt, özet kısayol, havale kalan bakiye + onay, kartlı yükle/çek, hareket filtre, dekont, admin sekmesi. Windows masaüstü (`flutter create --platforms=windows`) bu PC’de kurulum. Coder OWN += `mobile/**/*.dart`. Workspace: repo + `mobile/clearpay` (ajanlar Dart’ta; TARTISMA kökte). Hive yok.
  3. Ajan kökünü yalnız `mobile/clearpay` yapmak — hayır (`docs/` ve `src/` TARTISMA/ledger).
- **Karar:** **2.** TASK-16 Todo durur. Domain’e Dart yok.
- **Neden:** Kullanıcı işlemleri telefonda/Windows Flutter’da; para hâlâ C# port. Çok köklü workspace TARTISMA’yı korur.
- **Sonra hangi dosya:** `RegisterController`, `CardsController`, `AdminApiController`. `mobile/clearpay/lib/**`, Windows platform. `.cursor/rules/coder.mdc` + `flutter.mdc`. `docs/AGENTS.md`, HANDOFF append. Tester register+cards.

---

## T-063 — 2026-08-17 — Flutter aynı git repo + çok köklü workspace

- **Kim:** Orchestrator (kullanıcı: Flutter uygulaması ek olarak şuanki repo olarak eklensin)
- **Konu:** `mobile/clearpay` Cursor’da ayrı “current repo” mı, iç içe `git init` mı, yoksa aynı GitHub repo + workspace klasörü mü?
- **Seçenekler:**
  1. `mobile/clearpay` içinde ikinci `git init` / ayrı remote — hayır (TARTISMA iki yer, submodule karmaşası).
  2. **Aynı repo:** `HalilMertDeveli/clearpay` içinde `mobile/clearpay`. `ClearPay.code-workspace` iki klasör (site + Flutter). `ClearPay.slnx` Flutter içermez (T-061).
  3. Yalnızca ajan kökünü `mobile/` yapmak — hayır (T-062).
- **Karar:** **2.** TASK-16 Todo durur.
- **Neden:** Tek kasa, tek git; Flutter ikinci istemci klasörü. Ajanlar Dart’ta, TARTISMA kökte.
- **Sonra hangi dosya:** `ClearPay.code-workspace`, `.gitignore` (ephemeral/local.properties), `mobile/clearpay` kaynak git’te. HANDOFF append. `git init` yok.

---

## T-064 — 2026-08-17 — README: web+mobil aynı işlemler; mobil README kök stili

- **Kim:** Orchestrator / Sales copy (kullanıcı: repoya giren hem mobil hem web işlemlerini görsün; kök README güzel, mobil için benzeri)
- **Konu:** GitHub’da yalnız site mi görünür? `mobile/clearpay/README.md` kısa kaldı.
- **Seçenekler:**
  1. İki ayrı ürün README — hayır (iki kasa izlenimi).
  2. **Kök README:** 8 işlem tablosu Web rotası + Flutter ekran. Flutter rozeti. Mermaid’e JWT istemci. Mobil README kök ile aynı düzen (rozet, mermaid, ekran tablosu, demo, cmd). 9. ekran yok.
  3. Papara/mağaza vaadi — hayır.
- **Karar:** **2.** TASK-16 Todo. `src/` yok.
- **Neden:** Ziyaretçi “aynı kişi, iki istemci, tek defter” görsün.
- **Sonra hangi dosya:** `README.md` + TR/DE/FR; `mobile/clearpay/README.md`; HANDOFF append.

---

## T-065 — 2026-08-17 — Flutter Firebase yapılandırması (istemci; kasa değil)

- **Kim:** Orchestrator + Coder (kullanıcı: Flutter için Firebase yapılandırması)
- **Konu:** `mobile/clearpay` JWT istemci. T-061 eski reddi **Firebase ikizi / Firestore bakiye** içindi. Şimdi Google projesi + `firebase_core` mi, yoksa Auth/Firestore ikinci defter mi?
- **Seçenekler:**
  1. Firebase Auth + Firestore/Hive bakiye — hayır (T-061; `UPDATE Balance` ikizi; SQL ledger kırılır).
  2. **`firebase_core` istemci init.** JWT + C# port + SQL kasa durur. 8 ekran. 9. ekran / FCM inbox yok. `google-services.json` / `firebase_options.dart` FlutterFire üretir. Ajan Firebase/Google Cloud projesi **açmaz** (GIRIS-SOSYAL / CANLI ile aynı). CLI yoksa iskelet + Halil `firebase login` + `configure-firebase.cmd`. Crashlytics/Analytics sonra; şimdi yalnız core.
  3. Firebase’siz — kullanıcı yapılandırma istedi.
- **Karar:** **2.** TASK-16 Todo durur. Domain’e Dart yok. `ClearPay.slnx` Flutter içermez.
- **Neden:** 1 ikinci kasa. 3 isteği karşılamaz. 2 mülakat omurgası SQL/409; Firebase yalnızca mobil Google bağları.
- **Sonra hangi dosya:** Coder `mobile/clearpay` (`pubspec`, `lib/firebase/**`, `main.dart`, Android Gradle koşullu plugin, `tool/configure-firebase.cmd`, README). `docs/SENIN-ISLERIN.md` tık. HANDOFF append. `src/` yok.

---

## T-066 — 2026-08-17 — Flutter sol çekmece + perakende banka kromu (YK kopyası değil)

- **Kim:** Orchestrator + Coder (kullanıcı: solda pencere/panel; Yapı Kredi mobil gibi UI, iç özellikler sonra)
- **Konu:** Flutter’da sol panel + TR banka uygulaması ana ekran dili. Pixel-clone / YK ürün ızgarası mı, yoksa mevcut 8 işleme map’li ClearPay kromu mu?
- **Seçenekler:**
  1. Yapı Kredi piksel kopyası (logo, YK altın/mavi kimlik, kredi/döviz/fatura/QR ızgarası) — **red** (marka; SPEC 9. ekran; lisans iddiası).
  2. **Kazanan:** Sol `NavigationDrawer` + perakende-banka mobil kromu (app bar hamburger, hesap/bakiye kartı, kısayol karoları, işlem listesi). ClearPay navy `#1B2A4A`. Footer **Demo — sahte banka gateway.** Çekmece + ana kısayollar yalnız mevcut 8 işlem: Giriş (zaten), Kayıt (auth), Özet, Havale, Yükle/Çek, Hareketler, Dekont (hareketten), Admin (JWT Admin). Kartlar bugünkü gibi Yükle/Çek altında.
  3. Kredi / fatura / QR için yeni ekranlar — **park** (kullanıcı sonra dedi; yeni TARTISMA + SPEC şart).
- **Karar:** **2.** TASK-16 Todo durur. 9. ekran yok. Papara/YK wordmark yok.
- **Neden:** Aynı kişi, iki istemci, tek defter. TR banka uygulamasının UI dili (hamburger, hesap kartı, kısayol ızgarası) YK olmayı gerektirmez. 1 marka+SPEC kırar. 3 şimdi kapsam dışı.
- **Sonra hangi dosya:** Coder `mobile/clearpay/lib/screens/shell_screen.dart`, `overview_screen.dart`, `theme.dart`, `api/clearpay_client.dart` (JWT e-posta), `test/widget_test.dart`. HANDOFF append. `src/` yok. SPEC ekran listesi durur.

---

## T-067 — 2026-08-17 — Flutter ana ızgara + TC demo giriş + QR kanal (YK/FAST kopyası değil)

- **Kim:** Orchestrator + Coder (kullanıcı: Yapı Kredi benzeri ana alanlar; TC giriş; FAST/Piyasalar/QR; Daha fazla; uygulamadan QR ile öde/al)
- **Konu:** Ana ekranı TR banka hissi ile genişletmek. Pixel-clone (World/Jet QR/FAST gerçek ray) mı, yoksa aynı 8 işlem + kanal mı?
- **Seçenekler:**
  1. YK piksel kopyası; World / Jet QR / FAST gerçek ray — **red** (marka + lisans iddiası + 9. ekran).
  2. **Kazanan:** Aynı 8 işlem. Özet **kısayol ızgarası** + **Daha fazla**. **TC ile giriş:** mevcut girişte sekme (9. ekran değil). TCKN Mernis değil. Identity e-posta-only → Flutter demo map `10000000146` → `admin@clearpay.test`, arkada `POST /api/token`. **FAST** → Havale («Demo P2P — TCMB FAST değil»). **Piyasalar** park («Yatırım/döviz yok; SPEC 8»). **QR ile öde / QR ile al** (Jet QR / World Pay değil) → Özet+Havale kanalı; `qr_flutter`; `POST /api/transfers` + Guid `Idempotency-Key`. **Daha fazla:** 8 işlem + park (Piyasalar, Fatura, Kredi) «Park — demo değil».
  3. Gerçek Worldcard / TCMB FAST / Mernis — **red**.
- **Karar:** **2.** TASK-16 Todo. 9. ekran yok. Firebase/Hive kasa yok. `UPDATE Balance` yok.
- **Neden:** Banka evi dili YK olmayı gerektirmez. QR ürün değil kanal. TC KYC değil.
- **Sonra hangi dosya:** Coder `mobile/clearpay` login/overview/transfer/shell, `lib/demo/`, `lib/qr/`, `qr_flutter`, test, README. HANDOFF append.

---

## T-068 — 2026-08-17 — Splash + Bireysel/Kurumsal (SQL Identity; Firebase kasa yok)

- **Kim:** Orchestrator + Coder (kullanıcı: açılış animasyonu; Kurumsal/Bireysel; bunları Firebase’de ayrı tut)
- **Konu:** İki mod ve splash. Firestore/Firebase Auth mı, aynı SQL Identity mi?
- **Seçenekler:**
  1. İki Firebase / Firestore / Firebase Auth — **red** (T-061 ikinci kasa yok; T-065 `firebase_core` yalnız istemci bootstrap; Auth/Firestore cüzdan değil; yeni Firestore eklenmez).
  2. **Kazanan:** Splash (ClearPay tween; YK asset yok) → **Bireysel | Kurumsal** iki kart → giriş (T-067 e-posta + TC). Son mod `%LOCALAPPDATA%/ClearPay/account_kind.txt` (plugin yok). Sunucu: `ApplicationUser.AccountKind` aynı Identity SQL; JWT `account_kind`. Kayıt/giriş round-trip. Seed `admin@clearpay.test` = Bireysel. **Kurumsal POS / 9. ekran / üye iş yeri değil** — aynı 8 işlem; rozet/kopya.
  3. Yalnız Flutter local — **red** (kullanıcı sunucu ayrımı istedi).
- **Karar:** **2.** TASK-16 Todo. Bireysel/Kurumsal Firebase’e **yazılmaz**. `UPDATE Balance` yok.
- **Neden:** İkinci kullanıcı deposu T-061 kırar. SQL Identity dürüst ayrım.
- **Sonra hangi dosya:** Coder `ApplicationUser` + Identity migration; JWT/token/register; Flutter splash/mode/login/main; tests. HANDOFF append.

---

## T-070 — 2026-08-17 — Eşzamanlı çalışma belgesi (git / masalar / makine)

- **Kim:** Orchestrator (kullanıcı: eşzamanlı çalışma için gereken yapı; öğretici + bu projenin hali; paylaş)
- **Konu:** Chat cevabı mı, yoksa kalıcı OWN doküman + GitHub blob + isteğe Notion mu?
- **Seçenekler:**
  1. Yalnız sohbet özeti — kapanınca kaybolur; paylaşılmaz.
  2. **`docs/ESZAMANLI.md`:** üç katman (Git, masalar/ajanlar, makine); TASKS dürüst snapshot; yaşanan çatışmalar (MSB3027, VMP, Notion Publish, ERR_CONNECTION_REFUSED). README/README.tr bir satır. Notion kopya isteğe. `src/` yok.
  3. HANDOFF’a uzun tartışma / ikinci `OTURUM-PLAN` — hayır (append defteri; tıklama sırası zaten var).
- **Karar:** **2.** TASK-16 Todo durur. 409 TASK-06 Done (uydurma skip yok). Azure URL uydurulmaz.
- **Neden:** 1 paylaşılmaz. 3 HANDOFF/OWN karışır. 2 öğretir ve bu repo’nun gerçek paralelini (Architect T-016, docs masaları; iki Payments yok) yazar.
- **Sonra hangi dosya:** Orchestrator `docs/ESZAMANLI.md`; `README.md` + `README.tr.md` Docs satırı; HANDOFF append. Notion yeni sayfa (Publish Halil).

---

## T-073 — 2026-08-17 — Web + mobil + JWT parite dalgası (8 ekran)

- **Kim:** Orchestrator (onaylı plan: Web Mobil Backend Wave)
- **Konu:** Aynı 8 cüzdan işlemini Razor, Flutter ve JWT API’de hizalamak. 9. ekran / ikinci kasa mı?
- **Seçenekler:**
  1. Flutter Google/Apple (cookie OAuth) + PWA + Firestore/Hive bakiye + satıcı/POS/Kafka — **red**.
  2. **Kazanan:** 8 ekran parite. Backend: `GET /api/transfers/{id}` (201 Location), JWT 401 `ProblemDetails`, `GET /api/movements` `page`/`pageSize`. Web: Yükle **İptal**, Admin topbar rolü. Flutter: T-066 çekmece durur; hareket tarih+sayfa; güvenli JWT; 401 → giriş; dondurulmuşta Havale/Yükle/Çek kapalı. Designer token `brand.css`. `UPDATE Balance` yok.
  3. Yalnız kozmetik — **red** (Location boş, 401 gövdesi, freeze UX ledger kadar mülakat).
- **Karar:** **2.** TASK-16 Todo durur. Azure URL uydurulmaz.
- **Neden:** İki istemci, tek C# kasa (T-061). Cookie ≠ JWT. Ledger kuralları bozulmaz.
- **Sonra hangi dosya:** Payments `Controllers/TransfersController`, `ITransferLookup`, JWT events, `MovementsController`. Coder Razor `YukleCek.cshtml`, `_Layout.cshtml`. Coder Flutter `mobile/clearpay/lib/**`. Designer `TASARIM.md` + `brand.css`. Tester `TransferApiTests` + `flutter analyze`. HANDOFF append.

---

## T-071 — 2026-08-17 — Mobil↔web canlı bakiye (SignalR chrome)

- **Kim:** Orchestrator + Architect + Coder + Payments (kullanıcı: mobilde değişiklik web’de otomatik yansısin; eşzamanlı yapı; API’de Halil adımları)
- **Konu:** T-061 “SignalR yok / pull-to-refresh = Q2.1”. İki istemci aynı SQL kasayı yazıyor; web özeti Razor HTML — mobil POST sonrası tarayıcı eski bakiyeyi gösterir. T-070 `ESZAMANLI.md` git/masa öğreticisidir, para push’u değil. İkinci defter veya 9. ekran mı?
- **Seçenekler:**
  1. Yalnız pull-to-refresh (T-061) — kullanıcı otomatik istedi.
  2. **SignalR hub chrome:** `/hubs/wallet` (9. ekran değil). Ledger commit **sonra** `IWalletLiveNotifier` → grup `user:{id}`. Payload yalnız `{ reason, correlationId }` — bakiye yok; istemci `GET /api/wallet` veya Razor reload. Cookie (Razor) + JWT `access_token` query (Flutter). Firestore/FCM/Hive/`UPDATE Balance` yok. Kafka yok. T-057 “KYC+QR+SignalR+CSV yeni ekran” reddi durur.
  3. Firebase FCM / Firestore dinleyici — hayır.
- **Karar:** **2.** TASK-16 Todo durur. T-061 pull-to-refresh yedek (hub kopunca). Domain’e Dart yok. T-066 çekmece kararı durur.
- **Neden:** 1 isteği karşılamaz. 3 ikinci kaynak. 2 tek kasa + push “yenile”; mülakat: SignalR ≠ ledger.
- **Sonra hangi dosya:** Application `IWalletLiveNotifier`. Infrastructure NoOp + executor/admin notify. Web `WalletHub` + `site.js`. Flutter `signalr_netcore`. `docs/API-ESZAMAN.md`. HANDOFF append. SPEC 8 ekran aynı.

---

## T-072 — 2026-08-17 — Web internet-şube kromu (YK düzeni; marka kopyası değil)

- **Kim:** Orchestrator + Designer + Coder (kullanıcı: web uygulaması için güzel arayüz; Yapı Kredi *web* örnek)
- **Konu:** T-054 auth hero + T-053 mobil tabbar durur. Giriş sonrası Razor hâlâ dar 560px kolon + navy sidebar. YK internet şube *düzeni* (üst şerit + sol menü + hesap kartı + hızlı işlem karoları) mı, yoksa YK/Worldcard piksel kopyası mı?
- **Seçenekler:**
  1. Yapı Kredi / Worldcard piksel kopyası (logo, altın kimlik, kredi, kampanya) — **red** (marka; lisans iddiası; 9. ekran).
  2. **Kazanan:** Girişli Razor = internet-bankacılığı *web kromu*. Tam genişlik navy **masthead** (ClearPay + «İnternet» kicker + kullanıcı/çıkış). Sol menü 8 işlem (wordmark tekrar yok). Masaüstü içerik **~1120px** (T-053 560px kolon gevşer; ≤800px **tabbar durur**). Özet: hesap kartı + **Hızlı işlemler** beyaz karolar (Gönder/Yükle/Çek/Hareketler). Renk MARKA navy `#1B2A4A`. Auth T-054 değişmez. Ledger / `UPDATE Balance` yok.
  3. Kredi / fatura / 9. ekran — **park**.
- **Karar:** **2.** TASK-16 Todo durur. YK/Papara wordmark yok. T-071 hub kromu durur (ekran değil).
- **Neden:** Kullanıcı web yüzeyini YK *şube sitesi* gibi istedi; 1 marka+SPEC kırar. 3 kapsam dışı. T-015 «şube kromu yok» girişli kabuk için T-072 ile güncellenir; ürün hâlâ demo cüzdan.
- **Sonra hangi dosya:** Coder `_Layout.cshtml`, `Index.cshtml`, `site.css` / `brand.css`, resx 4 dil. Designer `TASARIM.md` + `MARKA.md`. Tester AuthOrUi. HANDOFF append.

---

## T-074 — 2026-08-17 — Web + Flutter iki platformda çalışma (hata kanıtı)

- **Kim:** Orchestrator + Coder (kullanıcı: 2 platform içinde çalışma sağla, hataları çöz)
- **Konu:** Site (Razor `:5153`) ve Flutter (Android emülatör + Windows masaüstü) birlikte. Tahminle yama yok; önce runtime log.
- **Seçenekler:**
  1. Kör yama (NDK pin, CORS gevşet, plugin sil) — **red** (kanıtsız; ledger/`UPDATE Balance` riski).
  2. **Kazanan:** Hipotez log’u (API taban URL, token yükleme, `/api/token`, SignalR hub, Windows symlink). Kanıt sonrası tek yama. 9. ekran yok. Firebase kasa yok.
  3. Flutter web/PWA (`dart:io` kırılır) — **red** (T-073).
- **Karar:** **2.** TASK-16 Todo durur. Windows Developer Mode kullanıcı tıklar (`ms-settings:developers`); ajan hesap açmaz.
- **Neden:** Terminalde Windows symlink ve Android NDK/cache zaten göründü; giriş/hub/URL henüz log’suz.
- **Sonra hangi dosya:** Coder Flutter `mobile/clearpay/lib/**` (debug ingest). Coder Web `TokenController`, `WalletHub`, `site.js`. HANDOFF append (landed değil; debug).

---

## T-075 — 2026-08-17 — Windows masaüstü: firebase_core C++ SDK zip yok (JWT durur)

- **Kim:** Coder (T-074 log + `flutter build windows`)
- **Konu:** Developer Mode açılınca symlink geçti. CMake `firebase_cpp_sdk_windows_13.9.0.zip` (~959MB) `ARCHIVE_EXTRACT` “File size could not be restored”. Firebase options Windows’ta zaten stub throw (T-065).
- **Seçenekler:**
  1. Zip’i elle aç / `FIREBASE_CPP_SDK_DIR` — 1GB SDK, JWT için gerekmez, extract yine kırılabilir.
  2. **Kazanan:** Windows native plugin listesinden `firebase_core` çıkar. Android `firebase_core` durur. Dart `initClearPayFirebase` catch aynı. `flutter_secure_storage_windows` durur. `UPDATE Balance` yok.
  3. pubspec’ten `firebase_core` sil — **red** (T-065 Android bootstrap).
- **Karar:** **2.** TASK-16 Todo durur. 9. ekran yok.
- **Neden:** Log: token 200 + web hub cookie bağlandı. Windows bloğu Firebase C++ extract; runtime zaten skip.
- **Sonra hangi dosya:** Coder `windows/CMakeLists.txt`, `windows/clearpay_plugins.cmake`, `windows/runner/clearpay_plugin_registrant.cc`, `runner/CMakeLists.txt`. HANDOFF append.

---

## T-076 — 2026-08-17 — Flutter web (Chrome) JWT istemci; Razor site durur

- **Kim:** Coder (kullanıcı: web tarafı çalışmıyor)
- **Konu:** `flutter build web` → «This project is not configured for the web.» Razor `:5153` cookie giriş 200 (curl). Chrome/Edge cihazı Flutter’da kırık. 9. ekran / PWA mağaza mı?
- **Seçenekler:**
  1. Yalnız Razor’u web say — kullanıcı Chrome cihazını seçince yine kırılır.
  2. **Kazanan:** `flutter create --platforms web`. `dart:io` koşullu import. API `http://localhost:5153` (CORS T-061 localhost). Aynı 8 ekran, aynı SQL JWT. Hive/Firestore kasa yok. Razor internet-şube durur.
  3. Azure canlı URL / PWA mağaza — **red** (TASK-16 blok Halil).
- **Karar:** **2.** TASK-16 Todo durur. T-073 PWA red durur; bu yalnızca `flutter run -d chrome` JWT.
- **Neden:** Yeni olgu: web klasörü yok. `dart:io` web’de derlenmez. CORS zaten localhost.
- **Sonra hangi dosya:** Coder `mobile/clearpay/web/**`, `lib/platform/**`, `dart:io` ayrımı, `auth-hero.js` opacity yedek. HANDOFF append.

---

## T-076 — 2026-08-17 — Development SQL: LocalDB (MSSQLLocalDB)

- **Kim:** Orchestrator + Coder + Deploy (kullanıcı: tam MSSQLSERVER değil; SQL Server LocalDB)
- **Konu:** T-058 Identity+ledger aynı Windows SQL `ClearPay` durur. Development şu an `lpc:localhost` (varsayılan `MSSQLSERVER`). Kullanıcı `(localdb)\MSSQLLocalDB` istiyor. Docker SA / Azure / TASK-16 değişmez.
- **Seçenekler:**
  1. `lpc:localhost` MSSQLSERVER durur — kullanıcı reddi (LocalDB istedi).
  2. **Kazanan:** Development `ConnectionStrings:ClearPay` + design-time factory = `Server=(localdb)\MSSQLLocalDB;Database=ClearPay;Integrated Security=True`. Identity T-058 aynı DB (SQLite `identity.db` yok). Testler `UseSqliteLedger`. Production Azure boş string. Docker Compose / `D:\ClearPay\data` dokunulmaz.
  3. Docker `sa` @ `:1433` Development — hayır (engine sık kapalı; kullanıcı Windows LocalDB).
- **Karar:** **2.** T-058 ilişkisel SQL durur (LocalDB hâlâ SQL Server). TASK-16 Todo. 8 ekran. `UPDATE Balance` yok.
- **Neden:** 1 kullanıcı isteği değil. 3 Docker şartı. 2 VS/dotnet LocalDB; tam instance şart değil.
- **Sonra hangi dosya:** Coder `appsettings.Development.json`, `AppIdentityDbContextFactory`, `ClearPayDbContextFactory`. Deploy `docs/DEPLOY.md` lokal satır. Migrate + `IdentitySeeder`. HANDOFF append. Production json yok.

---

## T-077 — 2026-08-17 — PC MySQL yan motor (web+mobil config; ledger MSSQL)

- **Kim:** Deploy + Coder (kullanıcı: PC’ye MySQL kur + web ve mobil için bağlantı config)
- **Konu:** SPEC kilit DB **SQL Server**. T-029/T-021: uygulama ledger MSSQL. MySQL Compose yan motor (`docker-compose.databases.yml`, `D:\ClearPay\data\mysql`). Flutter ikinci defter yasak (T-061 Hive/SQLite/Firestore — Dart MySQL aynı yasak). Kullanıcı PC kurulumu istedi (yalnız Docker yetmez). T-076 LocalDB Identity+ledger dokunulmaz.
- **Seçenekler:**
  1. Identity+ledger’ı web **ve** Flutter için MySQL’e taşı — **red** (SPEC, dual-write, 409/outbox SQL Server).
  2. **Kazanan (robust):** Kurulu Windows **MySQL84** varsa onu kullan (yeni installer yok). Yan motor connection config: `ConnectionStrings:MySql` Development’ta; `AddClearPay` / Identity LocalDB/SQL Server kalır. Flutter mysql paketi / bakiye yok. Compose `docker-compose.yml` ile birleşmez (T-020).
  3. Kurulumu atla, yalnız Docker MySQL — kullanıcı PC kurulumu istedi.
- **Karar:** **2.** TASK-16 Todo durur. 8 ekran. `UPDATE Balance` yok. Firebase yok.
- **Neden:** 1 SPEC ve 409/outbox’ı kırar. 3 kullanıcı isteği değil. 2 yan motor + araç (Workbench); kasa hâlâ SQL Server. Mobil JWT → C# → SQL.
- **Sonra hangi dosya:** Deploy `.env.example`, `docs/DEPLOY.md` (bir satır), `docker-compose.databases.yml` yorum (birleşmez). Coder `appsettings.Development.json` (`MySql` anahtarı; ClearPay/Identity LocalDB kalır), `mobile/clearpay/README.md`, kök README notu. HANDOFF **append**. Production json / `AddClearPay` / Pomelo yok.

---

## T-078 — 2026-08-17 — README mermaid ERD (LocalDB gerçek şema)

- **Kim:** Orchestrator / Sales copy (kullanıcı: ilişkisel DB diyagramı README; push)
- **Konu:** GitHub kök README’de şema. Ekran görüntüsü `Wallet.Balance` gösterebilir — yasak. 9. ekran / Papara / lisanslı e-para iddiası?
- **Seçenekler:**
  1. SSMS screenshot kopyala — **red** (`Balance` kolonu yalanı; T-003).
  2. **Kazanan:** Kök `README.md` (GitHub varsayılan EN) mermaid `erDiagram`; gerçek tablolar ve EF FK’ler. `Wallet`’ta **Balance yok**; bakiye = `LedgerPair.NetOf` (C# yardımcı; SQL tablosu değil). Identity + ledger aynı LocalDB `ClearPay`; iki history: `__EFMigrationsHistory` + `__EFMigrationsHistoryIdentity`. `Wallet.UserId` unique, AspNetUsers’a **FK yok** (iki DbContext). Flutter JWT + `firebase_core` proje `clearpay-c0485` (Firestore kasa yok). MySQL `ConnectionStrings:MySql` yan motor. 8 ekran. TR/DE/FR aynı yapı (T-064). Caption: Demo — sahte banka gateway. Lisanslı e-para değil.
  3. Papara / 9. ekran / lisanslı e-para — **red** (T-004, SPEC).
- **Karar:** **2.** TASK-16 Todo durur. `src/` bu maddede yok.
- **Neden:** 1 şema yalanı. 3 iddia. 2 mülakat + GitHub’da doğru ERD.
- **Sonra hangi dosya:** `README.md` + `README.tr.md` + `README.de.md` + `README.fr.md`; HANDOFF **append**.

---

## T-079 — 2026-08-17 — Web dekont PDF (mevcut ledger; 9. ekran değil)

- **Kim:** Orchestrator + Coder + Payments (kullanıcı: dekont işini web tarafında da yap)
- **Konu:** Mobil/API dekontu `GET /api/receipts/{id}` ile ledger’dan okunuyor. Site `/dekont/{id}` HTML + yazdır var (T-056). Kullanıcı **gerçek PDF** ve **bir örnek fiş** istiyor. Uydurma fiş / ikinci kasa mı?
- **Seçenekler:**
  1. PDF’siz HTML yazdır kalsın; örnek yok — kullanıcı PDF + örnek istedi.
  2. **Kazanan:** PDF = **var olan** çift kayıt belgesi (`correlationId`). `IReceiptPdf` ReceiptDto → byte[]. QuestPDF Infrastructure. Razor `OnGetPdf` (cookie) + `GET /api/receipts/{id}/pdf` (JWT). Development örnek: admin cüzdanına **LedgerPair** yükleme (sabit Guid); `UPDATE Balance` yok. UseSqliteLedger (test) seed yok. Yazdır/kopyala durur. 9. ekran / e-posta yok.
  3. Fişsiz PDF, Hive/Firebase fiş, Worldcard kopyası — **red**.
- **Karar:** **2.** TASK-16 Todo durur. SPEC ekran 7 aynı path.
- **Neden:** 1 isteği karşılamaz. 3 ikinci kaynak. 2 mülakat: PDF ≠ kasa; HTML ve PDF aynı SQL satırı.
- **Sonra hangi dosya:** Application `IReceiptPdf`. Infrastructure QuestPDF + `DemoReceiptSeeder`. Web `Dekont` handler + `ReceiptsController`. resx 4 dil. Tester PDF `%PDF` + 404. TASARIM/SPEC/URUN bir satır. HANDOFF append.

---

## T-080 — 2026-08-17 — Flutter Chrome web JWT (T-076 numara çakışması düzeltmesi)

- **Kim:** Coder (kullanıcı: web tarafı çalışmıyor). TARTISMA’da iki **T-076** vardı; LocalDB T-076 durur.
- **Konu:** `flutter build web` «not configured for the web.» `dart:io` Chrome’da derlenmez. Razor `:5153` IIS Express kilit / MSB3027 ayrı.
- **Seçenekler:**
  1. Yalnız Razor — Chrome cihazı kırık kalır.
  2. **Kazanan:** `flutter create --platforms web` + `dart:io` koşullu import. API `localhost:5153`. 8 ekran. PWA mağaza yok (T-073).
- **Karar:** **2.** TASK-16 Todo durur.
- **Neden:** Kullanıcı web istemcisini Chrome’da da istedi; LocalDB kararı T-076’da ayrı.
- **Sonra hangi dosya:** Coder `mobile/clearpay/web/**`, `lib/platform/**`. HANDOFF append.

---

## T-069 — 2026-08-17 — Dekont gerçekten oluşsun (Flutter boru) + PDF aynı fiş

- **Kim:** Payments + Coder (kullanıcı: dekont oluşturulsun; bir örnek; sonra gerçek PDF)
- **Konu:** Hareket satırında «Dekont» var ama havale/yükle/QR öde sonrası fiş açılmıyor; PDF yoktu. Fişsiz PDF / 9. ekran / Hive mi?
- **Seçenekler:**
  1. Fişsiz PDF veya sahte asset (uygulamada sabit PDF) — **red** (LedgerEntry yok; TIMEOUT’u posted göstermek aynı yalan).
  2. **Kazanan:** Dekont = **var olan** çift kayıt görünümü (`correlationId`). Başarılı havale / yükle / QR öde → API `correlationId` → Flutter **Dekont ekranına gider** (kopyala + PDF). Hareket satırı «Dekont» = aynı fiş. 409 replay → mevcut fiş. Gateway **202 TIMEOUT** → ledger yok, fiş/PDF yok. PDF T-079 durur: `GET /api/receipts/{id}/pdf` (JWT) + Razor `?handler=Pdf`; QuestPDF `ReceiptDto` → byte[]. Development örnek T-079 `DemoReceiptSeeder` (`aaaaaaaa-bbbb-4ccc-8ddd-eeeeeeee0001`). Footer: Demo — sahte banka gateway; banka resmi dekontu değil. YK markası yok.
  3. 9. ekran / Papara resmi dekont / `UPDATE Balance` — **red**.
- **Karar:** **2.** TASK-16 Todo durur. SPEC ekran 7 (`/dekont/{id}` + Flutter Dekont) aynı. T-079 web PDF yeniden açılmaz.
- **Neden:** 1 kullanıcı isteğini yalanlar. 3 SPEC. 2 mülakat: PDF ≠ kasa; HTML/JSON/PDF aynı SQL çifti.
- **Sonra hangi dosya:** Coder Flutter `clearpay_client`, `receipt_screen`, havale/yükle başarı, `platform/receipt_pdf*`, mobil README. Payments: PDF footer metni (T-079 renderer; tutar uydurma yok). Tester `WalletApiTests` transfer→JSON+PDF (T-079 landed). HANDOFF **append**.

---

## T-081 — 2026-08-17 — Dekont PDF native yok (QuestPDF disk)

- **Kim:** Coder (T-079 uygulama; MSB3027 «not enough space» QuestPDF Skia runtimes)
- **Konu:** T-079 QuestPDF seçilmişti. 2026.7.3 osx/linux/win-arm native kopyası C: dolu. Fişsiz PDF’e dönmek mi?
- **Seçenekler:**
  1. QuestPDF durur — derleme bu PC’de kırılır.
  2. **Kazanan:** Yönetilen PDF 1.4 (`SimplePdfReceiptRenderer`). `ReceiptDto` aynı. Native Skia yok. Tutar uydurma yok. T-079 path’ler durur.
  3. PDF iptal — kullanıcı PDF istedi.
- **Karar:** **2.** TASK-16 Todo. 9. ekran yok.
- **Neden:** 1 bu makinede kilit. 3 T-079’u bozar. 2 aynı SQL fiş, derlenir.
- **Sonra hangi dosya:** Infrastructure `Documents/SimplePdfReceiptRenderer.cs`. QuestPDF paket yok. HANDOFF append.

---

## T-082 — 2026-08-17 — Mobil hata (runtime kanıt; yama yok)

- **Kim:** Coder (kullanıcı: mobil tarafında bir hata var)
- **Konu:** Flutter Android/Windows JWT istemcisi. Tahminle yama yok. Önceki T-074 log’u web-ağırlıklı; native Firebase JNI, login `rethrow`, özet yükleme, SignalR, disk dolu (errno 112) henüz ayrışmadı.
- **Seçenekler:**
  1. Kör yama (Firebase stub, login catch-all, overflow) — **red** (kanıtsız).
  2. **Kazanan:** Hipotez log’u (`pre-firebase` / `firebase ok|catch`, FlutterError, login ok/fail, wallet load). Kanıt sonrası tek yama. 9. ekran yok. Firestore kasa yok. `UPDATE Balance` yok.
  3. Mobil iptal — kullanıcı hata istedi.
- **Karar:** **2.** TASK-16 Todo durur. T-065 stub kararı ancak native crash log’u gelirse yeniden açılır.
- **Neden:** Dart `catch` JNI crash yakalamaz; login `rethrow` kırmızı ekran üretebilir; `flutter test` errno 112 disk. Hangisi olduğunu log ayırır.
- **Sonra hangi dosya:** Coder `mobile/clearpay/lib/main.dart`, `firebase/bootstrap.dart`, `login_screen.dart`, `overview_screen.dart`. HANDOFF **append**.

---

## T-083 — 2026-08-17 — Mobil errno 112: C: disk dolu (Temp)

- **Kim:** Coder (kullanıcı: onu çöz — T-082 disk hipotezi)
- **Konu:** `flutter test` / derleme `FileSystemException writeFrom` errno 112. C: ~0.26 GB boş. Ledger/`UPDATE Balance` ile ilgisi yok. Firebase stub’a kör dönüş yok.
- **Seçenekler:**
  1. Kör Firebase/login yaması — **red** (disk kanıtı; uygulama kodu değil).
  2. **Kazanan:** `%LOCALAPPDATA%\Temp` Flutter/VS artıklarını sil (flutter_tools, dart kernel dirs, vscode installer). Flutter `TEMP`/`TMP` = `D:\ClearPay\tmp` (C: tekrar dolmasın). 8 ekran. Firestore kasa yok.
  3. Gradle/Android SDK sil — **red** (yeniden indirme C:’yi yine doldurur).
- **Karar:** **2.** TASK-16 Todo durur. T-082 log’ları durur (başka mobil hata kanıtı için).
- **Neden:** SO: disk dolu = errno 112 ([stackoverflow.com/questions/77871312](https://stackoverflow.com/questions/77871312/flutter-unhandled-exception), [71716920](https://stackoverflow.com/questions/71716920/flutter-build-failed-in-11s-unhandled-exception-filesystemexception-writefro)). Temp ~5.6 GB; silince C: ~4.3 GB.
- **Sonra hangi dosya:** Coder ortam (Temp silme; `D:\ClearPay\tmp`). `src/` yok. HANDOFF **append**.

---

## T-085 — 2026-08-17 — ClearPay C: → D: (boş disk)

- **Kim:** Coder / Deploy (kullanıcı: C’deki bu proje boş disklere taşınsın)
- **Konu:** C: ~4 GB boş, D: ~940 GB, E: ~519 GB. Repo `C:\Users\clt\Projects\clearpay` (~1.9 GB). Ledger LocalDB `C:\Users\clt\ClearPay.mdf` + `_log.ldf`. Android SDK / kullanıcı `.gradle` **bu proje değil** (paylaşılan araç).
- **Seçenekler:**
  1. Yalnız kopyala, C: aslı kalsın — C: dolmaz.
  2. **Kazanan:** Repo `D:\ClearPay\clearpay`. Eski yol **junction** (`C:\Users\clt\Projects\clearpay` → D:). LocalDB dosyaları `D:\ClearPay\data\mssql\` (`MODIFY FILE`; connection string `(localdb)\MSSQLLocalDB;Database=ClearPay` durur). `TEMP` zaten `D:\ClearPay\tmp`. Android SDK / `.gradle` taşınmaz. E: yedek boşluk. `UPDATE Balance` yok. 8 ekran.
  3. E:’ye taşı / SDK’yı da taşı — D: daha boş; SDK tüm Flutter işleri.
- **Karar:** **2.** TASK-16 Todo durur. T-076 LocalDB aynı instance; yalnızca physical path.
- **Neden:** Kullanıcı C: boşalsın istedi. Junction VS/Cursor eski yolu kırmaz. Ledger SQL Server’da kalır (T-021).
- **Sonra hangi dosya:** Deploy ortam. `appsettings` connection string **değişmez**. HANDOFF **append**.

---

## T-084 — 2026-08-17 — Flutter launcher icon (navy C)

- **Kim:** Coder (Flutter) + Designer (hafif)
- **Konu:** Emülatör hâlâ varsayılan Flutter robotu. ClearPay uygulama ikonu ne olsun? 9. ekran / `src/` ledger yok. (T-082 = mobil hata log; T-083 = C: disk; bu madde ikon.)
- **Seçenekler:**
  1. Flutter varsayılanı durur.
  2. **Kazanan:** Launcher = navy `#1B2A4A` zemin, beyaz geometrik **C** (ince teal halka). TASARIM wordmark (teal kare + C) splash’te halka+C olarak aynı glif. Android mipmap + adaptive; iOS AppIcon; Windows `app_icon.ico`. Kaynak `mobile/clearpay/assets/brand/`. YK/Papara/World yok.
  3. Cüzdan fotoğrafı / yazı «ClearPay» ikonda — 48dp’de okunmaz.
- **Karar:** **2.** TASK-16 Todo durur. 9. ekran yok. `UPDATE Balance` yok.
- **Neden:** 1 demo markası yok. 3 küçük boyutta bulanık. 2 MARKA navy + mevcut C glifi; adaptive güvenli bölge.
- **Sonra hangi dosya:** Coder OWN `mobile/clearpay/android/**/mipmap-*`, `mipmap-anydpi-v26`, iOS `AppIcon.appiconset`, `windows/runner/resources/app_icon.ico`, `assets/brand/`, splash `BrandMark`. Designer `docs/TASARIM.md` Wordmark bir satır. HANDOFF append.

---

## T-086 — 2026-08-17 — Flutter şifre kurtarma + telefon + Firebase Auth (web yok)

- **Kim:** Coder (kullanıcı: şifremi unuttum her girişte; telefon alınsın; yalnız mobil; kayıtlar Flutter Firebase)
- **Konu:** Mobil unutulan şifre / telefon / Firebase Auth. Web Razor giriş-kayıt durur mu? Firestore ikinci kasa mı?
- **Seçenekler:**
  1. Web + Flutter Razor `/sifre-sifirla` + 9. ekran — **red** (kullanıcı: mobil uygulama için yap, web için yapma; SPEC 8).
  2. Firestore / Firebase-only kullanıcı, SQL cüzdan yok — **red** (havale kırılır; T-061/T-065 ikinci kasa).
  3. **Kazanan:** Flutter kayıt/giriş kimliği = **Firebase Auth** (`createUser` / `signIn` / e-posta `sendPasswordResetEmail`). ID token → `POST /api/token/firebase` → SQL `ApplicationUser` provision/link + aynı ledger JWT. Telefon Identity `PhoneNumber` (yeni kayıtta zorunlu; seed admin `5550000001`). Unutulan şifre **yalnız Flutter** (e-posta + TC sekmeleri). SMS fatura yok; telefon sıfırlama = Identity token **log** (Production UI’da token yok) — Firebase yoksa aynı fallback. Razor `*.cshtml` / `/giris` **değişmez**. `POST /api/register` + `/api/token` test/web durur. Mernis / banka OTP / FAST iddiası yok.
- **Karar:** **3.** TASK-16 Todo durur. 9. ekran yok. `UPDATE Balance` yok. Web forgot-password **park**.
- **Neden:** Kullanıcı web UI istemedi. 2 SQL defteri koparır. 3 T-065 `firebase_core` üzerine Auth ekler; kasa SQL kalır. Halil Ads/Azure açmaz; `google-services.json` zaten T-065.
- **Sonra hangi dosya:** Coder `src/ClearPay.Web/Controllers` (`PasswordController`, `TokenController` firebase), Application port `IFirebaseIdTokenVerifier` + `IAccountMailer`, Identity seeder telefon, Flutter `login`/`register`/`forgot` + `firebase_auth`. Razor Account sayfaları **yok**. HANDOFF **append**.

---

## T-087 — 2026-08-17 — Flutter web yok; yalnız mobil (Razor site durur)

- **Kim:** Orchestrator + Coder (kullanıcı: Flutter web geliştirme durdur; Flutter yalnız mobil)
- **Konu:** T-076/T-080 `flutter run -d chrome` JWT istemci. Kullanıcı web’i Razor’da istedi; Flutter telefon. PWA/9. ekran mı, yoksa platform silme mi?
- **Seçenekler:**
  1. Flutter Chrome + Razor iki web — **red** (kullanıcı; T-073 PWA zaten red; iki istemci aynı tarayıcıda kafa karıştırır).
  2. **Kazanan:** Flutter **web platformu silinir** (`mobile/clearpay/web/**`). Site = Razor (`:5153`). Flutter = Android/iOS (+ mevcut Windows masaüstü JWT, T-062). `*_web.dart` koşullu import stub kalır (VM/Android `dart:io`). 8 ekran. Hive/Firestore kasa yok.
  3. Windows’u da sil — **park** (bu madde Chrome; Windows T-062 lokal JWT).
- **Karar:** **2.** TASK-16 Todo durur. T-080 Chrome kararı **yeni olgu ile** kapanır. `UPDATE Balance` yok.
- **Neden:** 1 isteği tersine çevirir. 3 bu cümlede yok. 2 tek web yüzeyi Razor; telefon JWT.
- **Sonra hangi dosya:** Coder `mobile/clearpay/web` sil; `.metadata`; README/README mobil; `.cursor/rules/flutter.mdc`. HANDOFF **append**. `src/` Razor yok.

---

## T-088 — 2026-08-17 — Flutter + web kalan iş; kimlik/dil paritesi dilimi

- **Kim:** Orchestrator (kullanıcı: hem Flutter hem web, tartışma yarat, geliştirmeye devam)
- **Konu:** TASK-01…15 Done. TASK-16 Azure URL **blok Halil**. 8 ekran kilit. İki yüzeyde ne kaldı? 9. ekran / Flutter Chrome geri / satıcı / gerçek FAST / web şifre unuttum mu?

**Envanter (bu madde kilitler; aynı kararı yeni olgu yokken yeniden açma):**

| Madde | Durum |
|-------|--------|
| Azure App Service URL | TASK-16 — Halil `az login` |
| Flutter `web/` / Chrome JWT | T-087 **red** — site = Razor |
| Web şifre unuttum | T-086 **park** |
| Web QR / FAST ızgara | **park** — T-067 telefon; TASARIM özet 2×2 |
| Pull-to-refresh | T-061 Q2.1 **park** |
| Satıcı paneli / OAuth Flutter | Q2 / Halil secret **park** |
| Web kayıt telefon + Bireysel/Kurumsal | **bu dilim** — SQL kolon var, Razor form yok |
| Web giriş TC (demo seed) | **bu dilim** — T-067 yalnız Flutter’daydı; ekran 1 |
| Flutter TR/EN/DE/FR | **bu dilim** — T-040 layout chrome; cookie değil, yerel dosya |

- **Seçenekler:**
  1. Kod yok; yalnız TASK-16 bekle — kullanıcı «devam» dedi.
  2. **Kazanan:** Envanter + tek dilim: **aynı Identity satırı, iki yüzey.** Razor kayıt: telefon zorunlu + AccountKind. Razor giriş: E-posta \| TC demo (`10000000146` → `admin@clearpay.test`; Mernis yok). Flutter: aynı 4 dil seçici (auth + çekmece). API `/api/register` telefon **isteğe bağlı** kalır (mevcut JWT testleri). 8 ekran. `UPDATE Balance` yok. Hive/Firestore kasa yok.
  3. 9. ekran / PWA / Flutter web geri / TCMB FAST / web forgot — **red**.
- **Karar:** **2.** TASK-16 Todo durur.
- **Neden:** Web kayıtsız telefon Flutter kurtarmayı boş bırakır. TC yalnız telefonda mülakat tutarsız. Dil web’de var, Flutter’da yok (T-040 chrome, 9. ekran değil). 1 kullanıcı isteği değil. 3 SPEC.
- **Sonra hangi dosya:** Coder Razor Login/Register + `LoginRequest` + `DemoTc` + resx 4 dil + `site.css` `.auth-tabs` + testler. Coder Flutter `lib/l10n/**` + locale store. Designer TASARIM giriş/kayıt bir satır. SPEC ekran 1–2 alan notu. TASKS Done UI satırı. HANDOFF **append**.

---

## T-089 — 2026-08-17 — C: junction: debug log D: (T-085 kilidi)

- **Kim:** Coder (kullanıcı proceed; T-085 junction bitmedi)
- **Konu:** `ClearPay.Web` `AgentDebugLog` `C:\Users\clt\Projects\clearpay\debug-021de0.log` yazıyor; klasör silinemez. Ledger D: ONLINE. Kör hub yaması yok.
- **Seçenekler:**
  1. C: stub kalsın — kullanıcı D: istedi.
  2. **Kazanan:** Log yolu workspace `D:\ClearPay\clearpay\debug-021de0.log`. Host durdur → junction `C:\Users\clt\Projects\clearpay` → D:. Instrumentation durur. `UPDATE Balance` yok. 8 ekran.
  3. Debug log’u sil — **red** (oturum 021de0 kanıtı).
- **Karar:** **2.** TASK-16 Todo durur.
- **Neden:** Log satırı: host pid 19068 C: dosyasını açık tutuyor. Junction ancak handle kapanınca.
- **Sonra hangi dosya:** Coder `AgentDebugLog.cs` Path. HANDOFF **append**.

---

## T-090 — 2026-08-17 — TR/EN/DE/FR gerçekten UI değiştirir (web + Flutter)

- **Kim:** Coder (kullanıcı: dil şeridine basınca arayüz değişmiyor; hem mobil hem web)
- **Konu:** T-088 Flutter LanguageStrip + LocaleStore vardı; `L()` yalnız auth/çekmece kromunda. Özet/havale/yükle/hareket/dekont/admin/splash/QR park Türkçe sabit — English/Deutsch/Français no-op gibi duruyor. Web cookie `c=` + `/culture` (SetCulture) T-040 duruyor mu? 9. ekran / 5. dil / web forgot / Flutter Chrome geri mi?
- **Seçenekler:**
  1. Yalnız şerit görünsün, metin TR kalsın — kullanıcı «çalışsın» dedi.
  2. **Kazanan:** Aynı 4 dil **iki yüzeyde UI’yı sürer** (layout chrome, 9. ekran değil). Web: mevcut picker + cookie `c=` + SharedResource; kırıkysa tamir, yeniden tasarım yok. Flutter: `LocaleStore` + `LocaleScope` snapshot (`code`) + `L()` **8 işlem + splash/mode/auth/shell/QR park**. Dil şeridi auth + çekmece + girişli AppBar (çekmece açmadan). Tap → `save` + `onChanged` → `MaterialApp.locale` ve tüm `L()` yenilenir. 5. dil yok. Web forgot T-086 park. Flutter `web/` T-087 red.
  3. Kullanıcı hesabı dilini SQL’de tut / sunucu locale Flutter’a dayat — **red** (T-088 yerel dosya; cookie web).
- **Karar:** **2.** TASK-16 Todo durur. `UPDATE Balance` yok. 8 ekran.
- **Neden:** 1 şikâyeti bırakır. 3 ikinci kasa/hesap tercihi. 2 SPEC 4 dil + T-040 chrome; T-088 şeridi doldurur.
- **Sonra hangi dosya:** Coder Flutter `lib/l10n/app_strings.dart` + 8 ekran dart; gerekirse Razor SetCulture/picker (tasarım yok). Tester `widget_test` DE/FR + LocalizationTests kayıt kromu. SPEC bir satır. TASKS Done UI (TASK-16 değil). HANDOFF **append**.

---

## T-091 — 2026-08-17 — Flutter `cloud_firestore` yalnız meta ping (kasa SQL durur)

- **Kim:** Orchestrator + Coder (kullanıcı: Firebase’de veritabanı oluşturdu; Flutter tarafını yap)
- **Konu:** Console’da Firestore (`clearpay-c0485`) açıldı. Ledger’ı oraya taşımak mı, yoksa JWT/SQL kasayı koruyup Firestore’u para-dışı kullanmak mı?
- **Seçenekler:**
  1. Ledger / bakiye / havale Firestore’a — **red** (SPEC + T-061 + T-065 ikinci kasa; `UPDATE Balance` ikizi; Hive/SQLite cüzdan yok).
  2. **Kazanan:** `cloud_firestore` istemci **yalnız para-dışı** (`app_meta/ping` veya demo bayrak). JWT → ASP.NET → SQL Server/LocalDB değişmez. Firebase Auth ikinci Identity deposu değil (T-086 ID token hâlâ SQL provision). 8 ekran. 9. ekran yok. Init atlanırsa ping de atlanır. Ping hata verirse JWT girişi durmaz (fail-open log). Tutar / ledger / şifre Firestore’a yazılmaz.
  3. Firestore paketini atla — **red** (kullanıcı DB sonrası Flutter işi istedi; 2 konsolu gerçekten kullanır).
- **Karar:** **2.** TASK-16 Todo durur. Domain’e Dart yok. `ClearPay.slnx` Flutter içermez. Windows native Firebase C++ skip (T-075) durur.
- **Neden:** 1 SQL/409 omurgasını kırar. 3 isteği karşılamaz. 2 konsoldaki Firestore’u meta için bağlar; kasa C# defter.
- **Sonra hangi dosya:** Coder `mobile/clearpay` (`pubspec` `cloud_firestore`, `lib/firebase/bootstrap.dart`, test, README bir cümle). HANDOFF **append**. `src/` yok. Firestore rules Halil console (para yazımını public açma).

---

## T-092 — 2026-08-17 — Özet reload döngüsü + Windows hub HttpError

- **Kim:** Coder (kullanıcı proceed; log 021de0)
- **Konu:** Giriş sonrası `Index.OnGet` + `WalletHub` saniyeler içinde tekrar. `site.js` `WalletChanged` → `location.reload`. Flutter Windows `hub connect ok:false HttpError host=localhost`; Android `10.0.2.2` ok. `signalr_netcore` `requestTimeout` varsayılan **2000ms**. 9. ekran yok.
- **Seçenekler:**
  1. Kör CORS / Firebase stub — **red** (log başka şey diyor).
  2. **Kazanan:** Web: yüklemeden sonra 4s `WalletChanged` yok say + 4s debounce (döngü kesilir; canlı yenileme durur, F5 durur). Flutter hub: `requestTimeout` 15s; Windows `LongPolling` (WebSocket 2s HttpError). Instrumentation durur. `UPDATE Balance` yok.
  3. SignalR kaldır — T-071 chrome kaybı.
- **Karar:** **2.** TASK-16 Todo durur.
- **Neden:** Log: cookie `ok:true` sonra overview/hub tekrar (2323075→2326158). Windows HttpError; Android hub ok. Paket timeout 2000ms Windows’u keser.
- **Sonra hangi dosya:** Coder `wwwroot/js/site.js`, `wallet_live_hub.dart`. HANDOFF **append**.

---

## T-093 — 2026-08-17 — Özet reload: 4s skip yetmedi (location.reload yok)

- **Kim:** Coder (kullanıcı proceed; T-092 doğrulama)
- **Konu:** T-092 4s skip. Kestrel kanıt: aynı oturumda **üç** `Wallet`/`LedgerEntry` okuma (Index OnGet ×3). Döngü 4s sonra da sürebilir (Hangfire outbox). Kör CORS yok.
- **Seçenekler:**
  1. Skip süresini 60s yap — hâlâ reload; gecikmeli döngü.
  2. **Kazanan:** `WalletChanged` **sayfa yenilemez** (Flutter `liveTick` gibi). Log durur. Kullanıcı F5. T-071 chrome: ipucu yok = F5; döngü yok. `UPDATE Balance` yok. 8 ekran.
  3. SignalR kaldır — T-071 tamamen gider.
- **Karar:** **2.** TASK-16 Todo durur. T-092 Flutter timeout/LongPolling durur.
- **Neden:** 1 Kestrel’de 3× özet. 3 fazla. 2 döngüyü keser; para SQL’de.
- **Sonra hangi dosya:** Coder `wwwroot/js/site.js` (`location.reload` yok). HANDOFF **append**.

---

## T-096 — 2026-08-17 — ILinkedInstrumentStore.AddAsync Scheme (derleme)

- **Kim:** Coder (site `:5153` düştü; CS0535)
- **Konu:** Kartlar T-094 `SqlLinkedInstrumentStore.AddAsync(..., scheme, ...)`. Application port Scheme yok. Yükle/Çek 4 arg. Kör ledger yok.
- **Seçenekler:**
  1. Store’dan scheme sil — Kartlar BIN kırılır.
  2. **Kazanan:** Port `string? scheme = null`. Yükle/Çek ve API `null` geçer. 8+Kartlar. `UPDATE Balance` yok.
- **Karar:** **2.** TASK-16 Todo. Site D: yeniden ayağa.
- **Neden:** CS0535 host’u kesiyor. T-094 Scheme kararı durur.
- **Sonra hangi dosya:** Application `ILinkedInstrumentStore`. Coder `YukleCek`, `CardsController`. HANDOFF **append**.

---

## T-097 — 2026-08-17 — Kartlarım (`/kartlar`) + canlı önizleme (kullanıcı istedi; T-094 numara hub’a ait)

- **Kim:** Orchestrator + Architect + Coder + Designer + Tester (kullanıcı: kart bağla, Yapı Kredi örneği, yazarken canlı kart, karttan cüzdana yükle)
- **Konu:** T-055 kayıtlı kartı **Yükle/Çek paneli** yaptı (9. ekran yok, PAN yok). Kullanıcı şimdi ayrı alan + 16 hane önizleme istiyor. SPEC 8 kilit kullanıcı isteğiyle güncellenebilir. Gerçek POS/3DS/YK API yok.
- **Seçenekler:**
  1. Gerçek Visa/iyzico/YK API / 3DS — hayır; lisans, sır, MARKA.
  2. T-055 durur (yalnız son 4, `/kartlar` yok) — kullanıcı reddi.
  3. **Kazanan:** Yeni Razor `/kartlar` (ekran 9, kullanıcı istedi). Liste: şema + son 4 + kart adı. Form: numara/ad/SKT/CVV **canlı CSS 3D önizleme** (yazdıkça). Kayıt: `LinkedInstrument` son 4 + Label + Scheme (Visa/Mastercard/Troy BIN; Yapı Kredi = takma ad, şema değil). PAN/CVV SQL/git/log yok; CVV `name` yok (POST edilmez). «Bu karttan cüzdana yükle» → mevcut `/yukle-cek?kart={last4}` + `IFundingExecutor` / sahte `IBankGateway`. PageModel ledger yok. Flutter ekran **park** (JWT `/api/cards` durur; 3D web’e özgü).
- **Karar:** **3.** T-055 store/port durur; ek ekran SPEC’de 9. TASK-16 Todo. `UPDATE Balance` yok. Papara kopyası yok.
- **Neden:** 1 kapsam dışı. 2 kullanıcı isteğini karşılamaz. 3 önizlemeye yer; para hâlâ tek kasa + sahte gateway.
- **Sonra hangi dosya:** Coder `Pages/Kartlar.cshtml(.cs)`, layout nav, `brand.css` + `card-preview.js`, `ILinkedInstrumentStore` Scheme, EF `Scheme` kolonu, resx, Yükle/Çek bağla linki. Designer `docs/TASARIM.md`. SPEC ekran 9. Tester `/kartlar` 200 + anon `/giris` + HTML önizleme. HANDOFF append. Flutter `*.dart` yok.

---


## T-094 — 2026-08-17 — localhost hub HttpError (LongPolling yanlış platform)

- **Kim:** Coder (log: `hub connect ok:false HttpError host=localhost windowsLongPoll:false`)
- **Konu:** T-092 LongPolling yalnız `TargetPlatform.windows`. Kanıt: yeni alan `windowsLongPoll:false` + `host:localhost` hâlâ HttpError. Android `10.0.2.2` WebSocket **ok**. Timeout 15s yetmedi. Debug dosya hâlâ `C:\Users\clt\Projects\clearpay`.
- **Seçenekler:**
  1. Yalnız Windows enum — **red** (log false).
  2. **Kazanan:** Hub LongPolling **Android değilse** (`!isAndroidHost`); Android WebSocket. Debug append `D:\ClearPay\clearpay\debug-021de0.log`. T-093 reload yok durur. `UPDATE Balance` yok.
  3. Hub iptal — T-071 kaybı.
- **Karar:** **2.** TASK-16 Todo durur.
- **Neden:** Başarılı hub hep `10.0.2.2`; başarısız hep `localhost` + WebSocket.
- **Sonra hangi dosya:** Coder `wallet_live_hub.dart`, `debug_file_io.dart`. HANDOFF **append**.

---

## T-095 — 2026-08-17 — Azure canlı web ayarları (hesap açılmaz)

- **Kim:** Orchestrator + Deploy + Coder (kullanıcı Azure açıp siteyi canlıya alacak; bilgileri paylaşacak)
- **Konu:** TASK-16 URL Halil tıklar. Ajan `az login` / abonelik / DNS açmaz. Web tarafında Production’ın App Service arkasında cookie/HTTPS/CORS/JWT/SQL ile ayağa kalkması.
- **Seçenekler:**
  1. Yalnız `docs/CANLI.md` tık listesi; kod aynı — **zayıf** (X-Forwarded-Proto yoksa Secure cookie / yönlendirme kırılır; CORS yalnız `clearpay.azurewebsites.net`; F1 restart’ta DataProtection kaybolursa oturum düşer).
  2. **Kazanan:** Forwarded headers + Production DataProtection (`HOME/data-protection-keys`) + cookie SameSite Lax; Bicep/deploy.ps1 gerçek host’u `Cors__Origins__0` yapar; Identity/ledger hâlâ `ConnectionStrings:ClearPay` (Azure SQL). Yeni tablo yok. 8 ekran. `UPDATE Balance` yok. URL uydurulmaz. Secret git’e yok.
  3. DataProtection keys’i SQL tabloya — F1 tek instance; şema şimdi şart değil (sonra scale).
- **Karar:** **2.** TASK-16 Todo durur (açık URL Halil).
- **Neden:** 1 canlıda sessiz giriş kırığı. 3 şema; Q1 F1’de `/home` kalıcı. 2 SPEC + CANLI isimleri.
- **Sonra hangi dosya:** Coder `Program.cs` + Azure hosting; Identity cookie; Deploy `infra/main.bicep` + `deploy.ps1` + `docs/CANLI.md`. HANDOFF **append**.

---

## T-096 — 2026-08-17 — Flutter Firestore ping’i ekranda kanıtla (kasa SQL)

- **Kim:** Coder (kullanıcı: Flutter’da Firebase’e veri ekle, çalışıyor mu göster)
- **Konu:** T-091 `app_meta/ping` sessiz. Windows native Firebase C++ skip (T-075). Kullanıcı görünür yazma istiyor. Ledger Firestore’a taşınmaz.
- **Seçenekler:**
  1. Bakiye/havale Firestore’a — **red** (T-091 / SPEC; ikinci kasa).
  2. **Kazanan:** Mevcut giriş ekranında ping sonucu (yazıldı / atlandı / hata). Payload `ok` + `client` + `message` + `touchedAt` — tutar/şifre yok. Rules yalnız `app_meta/ping` (para koleksiyonu deny). 9. ekran yok. JWT/SQL durur. Init fail → giriş durmaz.
  3. Yeni Firebase ekranı — SPEC 8/9 şişirme.
- **Karar:** **2.** TASK-16 Todo durur.
- **Neden:** 1 kasa kırar. 3 ekran ekler. 2 konsolda belge + telefonda satır.
- **Sonra hangi dosya:** Coder `mobile/clearpay` (`bootstrap.dart`, `login_screen.dart`, `firestore.rules`). HANDOFF **append**. `src/` yok.

---

## T-098 — 2026-08-17 — Flutter Windows canlı: SignalR yok, JWT poll

- **Kim:** Coder (kullanıcı: bu kısım çalışmıyor, farklı yol)
- **Konu:** `signalr_netcore` localhost `HttpError` (T-092/T-094 LongPolling). Android hub `10.0.2.2` ok. Kör CORS yok. Hive bakiye yok.
- **Seçenekler:**
  1. SignalR transport daha — **red** (kullanıcı farklı yol).
  2. **Kazanan:** Android: SignalR durur. Windows/masaüstü: hub **bağlanmaz**; `Timer` 8s → `liveTick` → `GET /api/wallet` (mevcut JWT). Çek-yenile durur. 8 ekran. `UPDATE Balance` yok.
  3. Canlı yenileme iptal — T-071 tamamen gider; poll T-071’i REST ile tutar.
- **Karar:** **2.** TASK-16 Todo durur.
- **Neden:** 1 aynı HttpError. 3 kullanıcı canlı ister. 2 kanıtlı REST; kasa SQL.
- **Sonra hangi dosya:** Coder `wallet_live_hub.dart`. HANDOFF **append**.

---

## T-099 — 2026-08-17 — README mülakat yüzeyi + push

- **Kim:** Orchestrator (kullanıcı: bütün değişiklikleri push et, güzel README)
- **Konu:** GitHub `README.md` hâlâ “sekiz işlem”; SPEC ekran 9 Kartlarım (web). Firestore ping kanıtlandı, kasa SQL. Azure URL yok.
- **Seçenekler:**
  1. README’ye dokunmadan push — **zayıf** (yanlış ekran sayısı).
  2. **Kazanan:** İngilizce + Türkçe README: demo disclaimer, iki istemci bir SQL, 409 / `LedgerPair.NetOf`, Kartlarım web, Flutter JWT, Firestore `app_meta/ping` değil kasa, TASK-16 tık Halil. DE/FR giriş aynı doğruluk. Secret git’e yok.
  3. Dört dili silip tek README — T-040 chrome kaybı.
- **Karar:** **2.** TASK-16 Todo durur. `az login` ajan yapmaz. URL uydurulmaz.
- **Neden:** 1 mülakat yüzeyi yanlış. 3 dil şalterini kırar. 2 SPEC + kanıt.
- **Sonra hangi dosya:** `README.md`, `README.tr.md` (+ DE/FR giriş). HANDOFF **append**. Push kullanıcı istedi.

---

## T-101 — 2026-08-17 — GitHub README SVG görünmüyor

- **Kim:** Coder (kullanıcı: GitHub README resimleri çıkmamış)
- **Konu:** `docs/assets/*.svg` XML 1.0 yasak kontrol karakterleri (`U+0012` / `U+0014`) içeriyor; GitHub sanitizer dosyayı boş bırakır. Markdown `![]()` SVG Camo’da da kırılır.
- **Seçenekler:**
  1. Yolu `raw.githubusercontent.com` yapmak — **zayıf** (aynı bozuk SVG).
  2. **Kazanan:** SVG’yi geçerli UTF-8 yap; README `<img>` + PNG yedek (GitHub Camo PNG’yi gösterir). Relative `docs/assets/`. Secret yok. 8/9 ekran metni durur.
  3. Resimleri sil — mülakat diyagramı kaybı.
- **Karar:** **2.** TASK-16 Todo durur.
- **Neden:** 1 kök nedeni bırakır. 3 kullanıcı isteğine aykırı.
- **Sonra hangi dosya:** `docs/assets/*`, `README.md` / `README.tr.md` / DE / FR. HANDOFF **append**. Push GitHub yüzeyi için.

---

## T-100 — 2026-08-17 — web + Flutter Android manuel ve otomasyon testi

- **Kim:** Orchestrator + Tester + Coder + Deploy (kullanıcı: her yüzey teste tabi; manuel + otomasyon; web ve Flutter Android)
- **Konu:** Mevcut xUnit (cookie/JWT/409/ledger) ve `widget_test` dağınık. CI yalnız `dotnet test`. Android emülatör JWT + hub elle doğrulanmıyor. 10. ekran / ikinci kasa yok.
- **Seçenekler:**
  1. Yalnız mevcut testlere güven — **zayıf** (Flutter CI yok; manuel checklist yok; Android `10.0.2.2` / hub vs Windows poll yazılı değil).
  2. Appium / Maestro cihaz farm — kapsam şişer; emülatör hesabı; Q1 değil.
  3. **Kazanan:** Manuel `docs/SMOKE.md` (Razor 9 ekran + Flutter Android JWT). Otomasyon: Tester `DualSurfaceSmokeTests` (cookie TC + JWT `/api/token`+`/api/wallet` + hub negotiate + `site.js` reload yok + `Wallet.Balance` yok). Coder `flutter test` (Android taban URL, hub skip/poll, TC→JWT, 409). Deploy CI `flutter test` job. SPEC ekran listesi durur. `UPDATE Balance` yok. Hive/Firestore kasa yok. TASK-16 Todo.
- **Karar:** **3.** TASK-16 durur. `az login` ajan yapmaz. Flutter Chrome yok (T-087).
- **Neden:** 1 kullanıcı isteğini karşılamaz. 2 cihaz farm / yeni ürün yüzeyi. 3 landed sözleşmeyi kilitler; para hâlâ tek SQL.
- **Sonra hangi dosya:** Tester `tests/ClearPay.Tests/DualSurfaceSmokeTests.cs`, `docs/SMOKE.md`, `.cursor/rules/tester.mdc`. Coder `mobile/clearpay/lib/api/{clearpay_client,wallet_live_hub}.dart`, `mobile/clearpay/test/android_surface_test.dart`. Deploy `.github/workflows/ci.yml`. Orchestrator TASKS Done UI + HANDOFF **append**. Razor markup yok.

---

## T-101 — 2026-08-17 — Visa / Mastercard yüzü (ISO BIN + görünüm; web + Flutter)

- **Kim:** Orchestrator + Architect + Designer + Coder + Tester (kullanıcı: Mastercard veya Visa ayrımı nasıl yapılıyorsa öğren; kart görünümü ve numaraya göre hem mobil hem web)
- **Konu:** T-097 şema yazısı var; kart yüzü hep navy. Flutter Kartlarım park. ISO/IEC 7812 IIN (BIN) sektör standardı: Visa = `4…`; Mastercard = `51–55` veya `2221–2720` (2017 2-series). Troy `9792` durur. Resmi Visa/MC SVG yok (MARKA). PAN/CVV SQL yok. Gerçek POS/3DS yok.
- **Seçenekler:**
  1. Fotoğraftan OCR/ML şema — hayır; model, sır, 10. ekran.
  2. Yalnız metin “Visa/Mastercard” — kullanıcı görünüm istedi; zayıf.
  3. **Kazanan:** Aynı `CardNetwork.Detect` (C#) + Dart kopyası. Yazılan numaraya göre kart yüzü: Visa mavi + VISA yazısı; Mastercard koyu + iki örtüşen daire (geometrik, resmi logo dosyası yok); Troy teal. Web `/kartlar` + Yükle/Çek seçili kart. Flutter yeni Kartlarım sekmesi (SPEC 9; T-097 park kalkar). `POST /api/cards` isteğe `number` → parser last4+scheme, PAN kaydı yok. last4-only = Unknown. `IFundingExecutor` durur. `UPDATE Balance` yok.
- **Karar:** **3.** TASK-16 Todo durur. Flutter Chrome yok (T-087).
- **Neden:** 1 kapsam dışı. 2 görünümü karşılamaz. 3 sektör BIN + iki yüzey; kasa SQL.
- **Sonra hangi dosya:** Coder `CardNetwork` test, `CardsController`/`CardApiRequest`, `Kartlar.cshtml`, `YukleCek.cshtml`, `brand.css`, `card-preview.js`. Flutter `card_network.dart`, `live_payment_card.dart`, `cards_screen.dart`, shell/overview/l10n/client. Designer TASARIM §13. Tester parser + `/kartlar` Mastercard HTML + JWT scheme. HANDOFF **append**.

---

## T-102 — 2026-08-17 — README daha görsel (GitHub yüzeyi)

- **Kim:** Designer + Coder (kullanıcı: daha görsel README)
- **Konu:** README metin ağır; marka C + diyagram var, ürün yüzü yok. GitHub SVG kırığı T-101 ile PNG. Papara/YK kopya görsel yok. 10. ekran yok.
- **Seçenekler:**
  1. Yalnız mevcut iki diyagram — kullanıcı “daha görsel” dedi.
  2. **Kazanan:** PNG marka + hero + kural şeridi + iki istemci; gerçek `/giris` ekran görüntüsü (localhost). README EN/TR (DE/FR hero). Demo disclaimer durur. `UPDATE Balance` yok. URL uydurulmaz.
  3. Canva/Figma sahte banka mockup — MARKA red.
- **Karar:** **2.** TASK-16 Todo durur.
- **Neden:** 1 isteği karşılamaz. 3 marka. 2 GitHub’da görünen PNG + gerçek Razor.
- **Sonra hangi dosya:** `docs/assets/*`, `README.md` / `README.tr.md` (+ DE/FR üst). HANDOFF **append**.

---

## T-103 — 2026-08-17 — Visa / Mastercard yüzü numara düzeltmesi

- **Kim:** Orchestrator (T-101 README SVG’ye ait; Visa/MC karar aynı, numara çakıştı)
- **Konu:** ISO BIN kart yüzü web+Flutter. Yeni olgu: T-101 = GitHub SVG/PNG. Visa/MC işi **T-103**.
- **Seçenekler:**
  1. T-101 Visa bloğunu silmek — yasak (overwrite yok).
  2. **Kazanan:** Karar T-103; içerik bir üstteki Visa/MC kazanan 3 ile aynı.
- **Karar:** **2.** TASK-16 Todo durur.
- **Neden:** Numara çakışması; karar değişmedi.
- **Sonra hangi dosya:** Coder aynı OWN (Kartlar + Flutter Kartlarım). HANDOFF **append**.

---


