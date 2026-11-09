# CV — Halil Mert Develi (ClearPay maddesi)

Kaynak paket (bu ajanın daha önce ürettiği HTML): `C:\Users\clt\Desktop\Halil_Mert_Develi_CV_Paket`

| Klasör | Dosya |
|--------|--------|
| ASP.NET | `01_ASPNET_Core\TR_HalilMertDeveli_ASPNET_Core.html` · `EN_HalilMertDeveli_ASPNET_Core.html` |
| Banka / finans | `03_Banka\TR_HalilMertDeveli_Banka_Yazilim.html` · `EN_HalilMertDeveli_Banking_Software.html` |
| Mobil | `04_Mobil\TR_HalilMertDeveli_Mobil_Flutter.html` · `EN_HalilMertDeveli_Mobile_Flutter.html` |

IT sistemci CV’sine ClearPay **eklenmedi** (ilan AD/Veeam; cüzdan demosu o kapıya taşımaz).

İşveren tarihleri uydurulmadı. ClearPay **seçilmiş proje** (2026), Colorlight/ETG deneyiminin yerine geçmez.

**Kullanma:** lisanslı e-para, Papara rakibi, FAST/BDDK, “ödeme şirketi yayınladım”, canlı Azure 409 (TASK-16 URL yokken).

Repo: [github.com/HalilMertDeveli/clearpay](https://github.com/HalilMertDeveli/clearpay). Mülakat üçlüsü: [`IK.md`](IK.md). README İngilizce maddeler: kök [`README.md`](../README.md) *CV bullets (intended)*.

---

## Türkçe (01 / 03 paketleriyle aynı)

**ClearPay — ASP.NET Core 8 cüzdan demosu (2026)**  
ASP.NET Core 8, Clean Architecture, EF Core, SQL Server, Identity (cookie + JWT), Razor Pages, Flutter JWT, xUnit, Hangfire, SignalR, GitHub Actions

- Çift kayıt defteri: bakiye kolonu yok; bakiye ledger net’idir (`LedgerPair.NetOf`). `UPDATE Balance` yok.
- Aynı `Idempotency-Key` ikinci HTTP’de **409 Conflict**; timeout retry ikinci kesinti yapmaz. Ledger + outbox **aynı SQL transaction**.
- Sahte BankGateway REST ve SOAP (aynı sözleşme). Razor cookie + Flutter JWT **tek SQL**. SignalR diğer istemciyi yeniler — ikinci kasa değil.
- Lisanslı e-para / Papara rakibi değil. Canlı HTTPS TASK-16 (Portal publish profile sende).

**Flutter yüzü (04 paket):** JWT istemci; Hive/Firestore bakiye yok; Firebase en fazla `app_meta/ping`.

---

## English (same facts)

**ClearPay — ASP.NET Core 8 wallet demo (2026)**

- Double-entry ledger: no balance column; balance is `LedgerPair.NetOf`. No `UPDATE Balance`.
- Same `Idempotency-Key` → **409 Conflict**. Ledger + outbox in **one SQL transaction**.
- Mock BankGateway REST+SOAP. Razor cookie + Flutter JWT, one SQL. SignalR live refresh is not a second ledger.
- Not licensed e-money. Not a Papara rival. Public HTTPS is TASK-16 (you click the publish profile).
