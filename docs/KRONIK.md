# Kronik — ClearPay’i baştan oku

Bu dosya **öğrenme defteridir**, git log değildir. Her bölüm üç şey söyler: **ne yaptık**, **neden**, **sen ne öğrenmelisin**.

Kısa “neden böyle”: [`OGRENME.md`](OGRENME.md). Checklist: [`SENIN-ISLERIN.md`](SENIN-ISLERIN.md). Ödeme (nasıl): [`ODEME-SENIN.md`](ODEME-SENIN.md). Ürün: [`SPEC.md`](SPEC.md). Ajan sırası: [`CALISMA-PLANI.md`](CALISMA-PLANI.md).

Repo: `C:\Users\clt\Projects\clearpay` · Solution: `ClearPay.slnx` · Site: http://localhost:5153

---

## 1. LED’den ayrı repo

**Ne yaptık.** ClearPay kendi GitHub reposunda. LED teknik destek sitesine ödeme / cüzdan yazılmadı.

**Neden.** LED başka ürün, başka SPEC. Ödeme LED kapsamı dışı. CV’de tek ürün anlatılsın diye karışık repo yok.

**Sen ne öğrenmelisin.** Yazılımın sınırı SPEC’tir. “Aynı makinede” aynı ürün demek değildir.

---

## 2. GitHub public `main`

