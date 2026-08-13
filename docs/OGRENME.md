# Öğrenme defteri — neden böyle?

**Baştan oku (asıl kronik):** [`docs/KRONIK.md`](KRONIK.md) — her bölüm ne yaptık / neden / sen ne öğrenmelisin.

Bu dosya SPEC/PLAN’ı tekrar etmez; kısa “neden böyle”. Ürün: `docs/SPEC.md`. Sıra: `docs/PLAN.md` / `docs/TASKS.md` / `docs/CALISMA-PLANI.md`.

Checklist: [`SENIN-ISLERIN.md`](SENIN-ISLERIN.md). Ödeme nasıl: [`ODEME-SENIN.md`](ODEME-SENIN.md). Google/Apple OAuth (sen): [`GIRIS-SOSYAL.md`](GIRIS-SOSYAL.md).

---

## Senin yapman gerekenler

Kod yazmak ve “hangi TASK?” seçmek **senin işin değil**. Ajan `docs/TASKS.md` sırasını izler. Senin tarafın: hesap, makine, sır, canlı yayın.

1. **Visual Studio** — solution’ı aç: `C:\Users\clt\Projects\clearpay\ClearPay.slnx` (bir kez açıldıysa tekrar arama; aynı yol).
2. **Docker Desktop** açık dursun. SQL Server için repo kökünde: `docker compose up -d`. (Identity şimdilik SQLite; ledger SQL’e TASK-04’te bağlanır.)
3. **Lokal kontrol** — site: http://localhost:5153 — sol menüye bak; sonra giriş / kayıt dene.
4. **GitHub** — repo zaten `HalilMertDeveli/clearpay` (public, `main`). Yeni GitHub hesabı açma.
5. **Sırları git’e koyma** — şifre, connection string, JWT imza anahtarı, Gmail App Password commit edilmez.
6. **İleride Azure** — aboneliği **sen** açarsın. Ajan hesap açmaz; talimat yazar (`docs/DEPLOY.md`).
7. **DNS / domain** — senin panelin (varsa). Ajan DNS kaydı basmaz.
8. **Canlı sırlar (TASK-16 civarı)** — Azure App Settings’e **sen** yapıştırırsın: Azure SQL connection string, JWT signing key, Gmail App Password (e-posta gerekirse). Repo’ya yazılmaz.

Kısa liste: [`SENIN-ISLERIN.md`](SENIN-ISLERIN.md). Ödeme (lisans yok, demo akış): [`ODEME-SENIN.md`](ODEME-SENIN.md). Kronik: [`KRONIK.md`](KRONIK.md).

---

## Neyi neden yaptık

### LED sitesinden ayrı repo

LED teknik destek başka bir ürün. Ödeme / cüzdan LED SPEC’inin dışında. CV’de **tek ürün** anlatılsın diye ClearPay kendi reposunda. LED’e özellik eklenmez.

### GitHub: `HalilMertDeveli/clearpay`, public, `main`

Mülakatçı klonlayabilsin. Gizli repo CV’de “gösteremem” olur. Yeni hesap yok; mevcut GitHub.

### Önce SPEC / PLAN / ajanlar, sonra kod (tek TASK)

Para kuralları (409, ledger, outbox) koddan önce kilitlendi. Aksi halde “hızlı ekran” sonra motoru bozar. Orchestrator **tek seferde tek TASK** alır — yarım havale + yarım Azure aynı anda yok.

### TASK-02 — ev (iskelet), henüz kilit ve para yok

Bilinçli sıra: **ev → kilit → para motoru**.

| Ne | Neden |
|----|--------|
| .NET 8, C# 12 | İlanlarda “.NET Core”; Framework 4.8 / Java yok |
| Clean Architecture: Domain / Application / Infrastructure / Web | Domain para kurallarını tutar; EF/HTTP dışarıda. Mülakatta katman anlatılır |
| Razor Pages + API **tek host** | Ayrı “API projesi” Q1 şişirirdi; aynı sitede cookie + JSON |
| Sol menü (Özet, Havale, Yükle/Çek, Hareketler) | SPEC ekran listesi sabit; Admin role gelene kadar gizli |
| Navy `#1B2A4A`, Bootstrap yok | Kurumsal, düz; mockup’a yakın, Figma kopyası değil |
| Docker SQL ayakta, uygulama henüz bağlanmıyordu | Compose gün 1; EF/ledger **TASK-04**. Ev hazır, kasa boş |
| xUnit + 200 smoke | “Sayfa 500 vermiyor” kanıtı; 409 testleri para motorunda |

