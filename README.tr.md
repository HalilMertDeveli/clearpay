# ClearPay

<p align="center">
  <a href="README.md">English</a>
  · <b>Türkçe</b>
  · <a href="README.de.md">Deutsch</a>
  · <a href="README.fr.md">Français</a>
</p>

<p align="center">
  <a href="https://github.com/HalilMertDeveli/clearpay/actions/workflows/ci.yml"><img src="https://github.com/HalilMertDeveli/clearpay/actions/workflows/ci.yml/badge.svg" alt="CI"></a>
  <img src="https://img.shields.io/badge/.NET-8-512BD4?logo=dotnet" alt=".NET 8">
  <img src="https://img.shields.io/badge/Flutter-3.41-02569B?logo=flutter" alt="Flutter">
  <img src="https://img.shields.io/badge/SQL_Server-2022-CC2927?logo=microsoftsqlserver" alt="SQL Server">
  <img src="https://img.shields.io/badge/UI-TR%20%7C%20EN%20%7C%20DE%20%7C%20FR-1B2A4A" alt="Arayüz dilleri">
  <a href="LICENSE"><img src="https://img.shields.io/badge/license-MIT-blue.svg" alt="MIT"></a>
</p>

<p align="center"><b>Demo — yükleme için sahte gateway.</b> Lisanslı e-para kuruluşu değil. Papara / FAST / sahte perakende banka değil.</p>

**Tek cüzdan, iki istemci.** Aynı kişi sitede ve Flutter uygulamasında giriş yapar, havale atar, yükler, dekont açar. Tek SQL defter. Telefonda ikinci bakiye yok.

ASP.NET Core 8 **WePay benzeri cüzdan sitesi** ve **Flutter** JWT istemci ([`mobile/clearpay`](mobile/clearpay)). Tarayıcı Razor (cookie); uygulama JSON (JWT). Çift kayıt Domain’de — `Wallet` üzerinde **`Balance` kolonu yok**.

Ben Halil Mert Develi. Bunu Papara klonu diye değil, .NET mülakatında (Intertech / Softtech) savunabileceğim repo diye yazdım. Lisans MIT.

---

## Kurulan yapı

![Clean Architecture katmanları](docs/assets/clearpay-layers.svg)

Web ledger hesabı yapmaz. Özet sayfası `IWalletReader` sorar. Bugün adapter `SqlWalletReader`: bakiye = `LedgerPair.NetOf`, bu ay giden/gelen, son beş hareket, freeze rozeti. SQL Server kapalıysa site yine açılır — sıfırlar, 500 değil.

![Çift kayıt çifti](docs/assets/clearpay-ledger.svg)

```mermaid
flowchart TB
  subgraph clients [Same person]
    razor[Website Razor cookie]
    flutter[Flutter app JWT]
  end
  subgraph web [ClearPay.Web]
    pages[Razor Pages TR/EN/DE/FR]
    api[JSON API]
  end
  subgraph app [ClearPay.Application]
    reader[IWalletReader]
    exec[ITransferExecutor]
  end
  subgraph infra [ClearPay.Infrastructure]
    sql[SqlWalletReader + EF]
    id[Identity]
  end
  subgraph domain [ClearPay.Domain]
    pair[LedgerPair / LedgerEntry]
  end
  razor --> pages
  flutter --> api
  pages --> reader
  pages --> exec
  api --> reader
  api --> exec
  reader --> sql
  exec --> pair
  sql --> pair
```

| Katman | Proje | Ne tutar | Ne tutmaz |
|--------|-------|----------|-----------|
| UI + host | `ClearPay.Web` | Razor, cookie, dil çerezi, `:5153` | Ledger net, `UPDATE Balance` |
| Use case | `ClearPay.Application` | Portlar, DTO, FluentValidation | Connection string |
| Adapter | `ClearPay.Infrastructure` | EF SQL Server, Identity SQLite, gateway stub | Razor / CSS |
| Kural | `ClearPay.Domain` | `LedgerPair`, `Wallet` (bakiye alanı yok) | EF, HTTP, ASP.NET |

Bağımlılık **içe** bakar. Domain EF veya ASP.NET görmez.

---

## Bugün tıklanan

Sekiz işlem sitede ve uygulamada aynı. Site dilleri: **Türkçe (varsayılan), English, Deutsch, Français**. Flutter varsayılan Türkçe. 9. ekran değil.