**Ne yaptık.** [HalilMertDeveli/clearpay](https://github.com/HalilMertDeveli/clearpay), **public**, dal `main`. Yeni hesap yok.

**Neden.** Mülakatçı klonlayabilsin. Gizli repo = “gösteremem”. Secret git’e konmaz.

**Sen ne öğrenmelisin.** Portföy açık olur; sır App Settings’tedir. Ajan hesap açmaz.

---

## 3. TASK-01 — önce kâğıt, sonra kod

**Ne yaptık.** `SPEC.md`, `PLAN.md`, `TASKS.md`, `AGENTS.md`. Kod yoktu.

**Neden.** 409, çift kayıt, outbox kilitlendikten sonra ekran yazılır. Tersi: güzel form, arkada `UPDATE Balance`.

**Sen ne öğrenmelisin.** Tek seferde tek TASK. Komutun: «sıradaki işi yap» / «devam». TASK seçmek senin işin değil.

---

## 4. TASK-02 — ev (iskelet)

**Ne yaptık.** .NET 8, C# 12. Clean Architecture: Domain / Application / Infrastructure / Web. Razor + API **tek host**. Sol menü: Özet, Havale, Yükle/Çek, Hareketler (Admin gizli). Navy `#1B2A4A`, Bootstrap yok. Docker SQL ayakta; **uygulama henüz bağlanmıyordu**. xUnit: sayfalar **200**.

**Neden.** Sıra **ev → kilit → para motoru**. Tek host: 409 ve outbox tek SQL transaction’da kanıtlanır. SQL gün 1 Compose’da olsun ki TASK-04 takılmasın.

**Sen ne öğrenmelisin.** Domain kural tutar, Web HTTP tutar. “SQL var = bakiye var” değil.

---

## 5. TASK-03 — kilit (Identity) — WIP

**Ne yaptık / durum.** Giriş + kayıt + boş cüzdan (0,00 ₺) **yapılıyor**. Cookie Identity, rol `Musteri`. JWT yok (TASK-06). Identity SQLite (`App_Data`); ledger SQL ayrı. `TASKS.md`’de Done sayma — Coder bitince kayıt → giriş → özet 0 ₺.

**Neden.** Önce tarayıcı kimliği. Cookie = site; JWT = JSON API sonra.

**Sen ne öğrenmelisin.** Kontrol: http://localhost:5153 — menü, sonra giriş/kayıt.

---

## 6. Domain ledger — defter var, havale API yok

**Ne yaptık.** Domain’de cüzdan, `LedgerEntry`, idempotency, audit, outbox tipleri. Çift kayıt (`LedgerPair`). Bakiye kolonu yok; bakiye = satır net’i (`NetOf`). `UPDATE Balance` yok. **409 henüz HTTP değil** — `POST /api/transfers` TASK-06.

**Neden.** Para kuralı PageModel’de olursa UI bozulunca defter bozulur. Outbox fikri gün 1; worker TASK-11.

**Sen ne öğrenmelisin.** Bakiye hesaplanmış net’tir. İade = ters kayıt. “409 kodu yok” ≠ unutuldu; sıra bilinçli.

---

## 7. SOLID + Application portları

**Ne yaptık.** `ARCHITECTURE.md`. Portlar: `IBankGateway`, `IWalletReader`, `ITransferExecutor`. **PageModel’de ledger/havale yok.**

**Neden.** DIP: Web sahte bankayı `new` etmez. REST ve SOAP aynı arayüz.

**Sen ne öğrenmelisin.** Havale Web’de hesaplanmaz; port üzerinden biter.

---

## 8. Azure — plan var, hesap yok

**Ne yaptık.** [`CANLI.md`](CANLI.md): App Service Linux + Azure SQL, West Europe. **Publish yok. Abonelik açılmadı.**

**Neden.** TASK-16, yeşil Actions (TASK-15) ve senin aboneliğinden sonra. Ajan hesap açmaz.

**Sen ne öğrenmelisin.** Abonelik, RG, App Settings (SQL + JWT) senin. Değer git’e yok. Şimdi kart çıkarma.

---

## 9. SEO / Ads — demo, gerçek banka değil

**Ne yaptık.** `SEO.md`, `ADS.md`. Her metinde **Demo**. Ads yalnızca canlı URL sonrası; harcama yok.

**Neden.** “Ucuz havale / Papara alternatif” yalan ve lisans riski.

**Sen ne öğrenmelisin.** SC / GA4 / Ads hesabını sen açarsın, URL gelince. Şimdi kampanya yok.

---

## 10. FARK / SATIS — Papara yerine geçmeyiz

**Ne yaptık.** `FARK.md`: tercih = mutabakat defteri. `SATIS.md`: pitch / CV. Lisanslı cüzdan değiliz.

**Neden.** Papara ile özellik yarışı kaybedilir. Fark: 409, çift kayıt, outbox.

**Sen ne öğrenmelisin.** “Rakip Papara” deme. De: demo cüzdan; çift tıklamada ikinci kesinti yok. One-liner: **ClearPay — demo cüzdan, sahte banka.**

---

## 11. Senin işlerin ve ödeme yöntemi

**Ne yaptık.** [`SENIN-ISLERIN.md`](SENIN-ISLERIN.md). Ödeme nasıl: [`ODEME-SENIN.md`](ODEME-SENIN.md).

**Neden.** “Ödeme” deyince insan Papara başvurusu sanır. Burada ödeme = sahte BankGateway + ledger.

**Sen ne öğrenmelisin.** Yapmazsın: gerçek banka / Papara / iyzico / FAST, POS, 3DS, lisans, gerçek para. Yaparsın: Docker + :5153; kayıt/giriş; (TASK-06) havale iki kez → 409; (TASK-07) sahte timeout; Azure’da ödeme KEY yok; Ads’te “ucuz havale” yazma.

---

## 12. Ajan kadrosu ve HANDOFF

**Ne yaptık.** Orchestrator TASK seçer. Coder Razor. Payments ledger. Architect port. Tester test. Deploy Compose/Azure talimatı. Designer / SEO / Sales docs. Defter: [`HANDOFF.md`](HANDOFF.md) (append; silme).

**Neden.** Aynı `.cshtml`’e iki yazar girmez. LED’e dokunulmaz.

**Sen ne öğrenmelisin.** Sen kontrol edersin; kodu ajan yazar. TASK-03 bitmeden havale API yok.

---

## 13. Mülakat üçlüsü (2 dakika)

Kod henüz tam kanıtlamıyor (409 = TASK-06, outbox worker = TASK-11). Nedenini söyle.

**Neden 409?** Aynı `Idempotency-Key` = aynı niyet. İkinci 201 = iki kez kesinti. 409 = işlendi.

**Neden transaction?** −, +, idempotency, audit, outbox tek commit. Bakiye `UPDATE` ile düzeltilmez.

**Neden outbox?** Timeout + retry. Önce DB (ledger + outbox satırı), sonra yayın.

Detay: [`OGRENME.md`](OGRENME.md) · [`FARK.md`](FARK.md).

---

## Sıradaki okuma sırası

Hepsi `docs/` altında, VS’de yukarıdan aşağı:

1. **KRONIK.md** — bu dosya
2. **SENIN-ISLERIN.md** — senin checklist
3. **ODEME-SENIN.md** — ödeme: ne yapmazsın / nasıl dene
4. **OGRENME.md** — kısa nedenler + üçlü
5. **SPEC.md** — ekran + para kuralları
6. **CALISMA-PLANI.md** — faz + test kapısı
7. **PLAN.md** / **TASKS.md** — kabul ve kuyruk
8. **ARCHITECTURE.md** — katman, cookie/JWT
9. **CANLI.md** — Azure (şimdi hesap açma)
10. **FARK.md** + **SATIS.md** — neden biz
11. **SEO.md** + **ADS.md** — demo sesi
12. **DEPLOY.md** — lokal Compose
13. **AGENTS.md** — roller
14. **README.md** — İngilizce özet

İstersen: `HANDOFF.md` (ajan defteri). `YONETICI-RAPORU.md` (anlık durum).

Kod yazmak yok. Anlamadığın bölümü sohbette sor.
