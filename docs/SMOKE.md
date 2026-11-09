# SMOKE — web + Flutter Android

Demo cüzdan. Lisanslı e-para / FAST / Papara değil. Sahte gateway. Bakiye = SQL `LedgerPair.NetOf`; `UPDATE Balance` yok. Hive’da bakiye yok.

Otomasyon: repo kökünde `dotnet test -c Release`; `mobile/clearpay` içinde `$env:TEMP='D:\ClearPay\tmp'; $env:TMP='D:\ClearPay\tmp'; flutter test`. CI her ikisini de çalıştırır. Bu sayfa **elle tıklama**.

Kök: `D:\ClearPay\clearpay`. Site ve emülatör **aynı** SQL’e JWT/cookie ile gider.

## 0. Siteyi aç

1. Visual Studio F5 ve `dotnet run` **birlikte değil** (MSB3027).
2. `dotnet run --project src\ClearPay.Web --launch-profile http`
3. http://localhost:5153/giris — **200**. `/api/health` — ClearPay, redis/rabbit `off` (lokal factory dışı Compose’suz).

Demo: `admin@clearpay.test` / `Deneme123`. TC (demo): `10000000146` (Mernis yok).

## 1. Web (Razor)

| # | Yol | Beklenen |
|---|-----|----------|
| 1 | `/giris` | E-posta + TC (demo) + 4 dil. Şifremi unuttum **yok**. |
| 2 | `/kayit` | Ad, e-posta, telefon, Bireysel/Kurumsal. Kayıt → **0,00 ₺**. |
| 3 | `/` özet | Bakiye, bu ay giden/gelen, son hareket. Masthead. Worldcard yok. |
| 4 | `/havale` | Alıcı + tutar. Aynı işlemi iki kez gönder → **409**, bakiye bir kez düşer. |
| 5 | `/yukle-cek` | Yükle sahte gateway. `TIMEOUT` hesabı → ledger kesinleşmez. |
| 6 | `/hareketler` | Filtre + dekont. Örnek fiş (seeder): `aaaaaaaa-bbbb-4ccc-8ddd-eeeeeeee0001`. |
| 7 | `/dekont` | Correlation id; PDF `%PDF`. |
| 8 | `/admin` | Yalnız Admin. Dondur / kuyruk. |
| 9 | `/kartlar` | Canlı CSS önizleme. PAN/CVV SQL’de yok. «Bu karttan cüzdana yükle» → `/yukle-cek`. |

Girişsiz korumalı yollar `/giris`’e gider. Dil şalteri TR/EN/DE/FR metni değiştirir. Hub `WalletChanged` **sayfayı yenilemez** (çek-yenile / F5 yeter).

## 2. Flutter Android (emülatör)

Chrome/PWA yok. Windows masaüstü ayrı: SignalR yok, 8 sn JWT poll (T-098). Android **hub** `10.0.2.2:5153`.

```powershell
cd D:\ClearPay\clearpay\mobile\clearpay
$env:TEMP = 'D:\ClearPay\tmp'
$env:TMP = 'D:\ClearPay\tmp'
flutter run -d emulator-5554
```

Site `:5153` açık olsun. Emülatör API tabanı `http://10.0.2.2:5153` (localhost değil).

| Adım | Beklenen |
|------|----------|
| Splash → Bireysel | Giriş; TC (demo) sekmesi. |
| TC `10000000146` + `Deneme123` | JWT; özet bakiyesi SQL. Hive uyarısı yok. |
| E-posta `admin@clearpay.test` | Aynı cüzdan (cookie web ile aynı kişi). |
| Özet | Hızlı işlemler; FAST→Havale; QR al/öde. Yapı Kredi / World / Jet QR yok. |
| Çekmece | Özet, Havale, Yükle/Çek, Hareketler, Dekont, Admin (admin JWT), Çıkış. Kartlarım **yok** (3D web). |
| Canlı | Hub kırmızı hata yok. Web’de yükleme sonrası telefonda taze net (çek-yenile veya hub). |
| Havale çift | İkinci deneme 409 metni; ikinci kesinti yok. |
| Yükle TIMEOUT | 202; dekont yok. |
| Firestore | Girişte ping satırı. Tutar/şifre yok. Kasa hâlâ JWT. |
| 4 dil | English / Deutsch / Français kromu değişir. |

Hot restart hub değişikliğinden sonra yetmez; uygulamayı durdur-aç.

## 3. Fail sayılır

- Negatif bakiye veya `Wallet.Balance` kolonu.
- İkinci 200 ile çift havale.
- Flutter’da yerel bakiye (Hive/SQLite/MySQL).
- Firestore’a tutar yazmak.
- `flutter run -d chrome`.
- 10. ekran / satıcı paneli / gerçek POS.

Azure açık URL: TASK-16 (Halil `az login`). Bu smoke lokaldir.
