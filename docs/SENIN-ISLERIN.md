# Senin işlerin

Kod ve TASK sırası ajanın işi. Sen hesap, makine ve sırları tutarsın.

- Baştan oku: [`KRONIK.md`](KRONIK.md)
- Nedenler: [`OGRENME.md`](OGRENME.md)
- Ödeme nasıl: [`ODEME-SENIN.md`](ODEME-SENIN.md)
- Azure (şimdi hesap açma): [`CANLI.md`](CANLI.md)

## Her gün

1. **Visual Studio** — `C:\Users\clt\Projects\clearpay\ClearPay.slnx` (zaten bir kez açıldıysa aynı dosya).
2. **Docker Desktop** açık tut. SQL için repo kökünde: `docker compose up -d`.
3. **Lokal kontrol** — http://localhost:5153 — sol menü, sonra giriş / kayıt.
4. **GitHub** hazır: `HalilMertDeveli/clearpay` (public, `main`). Yeni hesap açma.
5. **Secret / şifre git’e koyma** (connection string, JWT, Gmail App Password, Azure SQL).
6. **İleride Azure** — aboneliği **sen** açarsın; ajan talimat yazar, hesabı açmaz.
7. **DNS / domain** — senin panelin.
8. **Canlıda App Settings** (TASK-16) — sen yapıştırırsın: Azure SQL connection string, JWT. Ödeme kuruluşu KEY yok.
9. **Kod yazmak / hangi TASK** — senin işin değil. Komut: «sıradaki işi yap» / «devam».
10. **Search Console / GA4 / Ads** — canlı URL sonrası sen açarsın; harcama yok. “Ucuz havale” yazma.

---

## Canlı + Google (PR)

Playbook: [`PR.md`](PR.md). **Dürüst:** “havale” / “Papara”da #1 olmayız (ücretli tüketici araması + lisans).

- **Canlı URL** — Azure’u **sen** açarsın ([`CANLI.md`](CANLI.md)). Hedef `https://clearpay.azurewebsites.net`, sonra özel domain, HTTPS. TASK-15 Actions yeşil → TASK-16.
- **Google** — Search Console doğrulaması **sen**; sitemap/robots/meta ajan ([`SEO.md`](SEO.md)). Title: `ClearPay — ASP.NET Core cüzdan demo`. `/giris` ince kopya indeks olmasın.
- **Üst sıra (gerçekçi)** — `ClearPay ASP.NET`, `idempotent wallet .NET`, repo, senin adın + ClearPay. README İngilizce, bir LinkedIn/Medium, GitHub topics.
- **Launch sırası** — 1 yeşil build 2 Azure URL 3 Search Console 4 LinkedIn 5 isteğe Ads ([`ADS.md`](ADS.md); başlıkta Demo, “ucuz havale” yok).

---

## Ödeme yöntemi — senin işin

ClearPay’de ödeme **Papara başvurusu değil**. Sahte banka + çift kayıt. Adımlar: [`ODEME-SENIN.md`](ODEME-SENIN.md).

### Sen yapmazsın

Gerçek banka / Papara / iyzico / FAST başvurusu. POS, 3D Secure, lisans. Gerçek para yatırma. Ads’te “ucuz havale / Papara alternatif”.

### Sen yaparsın (nasıl)

1. Demo olduğunu bil — sahte `BankGateway`. Ekrana “gerçek havale” diye bakma.
2. Docker Desktop → `docker compose up -d`. Site: VS F5 veya `dotnet run --project src/ClearPay.Web --launch-profile http` → http://localhost:5153
3. TASK-03+: kayıt → giriş → özet 0 ₺. TASK-06+: havale; aynı gönderi iki kez → **409**, bakiye çift düşmez.
4. TASK-07: yükle/çek’te sahte **timeout**; kuyruk kaydı kalsın.
5. Canlıda: Azure’u sen açarsın (`CANLI.md`). App Settings’e SQL + JWT. **Ödeme KEY yok.**
6. Mülakat 2 dk: neden 409, transaction, outbox (`OGRENME.md` / `FARK.md` / `KRONIK.md` §13).
7. Google Ads’te “ucuz havale / Papara alternatif” **yazma**.
