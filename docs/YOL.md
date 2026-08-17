# YOL — ClearPay ne işe yarar, nereye gider

Karar: [`TARTISMA.md`](TARTISMA.md) **T-059** (yol) + **T-060** (senaryo kataloğu). Wedge: [`FARK.md`](FARK.md). Gelir mertebesi: [`GELIR.md`](GELIR.md) (T-013). Mülakat: [`IK.md`](IK.md) / [`SATIS.md`](SATIS.md). Canlı tık: [`CANLI.md`](CANLI.md). Görev listesi: [`TASKS.md`](TASKS.md).

**Kilit:** Lisanslı ödeme kuruluşu değiliz. Ekrandaki ₺ demo (sahte BankGateway). Q1 başarı = mülakatta 409’u tarayıcıda göstermek. Q1 başarı **ciro değil**.

---

## Ne işe yarar

Kurumsal **.NET** mülakatında (Intertech, Softtech, Bileşim, TAV, İGA, uni BT, sanayi, Turkcell .NET) şunu kanıtlar:

- Çift kayıt defteri (bakiye = ledger net; `UPDATE Balance` yok)
- Aynı havale `Idempotency-Key` → **409**; ikinci kesinti yok
- Ledger + outbox **aynı SQL transaction**; timeout kaybettirmez
- Sahte banka REST **ve** SOAP, aynı sözleşme

15 saniye: *Defter sizin. Bakiye güncellendi demiyoruz. Demo.*

Papara / FAST / BDDK iddiası yok. Java ilanına bu repo ile girilmez.

---

## Şu an

TASK-01…15 **Done** (ledger, havale 409, gateway, hareket/dekont, admin, outbox, Redis/Rabbit lokal, test, README, CI, UI).

**Tek açık ürün TASK:** TASK-16 — Azure App Service + Azure SQL, tarayıcıda HTTPS. T-104: host kilit; zip yok. Ajan Portal / `az login` / DNS açmaz.

Lokal: repo **`D:\ClearPay\clearpay`**. Docker Desktop → `docker compose up -d` → `dotnet run --project src/ClearPay.Web --launch-profile http` → http://localhost:5153/giris  
5153 `ERR_CONNECTION_REFUSED` = Kestrel kapalı, SQL hatası değil. Canlı tık: [`CANLI.md`](CANLI.md).

---

## Sıra (üç kademe)

```
Q1 kod (bitti) → TASK-16 URL (Halil tıklar) → kariyer kapısı → Q2 ticari (park)
Kendi e-para lisansı: kapalı (40 / 105 milyon TL özkaynak)
```

### 1. Kanıt — TASK-16 (bu hafta, sen)

Portal site duruyor (T-104). `.\infra\deploy.ps1` **çalıştırma** (RG `ClearPay_group` / `ClearPay` ezilir).

1. Portal **Get publish profile** → GitHub secret `AZURE_WEBAPP_PUBLISH_PROFILE` (sohbete yapıştırma). Variable `AZURE_WEBAPP_NAME` = `ClearPay` (ajan koydu).
2. Portal startup `dotnet ClearPay.Web.dll`; HTTPS Only On; SQL + JWT App Settings.
3. Actions **Azure deploy** on **`main`** (bu feature dalı zip atmaz).
4. Tarayıcı: https://clearpay-eecuaqc7c5ehbmb5.canadacentral-01.azurewebsites.net/api/health sonra `/giris`. Production seed yok — `/kayit`.

Ayrıntı: [`CANLI.md`](CANLI.md). Ads yok. Footer: demo.

### 2. Kariyer — ilk nakit (SPEC hedefi)

URL tarayıcıda **açık olduktan sonra:**

- GitHub repo Settings → Website = canlı kök (localhost koyma). About/topics: [`PAZARLAMA.md`](PAZARLAMA.md).
- LinkedIn **taslak B** (sen yayınlarsın):

> ClearPay **demo** canlı: {CANLI_KOK}  
> ASP.NET Core 8 cüzdan demosu — ledger, idempotency 409, sahte banka REST+SOAP. Gerçek havale / IBAN yok.  
> Kod: https://github.com/HalilMertDeveli/clearpay  
> Demo — sahte banka gateway.

URL yokken taslak A (yalnızca repo) serbest; “yayındayız” yok.

**15/30 dk prova** (lokal veya canlı): [`IK.md`](IK.md). Göster: kayıt → özet → havale iki kez → **409**. Üçlü: 409 / transaction / outbox.

**Kapı (sen başvurursun; ajan İK paneli açmaz):**

