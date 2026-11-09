# API eşzaman — mobil ve web aynı kasa

Karar: [`TARTISMA.md`](TARTISMA.md) **T-071**. Git/masa öğreticisi bu dosya değil: [`ESZAMANLI.md`](ESZAMANLI.md).

**Kilit:** Bakiye SignalR’da yok. Hub yalnız “yenile” der. Para SQL ledger’dadır (`GET /api/wallet`). Firestore / Hive / `UPDATE Balance` yok. 9. ekran yok.

```
Flutter POST /api/transfers  →  ITransferExecutor (çift kayıt + 409)  →  SQL commit
                                                                      →  SignalR WalletChanged { reason, correlationId }
Tarayıcı cookie /hubs/wallet  →  GET yok; Razor sayfa reload
Flutter JWT  /hubs/wallet     →  GET /api/wallet
```

Site cookie, uygulama JWT. İki protokol, **bir** `ClearPay` veritabanı.

---

## Senin yapacakların (API)

Kod ajanın işi. Aşağıdakiler **senin tıkların**. Secret git’e koyma. Azure hesabı ajan açmaz.

### 1. API’yi çalıştır

1. Docker Desktop açık → repo kökünde `docker compose up -d` (SQL).
2. Visual Studio `ClearPay.slnx` F5 **veya**  
   `dotnet run --project src/ClearPay.Web --launch-profile http`
3. Tarayıcı: http://localhost:5153/giris — `ERR_CONNECTION_REFUSED` = Kestrel kapalı, SignalR hatası değil.
4. Sağlık: http://localhost:5153/api/health → `{ "status": "ok" ... }`

### 2. Swagger ile JWT al

1. http://localhost:5153/swagger
2. `POST /api/token` — body:

```json
{ "email": "admin@clearpay.test", "password": "Deneme123" }
```

3. Dönen `access_token` kopyala.
4. **Authorize** → `Bearer {token}` (kelime `Bearer` + boşluk + token).
5. `GET /api/wallet` dene. 401 = token yok/yanlış; 200 = aynı kasayı okudun.

### 3. Para yazan her POST’ta `Idempotency-Key`

Header zorunlu. Aynı anahtar ikinci kez → **409**, ikinci kesinti yok.

| İşlem | Yol | Header |
|--------|-----|--------|
| Havale | `POST /api/transfers` | `Idempotency-Key: {yeni-guid}` |
| Yükle | `POST /api/topup` | aynı |
| Çek | `POST /api/withdraw` | aynı |

Swagger’da bu header’ı her denemede **yeni Guid** yap. Telefonda uygulama zaten Guid üretir.

### 4. Canlı kanal (SignalR) — ayrı kasa değil

- Adres: `http://localhost:5153/hubs/wallet`
- **Site:** giriş cookie’si yeter; ekstra token yok.
- **Flutter:** JWT. Kütüphane WebSocket’e `?access_token=` ekler (sen elle yapıştırmazsın).
- Olay adı: `WalletChanged`. Gövde örnek: `{ "reason": "transfer", "correlationId": "..." }` — **amount yok**.
- Hub 401: token yok veya cookie yok. Para API’si çalışıyorsa kasa sağlamdır; hub’ı sonra düzelt.

Negotiate (kanıt, tarayıcı DevTools veya Swagger dışı):

`POST /hubs/wallet/negotiate?negotiateVersion=1`  
`Authorization: Bearer {token}`  
→ 200 + `connectionToken`. Bearer yoksa **401**.

### 5. CORS (yalnız Flutter **web** veya başka origin)

Native Android/Windows Origin göndermez; CORS şart değil.

Canlıda `appsettings` / Azure App Settings:

```
Cors__Origins__0 = https://senin-flutter-web-kokun
```

Lokal development: `localhost`, `127.0.0.1`, `10.0.2.2` zaten açık. Yeni origin uydurma.

### 6. Flutter’ın API kökü

Emülatör: `http://10.0.2.2:5153`  
Windows / iOS sim: `http://localhost:5153`  
Canlı: `https://<app>.azurewebsites.net` — URL’yi ajan yazmaz; TASK-16 sende.

İsteğe `--dart-define=CLEARPAY_API=https://...` (site ayaktayken).

### 7. Kanıt (iki istemci aynı anda)

1. Tarayıcıda giriş → özet açık bırak.
2. Telefonda / Windows Flutter aynı hesap → havale veya yükle.
3. Web bakiyesi **kendiliğinden** yenilenmeli (sayfa reload). Olmazsa F5; hub kopmuştur, kasa yine SQL’dedir.
4. Tersi: web’den havale → Flutter özet güncellenir (pull-to-refresh yedek).

### 8. Yapma

- İkinci veritabanı / Firestore bakiye / Hive `balance`
- Hub’a tutar gömmek (kaynak `GET /api/wallet`)
- Papara / FAST / gerçek POS
- Azure SignalR Service **şimdi** (lokal hub yeter). Ölçek sonra; hesabı sen açarsın, ajan açmaz.
- JWT signing key’i git’e koymak. Canlıda App Settings: `Jwt__SigningKey`

### 9. Azure (TASK-16, sonra)

Hub aynı App Service’te. Ayrı mikroservis yok. `az login` + `.\infra\deploy.ps1` sende. Açık HTTPS URL olmadan “canlı eşzaman” iddiası yok.
