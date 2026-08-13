# Google / Apple ile giriş — senin işin

Kod (buton, callback, Identity) **Coder’ın**. Senin işin yalnızca: **OAuth istemcisi açmak**, Client ID/secret’ı **user-secrets** (lokal) veya Azure App Settings (canlı) içine yapıştırmak. **Git’e koyma.**

Ajan Google Cloud / Apple Developer hesabı açmaz, Client ID üretmez, secret sormaz.

Mevcut Gmail: **`halilmertdeveliii@gmail.com`**. Yeni Google hesabı yok.

---

## Sen yapmazsın

- Giriş sayfasına buton / Razor yazmak
- `appsettings.json` içine Client Secret koymak
- Secret’ı commit / push / sohbette yapıştırmak
- Yeni Google / Apple ID uydurmak
- Apple için ücretli Developer hesabını ajanın kartıyla almak

---

## 1) Google (lokal demo — asıl iş)

1. [Google Cloud Console](https://console.cloud.google.com/) — bu Gmail.
2. Proje seç veya “ClearPay demo” diye bir proje oluştur (ücretsiz).
3. **APIs & Services → OAuth consent screen**  
   - User type: **External**  
   - App name: `ClearPay` (Demo)  
   - Test users: kendi Gmail’ini ekle (Production yayınlama şart değil).
4. **Credentials → Create credentials → OAuth client ID → Web application**  
   - Name: `ClearPay local`  
   - **Authorized JavaScript origins:** `http://localhost:5153`  
   - **Authorized redirect URIs:** `http://localhost:5153/signin-google`  
   HTTPS launch profile kullanırsan aynı path’i `https://localhost:<port>/signin-google` olarak da ekle.
5. **Client ID** ve **Client secret** kopyala. Ekranda bir daha tam görünmez; kaybetme. Git’e yazma.

Lokal yapıştırma (PowerShell, repo değil **Web projesi**):

```powershell
cd C:\Users\clt\Projects\clearpay\src\ClearPay.Web
dotnet user-secrets init
dotnet user-secrets set "Authentication:Google:ClientId" "BURAYA-CLIENT-ID"
dotnet user-secrets set "Authentication:Google:ClientSecret" "BURAYA-CLIENT-SECRET"
```

`secrets.json` Windows’ta kullanıcı klasöründedir (`%APPDATA%\Microsoft\UserSecrets\...`). Repo’da durmaz; `.gitignore` zaten user-secrets’ı kapsar. **Kontrol:** `git status` bu değerleri göstermez.

Site: http://localhost:5153 `/giris` — Coder butonu koyunca “Google ile giriş” çalışır. Secret yoksa buton hata verir veya gizlenir; o Coder işi.

Canlı (TASK-16, URL gelince): aynı Client’a redirect ekle  
`https://<app>.azurewebsites.net/signin-google`  
Portal → App Settings (değer git’e yok):

| Ad | Ne |
|----|----|
| `Authentication__Google__ClientId` | OAuth Client ID |
| `Authentication__Google__ClientSecret` | OAuth Client secret |

---

## 2) Apple (isteğe bağlı — zor)

Apple Sign In **ücretli** Apple Developer Program ister (yıllık). Yoksa **atla**; Google yeter (demo + mülakat). Ajan hesap açmaz.

Varsa: Identifiers → Services ID, Sign in with Apple, return URL  
`http://localhost:5153/signin-apple`  
Sonra user-secrets (Coder aynı isimleri kullanır):

```powershell
cd C:\Users\clt\Projects\clearpay\src\ClearPay.Web
dotnet user-secrets set "Authentication:Apple:ClientId" "com.xxx.xxx"
dotnet user-secrets set "Authentication:Apple:TeamId" "TEAMID"
dotnet user-secrets set "Authentication:Apple:KeyId" "KEYID"
dotnet user-secrets set "Authentication:Apple:PrivateKey" "-----BEGIN PRIVATE KEY-----..."
```

Private key dosyasını da git’e koyma.

---

## Kontrol listesi

1. Google Cloud’da Web client + redirect `http://localhost:5153/signin-google`
2. `dotnet user-secrets set` (yukarı) — `appsettings.json` değil
3. `git status` temiz; secret yok
4. Coder bitince `/giris` → Google dene
5. Canlıda App Settings + canlı redirect URI

Checklist özeti: [`SENIN-ISLERIN.md`](SENIN-ISLERIN.md). Kod: Coder. Ödeme KEY yok (`ODEME-SENIN.md`) — OAuth secret ödeme kuruluşu değildir.
