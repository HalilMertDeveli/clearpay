# Yönetici çalışma listesi — Alipay envanteri → ClearPay

**Tarih:** 2026-08-13. **Kim:** Yönetici (Orchestrator-doc). **TARTISMA:** T-039.

Bu belge **boşluk listesi**. ClearPay, Alipay veya Papara **rakibi değildir**. Lisanslı cüzdan, süper uygulama, POS ağı iddiası yok. Demo dijital cüzdan: 8 ekran kilit (`docs/SPEC.md`).

**Kilit:** Alipay’de olan her şey ClearPay’e yazılmaz. Layout benzerliği (T-038, Coder Razor) ≠ Alipay ürünlerini kopyalamak. `brand.css` / `_Layout` bu belgede dokunulmaz.

**Dürüst durum (2026-08-13):**
- TASK-05 Done (canlı özet). **Havale HTTP 409 yok** — xUnit skip, TASK-06.
- Açık Azure URL **yok**. Şablon `docs/CANLI.md`; `azurewebsites.net` uydurulmaz.
- Lokal: http://localhost:5153 `/giris`

---

## Kaynak (tüketici + web)

| Kaynak | Ne görüldü |
|--------|------------|
| [alipay.com](https://www.alipay.com/) | Hızlı giriş; alt menü: 余额宝, 芝麻信用, 蚂蚁微贷, 网商银行, açık platform, satıcı merkezi, 口碑 |
| [Wikipedia: Alipay](https://en.wikipedia.org/wiki/Alipay) | Cüzdan, QR, P2P, fatura, kart borcu, bilet, yemek, taksi, sigorta, Yu’e Bao, Huabei/Jiebei, Tourpass, yüz ödeme, sağlık kodu |
| [open.alipay.com](https://open.alipay.com/) | Mini program, IoT, mini oyun, yaşam hesabı, web/App ödeme, AI ödeme |
| [alipayplus.com](https://www.alipayplus.com/) | Sınır ötesi kasa, QR/NFC, remittance, tax refund, Super App, Voyager, Trust Score |
| [global.alipay.com](https://global.alipay.com/platform/site/ihome) | Satıcı portalı (tüketici evi değil); QR; Antom’a yönlendirme |
| [antom.com](https://www.antom.com/) | Küresel acquiring, 300+ yöntem, BNPL, Shopify eklentisi, fraud, split payout |

---

## Sayım

| Kova | Adet | Anlam |
|------|------|--------|
| Katalog | **57** | Aşağıdaki numaralı Alipay özellikleri |
| **Q1 / zaten TASK** | **9** | SPEC 8 ekrana düşen tüketici analogları (`docs/TASKS.md`) |
| **later / Q2** | **5** | SPEC’te aday veya aynı ekrana eklenti; **şimdi iş yok** |
| **never** | **43** | Lisans, Papara/Alipay rakibi, POS, gerçek banka, 9. ekran (onaysız) |

9 + 5 + 43 = 57.

SOAP, Hangfire, Swagger, CI, Azure **Alipay kopyası değil**; bizim kanıt sırası. Aşağıda ayrı tablo.

---

## Envanter (Alipay → SPEC)

Kova: **Q1** = TASKS’te var (Done veya Todo). **Q2** = SPEC “şimdi değil”. **never** = yasak veya 9. ekran.

### Q1 — zaten TASK (9)

| # | Alipay (kısa) | ClearPay | TASK | Durum |
|---|---------------|----------|------|--------|
| 1 | Hesap girişi | Ekran 1 `/giris` | TASK-03 | **Done** |
| 2 | Hesap oluşturma | Ekran 2 `/kayit` | TASK-03 | **Done** |
| 3 | Cüzdan bakiyesi | Ekran 3 `/` ledger net | TASK-05 | **Done** |
| 4 | Kişiye para gönder (P2P) | Ekran 4 `/havale` | TASK-06 | **Todo** — API yok; 409 **skip** |
| 5 | Bakiyeye yükle (bankadan) | Ekran 5, sahte gateway | TASK-07 | Todo — gerçek banka değil |
| 6 | Bakiyeden çek | Ekran 5 | TASK-07 | Todo |
| 7 | İşlem geçmişi / cüzdan ekstresi | Ekran 6 | TASK-09 | Todo — kamu faturası değil |
| 8 | Dekont / işlem kanıtı | Ekran 7, correlation id | TASK-09 | Todo |
| 9 | Risk / hesabı dondur | Ekran 8 Admin | TASK-10 | Todo — süper-app paneli değil |

### Q2 — later (5) — şimdi başlama

| # | Alipay (kısa) | ClearPay |
|---|---------------|----------|
| 10 | Satıcı tahsilatı / mağaza kasa | SPEC: **Satici** ayrı ekran yok; Q2 adayı |
| 11 | Kapalı devre QR (alıcı seç; POS ağı değil) | Ekran 4 eklentisi, menü maddesi değil; kullanıcı onayı |
| 12 | Gizlilik / veri özeti | Layout chrome adayı, 9. ekran değil |
| 13 | Ölçek: cache / kuyruk broker | TASK-12 Compose var, uygulama bağlı değil; canlı hesap yok |
| 14 | Kapalı devre kampüs/yemek cüzdanı | `FARK.md` — gerçek iş, Q1 ekranı yok |

### never (40)

| # | Alipay (kısa) | Neden never |
|---|----------------|-------------|
| 15 | Yu’e Bao (para piyasası fonu) | Lisans / menkul kıymet |
| 16 | Huabei (taksit / BNPL kredi) | Tüketici kredisi |
| 17 | Jiebei (mikro kredi) | Kredi |
| 18 | Zhima / Sesame Credit | Kredi skoru |
| 19 | Ant mikro kredi (蚂蚁微贷) | Kredi platformu |
| 20 | MYbank (网商银行) | Gerçek banka |
| 21 | AlipayHK lisanslı cüzdan | Yabancı lisans |
| 22 | QR mağaza ödeme ağı (tara / kod göster) | Gerçek POS |
| 23 | NFC / Alipay Tap / BlueTap | POS / donanım |
| 24 | GlassPay (AR ödeme) | Donanım |
| 25 | Yüz tanıma / Smile to Pay | Biyometri + POS |
| 26 | Yüz ödemede güzellik filtresi | — |
| 27 | Taobao/Tmall checkout | Pazaryeri |
| 28 | Emanet (escrow) | Pazaryeri |
| 29 | Gerçek kart bağlama / PCI | Kart acquiring |
| 30 | Kredi kartı borcu ödeme | Banka/kart lisansı |
| 31 | Fatura: su / elektrik / gaz | 9. ekran + kurum |
| 32 | Mobil hat yükleme | 9. ekran |
| 33 | Otobüs / tren bileti | Süper uygulama |
| 34 | Trafik cezası / aidat / kablo TV / harç | Kamu ödeme |
| 35 | Yemek siparişi | Süper uygulama |
| 36 | Taksi / araç çağırma | Süper uygulama |
| 37 | Sinema / KTV / zincir POS | POS ağı |
| 38 | Sigorta satışı | Lisans |
| 39 | Dijital kimlik belgesi saklama | e-devlet |
| 40 | Mini programlar | Süper uygulama |
| 41 | Mini oyunlar | — |
| 42 | Yaşam hesabı / içerik (生活号) | Medya |
| 43 | Servis ekle (üçüncü parti ızgara) | Süper uygulama; 9. ekran |
| 44 | IoT / donanım SDK | — |
| 45 | Tourpass / döviz ön yükleme | FX + lisans |
| 46 | Sınır ötesi turist ödeme (Alipay+) | Acquiring ağı |
| 47 | Vergi iadesi (tax refund) | Gümrük |
| 48 | Uluslararası remittance | Havale lisansı |
| 49 | A+ Rewards / kupon / “ucuz öde” | Papara/Alipay GTM; Ads yasağı |
| 50 | Alipay+ Trust Score | Skor ürünü |
| 51 | Voyager seyahat ajanı | Süper uygulama |
| 52 | Antom küresel kasa / Shopify / 300 yöntem | PSP; iyzico yarışı |
| 53 | Pazaryeri split / abonelik tahsilatı | Satıcı motoru (SPEC’te yok) |
| 54 | Sağlık kodu (COVID) | Kamu sağlık |
| 55 | QR birlikte çalışabilirlik (WeChat / UnionPay) | Ulusal ödeme rayı |
| 56 | AI ajan ticareti / GenAI Cockpit | Alipay+ ürünü |
| 57 | Açık platform (üçüncü parti App ödemesi) | SDK pazarı |

---

## Bizim kanıt sırası (Alipay ürünü değil)

Bunlar mülakat omurgası. Alipay envanterinden **ekran doğmaz**.

| TASK | Ne | Durum | Kim |
|------|-----|--------|-----|
| TASK-04 | Ledger şema | **Done** | — |
| TASK-08 | SOAP = REST sözleşmesi | Todo | Coder |
| TASK-11 | Outbox + Hangfire | Todo | Payments + Coder |
| TASK-13 | 409/ledger test, skip kalksın | Todo | Tester |
| TASK-14 | README CV + Swagger | Todo (görsel README var; Swagger Done değil) | Coder |
| TASK-15 | GitHub Actions | **Done** | — |
| TASK-16 | Azure App Service + SQL, **açık URL** | Todo — **şimdi değil** | Deploy + **kullanıcı** |

---

## Yönetici çalışma listesi (sen tıklarsın / sen söylersin)

Sıra **tek ürün TASK** kuralına uyar. Alipay özelliği diye atlama yok. Komut «sıradaki işi yap» = bir sonraki Todo.

1. **Şimdi ürün: TASK-06.** Söyle: «sıradaki işi yap». **Kim:** Payments (`ITransferExecutor`, 409) + Coder (ekran 4, `POST /api/transfers`) + Tester (409 Fact skip kalkar). Yu’e Bao / QR POS / 9. ekran yok. Havale 409’u Done sayma.

2. **TASK-06 kanıtı.** İki Musteri. Aynı `Idempotency-Key` ikinci kez → HTTP **409**, bakiye çift düşmez. Skip duruyorsa mülakatta “kanıtladım” yok.

3. **TASK-07.** Yükle/çek + sahte REST gateway (başarı / timeout). Timeout’ta ledger **kesinleşmez**. **Kim:** Coder + Payments. Gerçek IBAN/FAST yok.

4. **TASK-08.** SOAP, aynı `IBankGateway`. **Kim:** Coder.

5. **TASK-09.** Hareketler + filtre + dekont (correlation id). **Kim:** Coder. Alipay “bills” burada **cüzdan ekstresi**, su faturası değil.

6. **TASK-10.** Admin: dondur, başarısız kuyruk, audit. **Kim:** Coder.

7. **TASK-11.** Outbox + Hangfire. **Kim:** Payments + Coder.

8. **TASK-12.** Uygulamayı Compose Redis/Rabbit’e bağla (lokal). Canlı Azure Redis hesabı açma. **Kim:** Coder.

9. **TASK-13.** Ledger + 409 + freeze yeşil, skip yok. **Kim:** Tester.

10. **TASK-14.** İngilizce README CV üçlüsü + Swagger 409 örneği. **Kim:** Coder. “Alipay/Papara rakibiz” yok (`FARK.md`).

11. **TASK-16 sonra, sen.** `az login` → [`CANLI.md`](CANLI.md) / `.\infra\deploy.ps1`. Ajan Portal açmaz. URL tarayıcıda açılmadan Done yok.

12. **Sen — Docker (T-037).** Windows restart → Docker Desktop → `powershell -File scripts/docker-up.ps1`. Ajan reboot etmez. Native MySQL `:3306` silinmez.

13. **Sen — Google OAuth.** [`GIRIS-SOSYAL.md`](GIRIS-SOSYAL.md). Secret **user-secrets**, git yok. Apple isteğe bağlı. Alipay hesabı bağlama değil.

14. **UI Alipay düzeni (T-038).** Coder OWN: `_Layout` / `brand.css` / Özet ızgarası. Bu liste ekran eklemez. Palet navy; Alipay `#1677FF` kopyalanmaz.

15. **Yapılmaz.** Huabei, Jiebei, Yu’e Bao, mini program, yüz ödeme, POS/NFC, Antom kasa, Tourpass, kurum faturası, Ads’te “Alipay/Papara alternatifi”. SPEC listesi tartışmasız genişlemez.

16. **Q2 notu (şimdi değil).** Satıcı paneli ancak sen SPEC’i açarsan. Kapalı devre QR = ekran 4, yeni menü değil.

---

## Yasak

- Alipay/Papara **rakip** pazarlama; “onlarda var bizde de” ile 9. ekran.
- Layout benzerliğini ürün kopyası saymak (T-038 ≠ T-039).
- TASK-06 bitmeden 409 Done.
- Azure URL icat.
- Bu belgeden `src/` / Razor. Razor yalnızca Coder.

---

## İlgili

- Ekran kilidi: [`SPEC.md`](SPEC.md)
- Sıra: [`TASKS.md`](TASKS.md) · [`PLAN.md`](PLAN.md)
- İnsan tıklamaları: [`SENIN-ISLERIN.md`](SENIN-ISLERIN.md)
- Neden rakip değiliz: [`FARK.md`](FARK.md)
- Karar: [`TARTISMA.md`](TARTISMA.md) T-039
- Rapor: [`YONETICI-RAPORU.md`](YONETICI-RAPORU.md)