| Firma | Neden ClearPay |
|-------|----------------|
| Intertech | Banka .NET; ledger / 409 / outbox |
| Softtech | Kurumsal .NET, para izi |
| Bileşim | Ödeme altyapısı .NET |
| TAV / İGA | Kurumsal .NET; FIDS değil |
| Uni BT / sanayi / Turkcell .NET | İlan C# ise |

Hayır: Trendyol/THY/Garanti **Java**, ASELSAN **C++**. Adresler (randevu, soğuk kapı değil): [`GELIR.md`](GELIR.md) §5.

CV üç satır: [`README.md`](../README.md) *CV bullets* — kelime kelime. Papara klonu yazma.

### 3. Ticari Q2 — park (T-059)

White-label (lisans alıcıda) veya kapalı devre (kampüs/yemekhane/OSB, 6493 sınırlı ağ) **şimdi kodlanmaz**. Avukat + kullanıcı onayı olmadan müşteri parası yok. Satıcı paneli = 9. ekran = SPEC onayı şart. IBAN’a serbest çekim e-para lisansı ister.

Kendi lisansı: TCMB 30 Haz 2026 özkaynak fatura ÖK 20 milyon / diğer ÖK 40 milyon / e-para 105 milyon TL — **bu projede kapalı**.

---

## 12 ay (resmi)

| Dönem | Hedef | Kim |
|-------|--------|-----|
| Bu hafta | TASK-16 HTTPS | Sen: publish-profile secret + Portal startup ([`CANLI.md`](CANLI.md)) |
| URL+1 | GitHub Website, LinkedIn B, 15 dk prova | Sen yayın; script `IK.md` |
| 1–3 ay | 5–10 .NET kapısına CV+repo | Sen başvurur |
| Q2 | Tek tesis PoC veya white-label konuşma | Avukat + yeni TARTISMA |

---

## Masalar ne hesapladı (T-060)

Dört ajan ayrı OWN ile konuştu; Orchestrator **en robust** tek yolu kilitledi (ledger + 409/outbox > kolay UI).

| Masa | Hesap | Çatışma |
|------|--------|---------|
| **Product** | 90 gün = URL + 8 ekran kanıtı. 12 ay hâlâ demo. 3 yıl tavan = kariyer + belki bir C veya D sözleşmesi. Papara olmaz. | Sales URL görünce landing/satıcı isteyecek — hayır. |
| **Architect** | TASK-16 sonrası “bitti” = tek process, Azure SQL, Hangfire in-process, kasa SQL. Redis/Rabbit kopya (Q2 string). | Product 9. ekranı “küçük ek” derse chrome ≠ SPEC ekranı. |
| **Payments** | 8 ekran içinde unfreeze / tarih / onay / 409→dekont güvenli. İkinci kart bakiyesi, Visa, FAST, Kafka dual-write, `UPDATE Balance` kırılır. | Architect para için mikroservis önerirse dual-write = red. |
| **Sales / İK** | Gelecek = Halil’in 12–36 ay kapı kalitesi. Skor B (maaş) ayrı, A (lisanslı şirket) = 0, C/D avukatsız yok. | URL’den önce satıcı paneli mülakatı geciktirir — veto. |

**Kazanan 12 ay:** TASK-16 HTTPS → LinkedIn + 409 tıklaması → Yol **B**. Tek host durur. 9. ekran yok.

---

## Olası gelecek senaryoları

Olasılık: **izle** (şimdi doğru yol) / **park** (onay + avukat sonra) / **kapalı** (bu projede yok).

### A. Kariyer ve ticari (kim öder)

| # | Senaryo | Olasılık | Ne olur | Şart | SPEC |
|---|---------|----------|---------|------|------|
| 1 | **Ledger’lı .NET maaşı (Yol B)** | **İzle — yüksek** | Intertech / Softtech / Bileşim kapısı | HTTPS URL + canlı 409 + README=CV | 8 kalır |
| 2 | **TAV / İGA kurumsal .NET** | İzle — orta–yüksek | Para disiplini (freeze, audit); FIDS değil | URL + admin dondur/çöz | 8 kalır |
| 3 | **Üni / sanayi / Turkcell .NET** | İzle — orta | İlan C# ise; kampüs sahne, ürün iddiası değil | URL + dürüst demo | 8 kalır |
| 4 | **White-label defter (Yol C)** | Park — düşük–orta, 18+ ay | Lisanslı ÖK/banka inovasyon: fon onlar, defter biz | URL + “lisans sizde” + tüzel kişilik | Tenant seçici = onay |
| 5 | **Tek tesis kapalı devre (Yol D)** | Park — düşük | Kampüs/fabrika yemek; IBAN’a serbest çekim yok | Avukat 6493 sınırlı ağ | Kiosk/satıcı = 9+ onay |
| 6 | **Kendi e-para / ÖK lisansı (Yol A)** | **Kapalı** | 40–105 milyon TL özkaynak; MASAK | Bu repo’nun işi değil | — |