### TASK-03 — kilit (Identity), JWT sonra

Site **cookie** (ASP.NET Identity). JSON API için JWT **sonra** (havale API’si TASK-06 civarı). Önce tarayıcıdan giriş; token’ı boş yere erken bağlamak kafa karıştırır.

Şu an: kayıt / giriş, boş cüzdan (0,00 ₺). Identity lokal **SQLite** (`App_Data`); SQL Server cüzdan defteri TASK-04.

### 409 / transaction / outbox henüz yok — bilinçli

Bunlar **para motoru** (TASK-04…11). Ev ve kilit bitmeden çift kayıt defteri yazmak, “ekran yokken 409” demek. Sıra bozulmaz: önce görünen site, sonra tablolar, sonra havale + 409, outbox işleyen TASK-11.

---

## Mülakat üçlüsü (SPEC ile aynı)

Kod bunları **henüz kanıtlamıyor**; TASK-06 ve TASK-11 bitince kanıtlar. Cümleler şimdiden net olsun:

**Neden 409?**  
Aynı `Idempotency-Key` = aynı niyet (çift tıklama, retry). İkinci HTTP 201 olursa cüzdan **iki kez** kesilir. 409 = “bu niyet zaten işlendi”; ikinci kesinti yok.

**Neden transaction?**  
Gönderenden −, alıcıya +, idempotency satırı, audit, outbox **tek SQL commit**. Biri ayrı commit olursa bakiye ile defter ayrılır. Bakiye “UPDATE Balance” ile düzeltilmez; ledger net’idir.

**Neden outbox?**  
Ledger yazıldı, HTTP timeout oldu, client tekrar dener. Mesajı commit’ten **önce** kuyruğa atarsan “banka/kuyruk kayboldu” olur. Önce DB (ledger + outbox satırı aynı transaction), **sonra** yayın. Timeout’ta kayıt kaybolmaz; worker tekrar dener.

---

## Dosya haritası

| Dosya | Ne işe yarar |
|-------|----------------|
| `docs/SPEC.md` | Ürün, ekran listesi, para kuralları — bozulmaz |
| `docs/PLAN.md` | Fazlar, TASK kabul kriteri |
| `docs/CALISMA-PLANI.md` | Tüm ajanların sıra + test kapısı (yönetici planı) |
| `docs/TASKS.md` | Kuyruk (Todo / Done) — ajan buradan iş alır |
| `docs/AGENTS.md` | Orchestrator / Architect / Coder / Payments / Tester / Deploy |
| `docs/DEPLOY.md` | Lokal Compose, Azure talimatı; hesap açma yok |
| `docs/KRONIK.md` | Baştan sona öğrenme (asıl okuma) |
| `docs/OGRENME.md` | Bu dosya: kısa nedenler |
| `docs/SENIN-ISLERIN.md` | Senin checklist |
| `docs/ODEME-SENIN.md` | Ödeme yöntemi — senin işin (nasıl) |
| `docs/TARTISMA.md` | Ekipler tartışır, sonra işlem (`src/` öncesi zorunlu) |
| `docs/HANDOFF.md` | Ajanların durum defteri (senin işin değil) |
| `README.md` | İngilizce özet, CV, nasıl çalıştırılır |

---

## Yol (makinen)

- Repo: `C:\Users\clt\Projects\clearpay`
- Solution: `ClearPay.slnx`
- Site: http://localhost:5153  
  `dotnet run --project src/ClearPay.Web --launch-profile http`
- SQL (ileride ledger): repo kökünde `docker compose up -d`

Kritik yol ajanın: **TASK-03** (giriş + boş cüzdan, WIP). Sen «sıradaki işi yap» / «devam» demen yeterli.
