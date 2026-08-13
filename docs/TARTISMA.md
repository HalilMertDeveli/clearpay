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
