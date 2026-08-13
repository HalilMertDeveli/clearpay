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
