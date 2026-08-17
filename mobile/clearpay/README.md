# ClearPay mobile (Flutter)

<p align="center">
  <a href="../../README.md">English (repo)</a>
  · <a href="../../README.tr.md">Türkçe</a>
  · <a href="../../README.de.md">Deutsch</a>
  · <a href="../../README.fr.md">Français</a>
</p>

<p align="center">
  <img src="https://img.shields.io/badge/Flutter-3.41-02569B?logo=flutter" alt="Flutter">
  <img src="https://img.shields.io/badge/.NET-8_API-512BD4?logo=dotnet" alt=".NET 8 API">
  <img src="https://img.shields.io/badge/SQL_Server-same_ledger-CC2927?logo=microsoftsqlserver" alt="SQL Server">
  <img src="https://img.shields.io/badge/UI-TR_default-1B2A4A" alt="Turkish UI">
  <a href="../../LICENSE"><img src="https://img.shields.io/badge/license-MIT-blue.svg" alt="MIT"></a>
</p>

<p align="center"><b>Demo — sahte banka gateway.</b> Lisanslı e-para kuruluşu değil. Papara / FAST değil. Hive’da bakiye yok.</p>

**Aynı kişi, aynı para.** Siteye giren Halil burada da giriş yapar, havale atar, yükler, dekont açar. Cookie yok: **JWT**. Kasa C# `ITransferExecutor` / `IWalletReader`. Bu klasör ikinci defter değildir.

Parent repo: [ClearPay](../../README.md). Workspace: `ClearPay.code-workspace` (site + bu klasör). `ClearPay.slnx` Flutter içermez.

---

## Picture of the build

```mermaid
flowchart LR
  app[Flutter TR UI]
  api[JSON /api JWT]
  ports[Application ports]
  sql[SQL Server ledger]
  app -->|Idempotency-Key| api
  api --> ports
  ports --> sql
```

Navy `#1B2A4A`. Footer her ekranda: **Demo — sahte banka gateway.** Relational schema (no `Wallet.Balance`): root [`README.md`](../../README.md) section **Relational schema (SQL Server)**.

| Must hold | Must not hold |
|-----------|----------------|
| JWT, pull-to-refresh, Guid `Idempotency-Key` | `UPDATE Balance`, Hive/SQLite/MySQL cüzdan, WebView cookie |

---

## What you can tap today

Site must be up: [http://localhost:5153](http://localhost:5153). Same eight operations as Razor.

| Operation | In the app | API |
|-----------|------------|-----|
| Sign in | Giriş — **E-posta** / **TC (demo)** | `POST /api/token` |
| Mode | Splash → **Bireysel** / **Kurumsal** (üye iş yeri değil) | JWT `account_kind`; SQL `AccountKind` |
| Register | Hesap oluştur | `POST /api/register` (+ `accountKind`) |
| Summary | Özet (hamburger + sol çekmece, bakiye kartı, kısayol ızgarası; live hub + pull-to-refresh) | `GET /api/wallet` + `/hubs/wallet` |
| Transfer | Havale + onay; **QR yapıştır** | `POST /api/transfers` + `Idempotency-Key` → 201 / **409** |
| Top-up / withdraw | Yükle / Çek + demo kart | `POST /api/topup` / `withdraw` |
| Movements | Hareketler + filtre | `GET /api/movements` |
| Receipt | Dekont (kopyala + **PDF indir**) | `GET /api/receipts/{id}` + `GET /api/receipts/{id}/pdf` |
| Admin | Admin sekmesi (rol) | `/api/admin/*` |

Dev: `admin@clearpay.test` / `Deneme123`. Demo TC (Mernis değil): `10000000146` → aynı admin e-posta, hâlâ `POST /api/token`. **Örnek dekont** (Development seed, çift kayıt TopUp 25 ₺): `aaaaaaaa-bbbb-4ccc-8ddd-eeeeeeee0001`. Site: [http://localhost:5153/dekont/aaaaaaaa-bbbb-4ccc-8ddd-eeeeeeee0001](http://localhost:5153/dekont/aaaaaaaa-bbbb-4ccc-8ddd-eeeeeeee0001) → **PDF indir**. Flutter: giriş → Hareketler → Dekont (aynı satır) → **PDF indir** (`GET /api/receipts/{id}/pdf`, uygulama içi sahte PDF yok). Android emulator base: `http://10.0.2.2:5153`. Windows / iOS: `http://localhost:5153`. Optional `--dart-define=CLEARPAY_API=...` only — **no MySQL dart-define**. Host MySQL (`MySQL84`, `ConnectionStrings:MySql`) is tools/sidecar; this app does not add a `mysql` package or store balances (T-061 / T-077). Same path as the website: JWT → C# → SQL Server.

QR: **QR al** Özet’te `clearpay://pay?to={email}` üretir (`qr_flutter`). **QR öde** yük yapıştırır / e-posta yazar, Havale formunu doldurur, mevcut onay + `POST /api/transfers`. Kamera eklentisi yok (Windows symlink). FAST kiremiti Havale’dir — TCMB FAST değil. Piyasalar / Fatura / Kredi **Park — demo değil**.

Bireysel/Kurumsal **Firebase’e yazılmaz** (T-068). SQL Identity `AccountKind` + yerel `%LOCALAPPDATA%\ClearPay\account_kind.txt`. Firestore/Auth ikinci kasa yok. `firebase_core` yalnız T-065 bootstrap.

## Interview (three sentences)

1. Same `Idempotency-Key` is the same intent: replay is **409**, the wallet is not charged twice.
2. Balance is `LedgerPair.NetOf` on SQL — this app only **GET**s the DTO.
3. Cookie is for the website; the phone carries **Bearer**, not `ClearPay.Auth`.

---

## Run (cmd)

Flutter **Command Prompt** (not PowerShell). `flutter doctor` on this machine is green (3.41.9).

```bat
cd /d C:\Users\clt\Projects\clearpay
dotnet run --project src/ClearPay.Web --launch-profile http
```

Second cmd:

```bat
cd /d C:\Users\clt\Projects\clearpay\mobile\clearpay
flutter doctor
flutter build windows
flutter run -d windows
```

Store listing / HTTPS live URL: TASK-16 (you click `az login`). CI stays `dotnet test`.

## Firebase (client only)

Ledger stays SQL Server. Firebase is **not** Auth/Firestore wallet.

1. Same Gmail: `halilmertdeveliii@gmail.com` — [Firebase console](https://console.firebase.google.com/) → add project (or reuse ClearPay Google Cloud).
2. Command Prompt:

```bat
npm install -g firebase-tools
firebase login
cd /d C:\Users\clt\Projects\clearpay\mobile\clearpay
tool\configure-firebase.cmd
```

3. Until that runs, the app still logs in with JWT (`firebase_core` skips). Do not put a second balance in Firestore.


## License

[MIT](../../LICENSE) © 2026 Halil Mert Develi