| İşlem | Site | Flutter uygulama |
|-------|------|------------------|
| Giriş | [`/giris`](http://localhost:5153/giris) | Giriş — `POST /api/token` |
| Kayıt | [`/kayit`](http://localhost:5153/kayit) | Hesap oluştur — `POST /api/register` |
| Özet | [`/`](http://localhost:5153/) | Özet, pull-to-refresh — `GET /api/wallet` |
| Havale | [`/havale`](http://localhost:5153/havale) | Havale + onay — `POST /api/transfers` + `Idempotency-Key` |
| Yükle / Çek | [`/yukle-cek`](http://localhost:5153/yukle-cek) | Yükle / Çek — `POST /api/topup` / `withdraw` |
| Hareketler | [`/hareketler`](http://localhost:5153/hareketler) | Hareketler + filtre — `GET /api/movements` |
| Dekont | [`/dekont/{id}`](http://localhost:5153/hareketler) | Dekont — `GET /api/receipts/{id}` |
| Admin | [`/admin`](http://localhost:5153/admin) | Admin sekmesi (rol Admin) — `/api/admin/*` |

Dev: `admin@clearpay.test` / `Deneme123`. Havale **201** / tekrar **409**. OpenAPI: [http://localhost:5153/swagger](http://localhost:5153/swagger). Mobil README: [`mobile/clearpay/README.md`](mobile/clearpay/README.md). `GET /api/health` → `{ "status": "ok", "product": "ClearPay" }`.

Redis yalnızca özet DTO (~60s; para hareketinde invalidate). Kasa SQL Server. Rabbit `clearpay.outbox` (`ConnectionStrings:RabbitMq` varsa). **Açık Azure URL yok** — sen `az login` tıklarsın (`docs/CANLI.md`).

## Mülakat (üç cümle)

1. Aynı `Idempotency-Key` aynı niyet: ikinci HTTP **409 Conflict**; timeout retry ikinci kez kesmez.
2. Debit, credit, transfer, idempotency, audit ve outbox **tek SQL transaction**; `UPDATE Balance` yok — bakiye `LedgerPair.NetOf`.
3. Outbox satırı aynı transaction’da yazılır; timeout mesajı kaybettirmez. Hangfire (ve bağlıysa Rabbit) commit’ten sonra yayınlar.

---

## Çalıştırma

.NET 8 SDK. Canlı özet için Docker Desktop.

```bash
docker compose up -d
dotnet run --project src/ClearPay.Web --launch-profile http
```

[http://localhost:5153](http://localhost:5153). Aynı para Flutter’da (**cmd**):

```bat
cd /d mobile\clearpay
flutter doctor
flutter run -d windows
```

Android emülatör: `http://10.0.2.2:5153`. Identity için Docker gerekmez. SQL yoksa özet `0,00 ₺` kalır.

```bash
dotnet test
dotnet build ClearPay.slnx
```

Lokal SA şifresi `.env.example` içinde. Yalnız Docker. `.env` commit edilmez. Azure’da bu şifre yok.

SQL veri bind: `D:\ClearPay\data\mssql` (bu makine). Uygulama defteri **yalnızca SQL Server**. MySQL/Oracle compose yan servis; cüzdan veritabanı değil.

---

## Dizin

```
src/ClearPay.Domain           LedgerEntry, LedgerPair, Wallet (Balance yok)
src/ClearPay.Application      IWalletReader, ITransferExecutor, IBankGateway
src/ClearPay.Infrastructure   SqlWalletReader, EF SQL Server, Identity SQLite
src/ClearPay.Web              Razor + localization + MapControllers
mobile/clearpay               Flutter JWT istemci (.slnx’de yok)
tests/ClearPay.Tests          LedgerPair, mimari, SqlWalletReader, dil
docker-compose.yml            SQL Server 2022 — web uygulaması değil
ClearPay.slnx
```

---

## Yol (dürüst)

| Bitti | Sıradaki |
|-------|----------|
| TASK-01…15 — ekranlar, ledger, 409, gateway, outbox, Redis/Rabbit, test, Swagger | **TASK-16** — Azure App Service + Azure SQL (`az login` sen tıklarsın; URL uydurulmaz) |

CI `main` üzerinde `tests/ClearPay.Tests` restore + test eder.

---

## Belgeler

- [`docs/YOL.md`](docs/YOL.md) — ne işe yarar, nereye gider (önce kariyer; canlı URL TASK-16)
- [`docs/SPEC.md`](docs/SPEC.md) — ekranlar ve para kuralları (409, tek transaction, outbox)
- [`docs/ARCHITECTURE.md`](docs/ARCHITECTURE.md) — soğan katmanları, önce cookie sonra JWT
- [`docs/FARK.md`](docs/FARK.md) — mutabakat; Papara rakibi değil
- [`docs/SATIS.md`](docs/SATIS.md) — 15 saniye pitch
- [`docs/DEPLOY.md`](docs/DEPLOY.md) — Compose + `dotnet run`
- [`mobile/clearpay/README.md`](mobile/clearpay/README.md) — Flutter istemci (aynı sekiz işlem)
- Adım adım: [`docs/OTURUM-PLAN.md`](docs/OTURUM-PLAN.md) (bu repo, public). Aynı liste [Notion](https://www.notion.so/3bb31a8b18e4816bb34ffa405b4dec5d) — sayfada Share → Publish to web (Notion hesabı olmayan da okusun).

Canlı hedef: Azure App Service + Azure SQL (West Europe). Tıklanacak `azurewebsites.net` yok.

## Lisans

[MIT](LICENSE) © 2026 Halil Mert Develi
