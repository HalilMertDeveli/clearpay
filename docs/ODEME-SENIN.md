# Ödeme yöntemi — senin işin (nasıl)

ClearPay **demo cüzdan**; sahte `BankGateway`. Gerçek para yok.

Kronik: [`KRONIK.md`](KRONIK.md) §11. Checklist: [`SENIN-ISLERIN.md`](SENIN-ISLERIN.md). Azure: [`CANLI.md`](CANLI.md). 409: [`OGRENME.md`](OGRENME.md).

## Sen yapmazsın

- Gerçek banka / Papara / iyzico / FAST başvurusu
- POS, 3D Secure, ödeme kuruluşu lisansı
- Gerçek IBAN’dan para yatırma / çekme
- Ads’te “ucuz havale” / “Papara alternatif”
- Repo’ya ödeme KEY’i veya connection string

## Sen yaparsın (nasıl)

1. **Demo olduğunu bil.** Ekrandaki havale sahte gateway. Footer: Demo — sahte banka gateway.

2. **Lokal.** Docker Desktop açık. Repo kökü: `docker compose up -d` (SQL). Site: VS F5 (`ClearPay.slnx`) veya `dotnet run --project src/ClearPay.Web --launch-profile http` → http://localhost:5153

3. **Akış (TASK-03+).** Kayıt → giriş → özet **0,00 ₺**. TASK-06 gelince başka kullanıcıya havale. **Aynı gönderiyi iki kez:** ikincisi **409**, bakiye çift düşmemeli.

4. **Yükle/çek (TASK-07).** Sahte **timeout** kutusunu dene. Ledger kesinleşmesin; kuyruk/outbox kaydı kalsın (worker TASK-11).

5. **Canlı (TASK-16, sonra).** Azure aboneliğini **sen** açarsın (`CANLI.md`). App Settings: Azure SQL + JWT. **Ödeme kuruluşu KEY yok.**

6. **Mülakat (2 dk).** Ezber değil: neden 409, transaction, outbox (`OGRENME.md` / `FARK.md` / `KRONIK.md` §13).

7. **Ads.** Canlı URL yokken kampanya yok. “Ucuz havale / Papara alternatif” yazma (`ADS.md`).
