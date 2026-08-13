# İK — aday Halil (işe alım değil)

ClearPay **işveren değil**. Bu dosya **aday** (Halil) içindir: CV, 15/30 dakika script, 409 / transaction / outbox cevapları, hangi firmaya basılır.

Kaynak CV: [`README.md`](../README.md) *CV bullets (intended)* — kelime kelime. Pitch: [`SATIS.md`](SATIS.md). Fark: [`FARK.md`](FARK.md). Motor: [`SPEC.md`](SPEC.md). Neden: [`OGRENME.md`](OGRENME.md).

**Kilit:** Demo — sahte banka gateway. Lisanslı cüzdan değil. TASK-06 (409 HTTP) ve TASK-11 (outbox worker) bitmeden “kanıtladım” deme.

---

## CV bullets (English — README)

LinkedIn / CV / e-posta. Üç satır, README ile aynı:

- Built ClearPay, an ASP.NET Core 8 wallet with idempotent P2P transfers, JWT/cookie auth, and a double-entry ledger on SQL Server.
- Integrated a mock bank gateway over REST and SOAP; used an outbox + queue so payment completion is not lost on timeout.
- Shipped Docker Compose, xUnit tests, Serilog correlation, and CI/CD to Azure App Service.

**Şimdi (dürüst):** iskelet + Identity WIP; ledger Domain’de var; HTTP 409 = TASK-06; outbox worker = TASK-11; Azure URL = TASK-16. Mülakatta “kural kilitli, kod TASK-06/11’de kanıt” de. Bitince README cümlesi durur; abartı ekleme.

Kullanma: Papara clone, FAST integration, licensed e-money, production wallet, BDDK.

Headline (isteğe): `ASP.NET Core 8 · idempotent wallet demo · double-entry ledger`