### B. Ürün yüzeyi (kullanıcı ne görür)

| # | Senaryo | Olasılık | Ne görünür | Risk |
|---|---------|----------|------------|------|
| 7 | **Mülakat sahnesi** | İzle | 8 ekran, sahte yükle/çek, dekont correlation, freeze | “Üretim cüzdanı” sanılması |
| 8 | **Canlı demo URL** | İzle (Halil tıklar) | Aynı site Azure’da; footer Demo | URL yokken Sales abartır |
| 9 | **8 ekran içi cilâ** | İzle (T-056/057 landed) | Onay adımı, unfreeze, tarih, last4, Beni hatırla | Yeni menü maddesi yok |
| 10 | **Satıcı paneli (Q2)** | Park | Tahsilat / üye iş yeri | SPEC 9. ekran; POS hikâyesine kayar |
| 10b | **Flutter JWT istemci (T-061)** | İzle — Q2.1 | Aynı 8 ekran, aynı SQL; pull-to-refresh yedek; canlı chrome `/hubs/wallet` (T-071, tutar yok) | Hive bakiye / 9. ekran yok |
| 11 | **Kampanya landing / “eksiksiz cüzdan”** | Kapalı (şimdi) | Ads, QR, KYC sayfası | Wedge sulanır; Ads yasağı |

### C. Teknik (kasa ve host)

| # | Senaryo | Olasılık | Robust | Neden |
|---|---------|----------|--------|--------|
| 12 | **Tek host + Q2 Redis/Rabbit bind** | **İzle — varsayılan** | En yüksek | Kasa SQL; cache/broker kopya; Hangfire yedek |
| 13 | **PSP adapter (`IBankGateway` arkası)** | Park | Orta | Timeout’ta ledger yok; SDK kasa olamaz |
| 14 | **API’yi ayrı host** | Kapalı (12 ay) | Düşük | İki deploy; tek SQL tx hikâyesi zayıflar |
| 15 | **Kafka kaynak-gerçek** | Kapalı | Kırılır | Outbox aynı tx kuralı ölür (dual-write) |
| 16 | **Kart için ikinci bakiye** | Kapalı | Kırılır | İki net; last4 ipucu vault değil |
| 17 | **K8s / mikroservis ağı** | Kapalı (Q1–12 ay) | Düşük | Para ağ gecikmesine taşınır; kanıt üretmez |

### D. İşe alımı öldüren (bilerek yok)

Papara klonu / “ucuz havale”. BDDK/TCMB “lisanslıyız”. FAST/BOA/gerçek IBAN/POS. “Müşteri fonu aldım.” Sahte banka uygulaması (şube, BankaX). `UPDATE Balance`. PageModel’de ledger.

---

## İzlenecek yol (tek sıra)

```
1. Halil TASK-16 HTTPS          ← ajan URL uydurmaz
2. 409’u tarayıcıda tıkla       ← aynı havale iki kez
3. LinkedIn + 5–10 .NET kapısı  ← Yol B, ilk nakit = maaş
4. 8 ekranı dondur              ← yeni ekran TASK’ı yok
5. Q2 C/D yalnızca yazılı talep ← TARTISMA + (satıcı ise) senin onayın
```

Ajan: Azure hesabı, DNS, Ads, lisans başvurusu, 9. ekran, LED reposu **yok**.

---

## Bilerek yok

Papara yarışı, FAST/POS, BDDK iddiası, LED’e ödeme, Kafka/K8s, satıcı paneli (şimdi), Ads “ucuz havale”, ajanın Azure URL uydurması.

---

## Yasak (bu belge)

- `src/` yazmak
- TASK-16’yı URL yokken Done yapmak
- Q2 ekranı / lisans başvurusu başlatmak

---

## Doğrulama 2026-08-17 (Cursor plan)

Plan dosyası değiştirilmedi. OWN durur: T-059 `YOL.md`, T-013 `GELIR.md`. Halil tık listesi: [`SENIN-ISLERIN.md`](SENIN-ISLERIN.md) *Yol haritası*. `infra/deploy.ps1` + `infra/main.bicep` var. `az` yok. TASK-16 Todo.