Repo: [github.com/HalilMertDeveli/clearpay](https://github.com/HalilMertDeveli/clearpay) — public, `main`.

---

## 15 dakika

| Dk | Ne | Söyle |
|----|----|--------|
| 0–1 | Kim / ne | ClearPay: ASP.NET Core 8 **demo** cüzdan. Lisans yok; sahte banka. Amaç: kurumsal .NET mülakatında anlatılacak repo. |
| 1–4 | Stack | C# 12, .NET 8, Razor + API tek host, SQL Server, EF Core, Identity cookie (JWT API sonra), Docker Compose, xUnit, Serilog. |
| 4–10 | Üçlü | Aşağıdaki **409 / transaction / outbox**. Ezber değil: çift tıklama, kısmi commit, HTTP timeout. |
| 10–13 | Mimari | Clean Arch: Domain para kuralı, Web HTTP. Bakiye kolonu yok = ledger net. `UPDATE Balance` yok. |
| 13–15 | Dürüst kapanış | Papara rakibi değilim. Demo. Soru alın. |

Kesme cümlesi (15s, SATIS): *Defter sizin. Bakiye güncellendi demiyoruz. Demo.*

---

## 30 dakika

15 dakikanın üstüne:

| Dk | Ne | Söyle / göster |
|----|----|----------------|
| 15–20 | Ledger | Çift kayıt (`LedgerEntry` +/−, `PairId`). Bakiye = `NetOf`. İade = ters kayıt. Freeze: gönderemez / çekemez. Audit + correlation id. |
| 20–24 | Gateway | Sahte `BankGateway`: REST **ve** SOAP, **aynı sözleşme** (timeout dahil). Gerçek FAST/POS yok. |
| 24–27 | Demo | Mümkünse: kayıt → özet `0,00 ₺` → (TASK-06+) aynı havale iki kez → **409**. Footer: Demo — sahte banka gateway. |
| 27–30 | Soru | “Neden 200 replay değil?” → ikinci kesinti yok. “Neden Java yok?” → hedef .NET kapısı. “Ne zaman Azure?” → TASK-16, hesabı ben açarım. |

Canlı site yoksa GitHub + SPEC para kuralları. Uydurma ekran yok.

---

## 409 / transaction / outbox

Kod henüz HTTP’de yoksa bile cümle aynı (SPEC). “Yarın yazacağız” değil: **neden** bu üçü.

### Neden 409?

Aynı `Idempotency-Key` = aynı niyet (çift tıklama, proxy retry). İkinci **201** olursa cüzdan **iki kez** kesilir. **409 Conflict** = “bu niyet işlendi”; ikinci kesinti yok.

Neden 200 + aynı body değil? Tercih 409: istemci “yeni başarı” sanmaz. SPEC: `POST /api/transfers` → 201, tekrar → 409.

### Neden transaction?

Gönderen −, alıcı +, `Transfer`, `IdempotencyRecord`, `AuditLog`, `OutboxMessage` **tek SQL commit**. Biri ayrı commit olursa bakiye ile defter ayrılır. Bakiye `UPDATE Balance` ile düzeltilmez; ledger net’idir. Fail → hepsi rollback.

### Neden outbox?

Ledger yazıldı, HTTP timeout, client retry. Mesajı commit’ten **önce** kuyruğa atarsan “banka/kuyruk kayboldu” olur. Önce DB (ledger + outbox **aynı** transaction), **sonra** worker yayınlar (Hangfire, TASK-11). Timeout kaydı silmez; ikinci debit 409 ile durur.

| Soru | Tek cümle |
|------|-----------|
| 409 nedir? | Aynı anahtar = aynı niyet; tekrar 409, ikinci kesinti yok. |
| Transaction? | −/+ / idempotency / audit / outbox tek commit veya hiç. |
| Outbox? | Önce DB, sonra mesaj; timeout ödemeyi yutmaz. |
| Bakiye? | Ledger net; kolon güncellemesi yok. |

---

## Hangi firmalar

İlan **C# / .NET 8 / ASP.NET Core** ise ClearPay anlatılır. Java/Spring veya C++/embedded ise **bu repo ile girilmez**.

### Hedef (.NET)

SPEC kapısı — bunlar:

| Firma | Neden bu repo |
|-------|----------------|
| **Intertech** | Banka .NET; ledger / idempotency / outbox cümlesi |
| **Softtech** | Aynı: kurumsal .NET, para izi |
| **Bileşim** | Kart / ödeme altyapısı; çift kayıt + 409 |
| **TAV** / **İGA** | Kurumsal .NET (havalimanı BT); demo cüzdan = para disiplini, FIDS değil |
| **Üniversite BT** | Kampüs/iç bakiye sahnesi; lisans iddiası yok |
| **Sanayi** ERP/.NET | İç avans, freeze, audit |
| **Turkcell .NET** | İlan .NET ise; Paycell Java ilanına kayma |

Aynı kapı: banka yazılım evleri, holding BT, .NET ilanı açık diğer kurumlar. Stack ilanda yazıyorsa başvur.

### Hedef değil

| Firma / stack | Neden değil |
|---------------|-------------|
| **Trendyol** (Java) | SPEC: açılmaz. ClearPay C# kanıtı; Spring mülakatını taşımaz. |
| **THY / Garanti Java** | Java ilanı. .NET hikâyesi yanlış kapı. |
| **ASELSAN C++** | Embedded / C++; cüzdan demosu alakasız. |

İlan Java, Kotlin, Go, C++ ise CV’ye ClearPay “ödeme ürünü” diye yapıştırma. Papara / Tosla **tüketici iş ilanı** (lisanslı e-para) — sen demo anlatıyorsun; “Papara klonu yazdım” ile girme.

---

## Söyleme

- İşe alıyoruz / ClearPay’de açık pozisyon
- Papara alternatifi, BDDK, gerçek FAST/POS/IBAN
- “409’u production’da kanıtladım” (TASK-06/11 öncesi)
- Maaş bandı, başlık uydurma, rakip firma iç bilgi

Sales 15s pitch ve site copy: `SATIS.md`. İK script’i ezberletir; Sales pazar cümlesini yazar — çelişirse **demo + üçlü** kazanır.
